using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorLiquidSeperation.Constants;
using BlazorLiquidSeperation.Models;
using Microsoft.JSInterop;
using WebExtensions.Net.Downloads;

namespace BlazorLiquidSeperation.Pages
{
    public partial class NewTab
    {
        private List<Image> _bingArchiveImages;
        private HttpClient _httpClient;
        private List<QuickLink> _quickLinks;
        private bool _quickLinksVisible;
        private int imageArchiveIndex = -1;
        private BingImageOfTheDay ImageOfTheDay;
        private string SearchQuery { get; set; }
        public string ImageDownloadLink { get; set; }
        public string MuseumCardText { get; set; }
        public string MuseumLink { get; set; }

        public bool QuickLinksVisible
        {
            get => _quickLinksVisible;
            set
            {
                if (value == _quickLinksVisible) return;
                Console.WriteLine($"Setting show quick links to {value}");
                _quickLinksVisible = value;
                Settings.UpdateSetting(SettingsValues.ShowQuickLinks, value);
                Settings.SaveAsync();
            }
        }

        private bool _searchVisible { get; set; }

        public bool SearchVisible
        {
            get => _searchVisible;
            set
            {
                if (value == _searchVisible) return;
                Console.WriteLine($"Setting search visible to {value}");
                _searchVisible = value;
                Settings.UpdateSetting(SettingsValues.ShowWebSearch, value);
                Settings.SaveAsync();
            }
        }

        public string QuickLinksFolder
        {
            get => Settings.GetSettingValue(SettingsValues.QuickLinkBookMarkFolder);
            set
            {
                var currentFolderName = Settings.GetSettingValue(SettingsValues.QuickLinkBookMarkFolder);
                if (string.IsNullOrWhiteSpace(value) || currentFolderName == value) return;
                Console.WriteLine($"Setting Quicklinks Folder To {value}");
                Settings.UpdateSetting(SettingsValues.QuickLinkBookMarkFolder, value);
                Settings.SaveAsync();
                #pragma warning disable 4014
                SetUpQuickLinks();
                #pragma warning restore 4014
            }
        }

        protected override async Task OnInitializedAsync()
        {
            //NOTE - Do NOT get settings until the quick links are set up. The elements need to exist in the dom first or the
            //QuickLinksVisible property will cause a nasty exception because it's trying to work on an HTML element that isn't drawn yet.
            await Settings.LoadSettingsAsync(WebExtensions).ContinueWith(_ => SetUpQuickLinks());
            QuickLinksVisible = Convert.ToBoolean(Settings.GetSettingValue(SettingsValues.ShowQuickLinks));
            SearchVisible = Convert.ToBoolean(Settings.GetSettingValue(SettingsValues.ShowWebSearch));
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

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await GetBingImage();
                await GetBingImageArchive();
            }

            await base.OnAfterRenderAsync(firstRender).ConfigureAwait(false);
        }

        private async Task GetBingImage()
        {
            try
            {
                Console.WriteLine("Getting Bing Image 🔍");
                var bingImageOfTheDayUrl =
                    "https://api.allorigins.win/raw?url=https%3A//www.bing.com/HPImageArchive.aspx%3Fformat%3Djs%26idx%3D0%26n%3D1%26mkt%3Den-US";
                Console.WriteLine($"Querying bing image of the day API with the following URL: {bingImageOfTheDayUrl}");
                _httpClient ??= new HttpClient();
                var dto = await _httpClient.GetFromJsonAsync<BingImageOfTheDayDto>(bingImageOfTheDayUrl);
                if (dto != null)
                {
                    ImageOfTheDay = dto.images[0];
                    if (ImageOfTheDay != null)
                    {
                        await SetBackgroundImage(ImageOfTheDay.url);
                        UpdateMuseumCardForImageOfDay();
                        StateHasChanged();
                    }
                    else
                    {
                        Console.WriteLine(
                            "We did not set the image, there was no first image in the json that came back. ☹");
                    }
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

        private async Task GetBingImageArchive()
        {
            var dateString = $"{DateTime.Now.Year}{DateTime.Now.Month:00}{DateTime.Now.Day:00}_0700&";
            Console.WriteLine($"Querying bing with date string - {dateString}");
            var bingImageArchiveUrl =
                $"https://api.allorigins.win/raw?url=https://www.bing.com/hp/api/v1/imagegallery?format=json&ssd={dateString}";
            Console.WriteLine($"Querying bing image archive with the following URL:{bingImageArchiveUrl}");
            _httpClient ??= new HttpClient();
            var dto = await _httpClient.GetFromJsonAsync<BingImageArchiveDto>(bingImageArchiveUrl);
            _bingArchiveImages = dto?.data.images.ToList();
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
            MuseumLink = $"{ImageOfTheDay.copyrightlink}";
        }

        private void UpdateMuseumCardForArchiveImage()
        {
            MuseumCardText = _bingArchiveImages[imageArchiveIndex].title;
            MuseumLink = $"https://bing.com{_bingArchiveImages[imageArchiveIndex].clickUrl}";
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

        private void FireSearchEngineQuery()
        {
            try
            {
                Console.WriteLine($"Performing a search with query : {SearchQuery}");
                // The code below always throws an exception; we are using JS Interop to work around this for now...
                // var searchProps = new SearchProperties {Query = SearchQuery};
                // await WebExtensions.Search.Search(searchProps);
                JS.InvokeVoidAsync("runSearchQuery", SearchQuery).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Console.WriteLine("We had an error doing the search query.");
                Console.WriteLine(e);
            }
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
    }
}