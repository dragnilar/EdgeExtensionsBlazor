using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorEdgeNewTab.Constants;
using BlazorEdgeNewTab.Models;
using Microsoft.JSInterop;
using WebExtensions.Net.Bookmarks;
using WebExtensions.Net.Downloads;

namespace BlazorEdgeNewTab.Pages
{
    public partial class NewTab
    {
        private List<Image> _bingArchiveImages;
        private HttpClient _httpClient;
        private List<QuickLink> _quickLinks;
        private int imageArchiveIndex = -1;
        private BingImageOfTheDay ImageOfTheDay;
        private bool _searchVisible;
        private bool _quickLinksVisible; 
        private string SearchQuery { get; set; }
        public string ImageDownloadLink { get; set; }
        public string MuseumCardText { get; set; }
        public string MuseumCardText2 { get; set; }
        public string MuseumLink { get; set; }
        public string MuseumLink2 { get; set; }
        public SettingsMenu SettingsMenuNewTab { get; set; }

        protected override async Task OnInitializedAsync()
        {
            //NOTE - Do NOT get settings until the quick links are set up. The elements need to exist in the dom first or the
            //QuickLinksVisible property will cause a nasty exception because it's trying to work on an HTML element that isn't drawn yet.
            await Settings.LoadSettingsAsync(WebExtensions).ContinueWith(_ => SetUpQuickLinks());
            await GetBingImage();
            await GetBingImageArchive();
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                _searchVisible = SettingsMenuNewTab.SearchVisible;
                _quickLinksVisible = SettingsMenuNewTab.QuickLinksVisible;
            }
            return base.OnAfterRenderAsync(firstRender);
        }

        private async Task SetUpQuickLinks()
        {
            _quickLinks = new List<QuickLink>();
            var bookMarkNode =
                await WebExtensions.Bookmarks.Search(Settings.GetSettingValue(SettingsValues.QuickLinkBookMarkFolder));
            if (bookMarkNode == null) return;
            var quickLinkBookMarks = await WebExtensions.Bookmarks.GetChildren(bookMarkNode.First().Id);
            _quickLinks.AddRange(quickLinkBookMarks.Select(quickLinkBookMark => new QuickLink
            {
                QuickLinkTitle = quickLinkBookMark.Title,
                QuickLinkImageUrl = "chrome://favicon/size/64/" + quickLinkBookMark.Url,
                QuickLinkUrl = quickLinkBookMark.Url
            }));
            StateHasChanged();
        }

        private async Task GetBingImage()
        {
            try
            {
                Console.WriteLine("Getting Bing Image 🔍");
                var dtoGetResult = await GetImageOfDayDto();
                if (dtoGetResult.dto != null)
                {
                    ImageOfTheDay = dtoGetResult.dto.images[0];
                    if (ImageOfTheDay != null)
                        await ApplyImageOfDayAndCache(dtoGetResult.dto, dtoGetResult.serializeDto);
                    else
                        Console.WriteLine(
                            "We did not set the image, there was no first image in the json that came back. ☹");
                }
                else
                {
                    Console.WriteLine("The image collection was null, we probably screwed up deserializing the JSON ☹");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("We got an error. 😡");
                Console.WriteLine(e);
            }
        }

        private async Task ApplyImageOfDayAndCache(BingImageOfTheDayDto dto, bool serializeDto)
        {
            await SetBackgroundImage(ImageOfTheDay.url);
            UpdateMuseumCardForImageOfDay();
            StateHasChanged();
            if (serializeDto)
            {
                Console.WriteLine("Caching image of day data...");
                Settings.UpdateSetting(SettingsValues.ImageOfTheDayCache, JsonSerializer.Serialize(dto));
                Settings.UpdateSetting(SettingsValues.ReQueryImagesAfterTime,
                    DateTime.Now.ToString(CultureInfo.InvariantCulture));
                await Settings.SaveAsync();
            }
        }

        private async Task<(BingImageOfTheDayDto dto, bool serializeDto)> GetImageOfDayDto()
        {
            BingImageOfTheDayDto dto;
            var serializeDto = false;
            var reQueryTime = Convert.ToDateTime(Settings.GetSettingValue(SettingsValues.ReQueryImagesAfterTime)
                                                 ?? DateTime.Now.AddDays(-1).ToString(CultureInfo.InvariantCulture));
            if (Settings.GetSettingValue(SettingsValues.ImageOfTheDayCache) != null &&
                DateTime.Now.Date <= reQueryTime.Date)
            {
                Console.WriteLine("Getting image of the day data from cache...");
                dto = JsonSerializer.Deserialize<BingImageOfTheDayDto>(
                    Settings.GetSettingValue(SettingsValues.ImageOfTheDayCache));
            }
            else
            {
                var bingImageOfTheDayUrl =
                    "https://api.allorigins.win/raw?url=https%3A//www.bing.com/HPImageArchive.aspx%3Fformat%3Djs%26idx%3D0%26n%3D1%26mkt%3Den-US";
                _httpClient ??= new HttpClient();
                dto = await _httpClient.GetFromJsonAsync<BingImageOfTheDayDto>(bingImageOfTheDayUrl);
                serializeDto = true;
            }

            return (dto, serializeDto);
        }

        private async Task GetBingImageArchive()
        {
            var reQueryTime = Convert.ToDateTime(Settings.GetSettingValue(SettingsValues.ReQueryArchiveAfterTime)
                                                 ?? DateTime.Now.AddDays(-1).ToString(CultureInfo.InvariantCulture));
            if (Settings.GetSettingValue(SettingsValues.ImageArchiveCache) != null &&
                DateTime.Now.Date <= reQueryTime.Date)
            {
                Console.WriteLine("Getting image archive data from cache...");
                _bingArchiveImages =
                    JsonSerializer.Deserialize<List<Image>>(Settings.GetSettingValue(SettingsValues.ImageArchiveCache));
            }
            else
            {
                var dateString = $"{DateTime.Now.Year}{DateTime.Now.Month:00}{DateTime.Now.Day:00}_0700&";
                Console.WriteLine($"Querying bing with date string - {dateString}");
                var bingImageArchiveUrl =
                    $"https://api.allorigins.win/raw?url=https://www.bing.com/hp/api/v1/imagegallery?format=json&ssd={dateString}";
                _httpClient ??= new HttpClient();
                var dto = await _httpClient.GetFromJsonAsync<BingImageArchiveDto>(bingImageArchiveUrl);
                _bingArchiveImages = dto?.data.images.ToList();
                Console.WriteLine("Caching image archive data....");
                Settings.UpdateSetting(SettingsValues.ImageArchiveCache, JsonSerializer.Serialize(_bingArchiveImages));
                Settings.UpdateSetting(SettingsValues.ReQueryArchiveAfterTime,
                    DateTime.Now.ToString(CultureInfo.InvariantCulture));
                await Settings.SaveAsync();
            }
        }

        private async Task GetNextImage()
        {
            Console.WriteLine($"Getting next image 🖼, current image index is: {imageArchiveIndex}");
            if (imageArchiveIndex >= _bingArchiveImages.Count - 1)
            {
                imageArchiveIndex = -1;
                await SetBackgroundImage(ImageOfTheDay.url);
                UpdateMuseumCardForImageOfDay();
            }
            else
            {
                imageArchiveIndex++;
                await SetBackgroundImage(GetImageUrlForIndex(imageArchiveIndex));
                UpdateMuseumCardForArchiveImage();
            }
        }

        private async Task GetPreviousImage()
        {
            Console.WriteLine($"Getting previous image 🖼, current image index is: {imageArchiveIndex}");
            if (imageArchiveIndex == -1)
            {
                imageArchiveIndex = _bingArchiveImages.Count - 1;
                await SetBackgroundImage(GetImageUrlForIndex(imageArchiveIndex));
                UpdateMuseumCardForArchiveImage();
            }
            else if (imageArchiveIndex == 0)
            {
                imageArchiveIndex = -1;
                await SetBackgroundImage(ImageOfTheDay.url);
                UpdateMuseumCardForImageOfDay();
            }
            else
            {
                imageArchiveIndex--;
                await SetBackgroundImage(GetImageUrlForIndex(imageArchiveIndex));
                UpdateMuseumCardForArchiveImage();
            }
        }

        private void UpdateMuseumCardForImageOfDay()
        {
            MuseumCardText = ImageOfTheDay.title;
            MuseumLink = ImageOfTheDay.copyrightlink;
            MuseumCardText2 = ImageOfTheDay.copyright;
            MuseumLink2 = $"https://bing.com{ImageOfTheDay.quiz}";
        }

        private void UpdateMuseumCardForArchiveImage()
        {
            MuseumCardText = _bingArchiveImages[imageArchiveIndex].title;
            MuseumLink = $"https://bing.com{_bingArchiveImages[imageArchiveIndex].clickUrl}";
            MuseumCardText2 = _bingArchiveImages[imageArchiveIndex].description;
            MuseumLink2 = $"https://bing.com{_bingArchiveImages[imageArchiveIndex].clickUrl}";
        }

        private string GetImageUrlForIndex(int index)
        {
            var imageUrls = _bingArchiveImages[index].imageUrls;
            return string.IsNullOrWhiteSpace(imageUrls.landscape.wallpaper)
                ? imageUrls.landscape.highDef
                : imageUrls.landscape.wallpaper;
        }

        private async Task SetBackgroundImage(string Url)
        {
            var fullImagePath = "https://bing.com" + Url;
            await JS.InvokeVoidAsync("setBackgroundImage1", fullImagePath).ConfigureAwait(false);
            ImageDownloadLink = fullImagePath.Replace("&pid=hp", "");
            Console.WriteLine("Image set 😊");
        }

        private void Enter()
        {
            if (!string.IsNullOrWhiteSpace(SearchQuery)) FireSearchEngineQuery();
        }

        private Task FireSearchEngineQuery()
        {
            try
            {
                Console.WriteLine($"Performing a search with query : {SearchQuery}");
                // The code below always throws an exception; we are using JS Interop to work around this for now...
                // var searchProps = new SearchProperties {Query = SearchQuery};
                //await WebExtensions.Search.Query(new QueryInfo {Text = SearchQuery, Disposition = Disposition.CurrentTab, AdditionalData = null, TabId = null}).ConfigureAwait(false);
                JS.InvokeVoidAsync("runSearchQuery", SearchQuery).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine("We had an error doing the search query.");
                Console.WriteLine(e);
            }

            return Task.CompletedTask;
        }

        private string GetQuickLinkName(string quickLinkText)
        {
            if (!string.IsNullOrWhiteSpace(quickLinkText) && quickLinkText.Length > 25)
            {
                var returnValue = quickLinkText.Substring(0, 22) + "...";
                return returnValue;
            }

            return quickLinkText;
        }

        private async Task OnDownloadImageClick()
        {
            await WebExtensions.Downloads.Download(new DownloadOptions
            {
                Url = ImageDownloadLink,
                SaveAs = true,
                Filename = "Image.jpg"
            });
        }

        private async Task SaveNewQuickLinkClickHandler()
        {
            Console.WriteLine("Save was clicked on the new quick link modal.");
            await SetUpQuickLinks();
        }

        private async Task QuickLinksFolderChangedHandler()
        {
            Console.Write("Quick links folder was changed from settings menu.");
            await SetUpQuickLinks();
        }

        private void SearchVisibleChanged()
        {
            Console.WriteLine("Search visible changed");
            _searchVisible = SettingsMenuNewTab.SearchVisible;
        }

        private void QuickLinksVisibleChanged()
        {
            Console.WriteLine("Quick Links visible changed");
            _quickLinksVisible = SettingsMenuNewTab.QuickLinksVisible;
        }
    }
}