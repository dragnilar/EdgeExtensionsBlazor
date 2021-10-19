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
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WebExtensions.Net.Bookmarks;
using WebExtensions.Net.Downloads;

namespace BlazorEdgeNewTab.Pages
{
    public partial class NewTab
    {
        private List<Image> _bingArchiveImages;
        private List<QuickLink> _quickLinks;
        private int imageArchiveIndex = -1;
        private BingImageOfTheDay ImageOfTheDay;
        private bool _searchVisible;
        private bool _quickLinksVisible; 
        private string SearchQuery { get; set; }
        public string ImageDownloadLink { get; set; }
        public string ImageDownloadName { get; set; }
        public string MuseumCardText { get; set; }
        public string MuseumCardText2 { get; set; }
        public string MuseumLink { get; set; }
        public string MuseumLink2 { get; set; }
        public SettingsMenu SettingsMenuNewTab { get; set; }
        public bool FilterMode { get; set; }

        protected override async Task OnInitializedAsync()
        {
            //NOTE - Do NOT get settings until the quick links are set up. The elements need to exist in the dom first or the
            //QuickLinksVisible property will cause a nasty exception because it's trying to work on an HTML element that isn't drawn yet.
            await Settings.LoadSettingsAsync(WebExtensions).ContinueWith(_ => SetUpQuickLinks());
            await GetBingImage();
            _bingArchiveImages = await NewTabService.GetBingImageArchive();
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

        private void FilterQuickLinks()
        {
            if (FilterMode)
            {
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    foreach (var quickLink in _quickLinks)
                    {

                        if (quickLink.QuickLinkTitle.Contains(SearchQuery))
                        {
                            quickLink.Visible = true;
                        }
                        else
                        {
                            quickLink.Visible = false;
                        }
                    }

                    StateHasChanged();
                }
                else
                {
                    _quickLinks.ForEach(x=>x.Visible = true);
                }
            }
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
                QuickLinkUrl = quickLinkBookMark.Url,
                QuickLinkId = quickLinkBookMark.Id,
                Visible = true
            }));
            StateHasChanged();
        }

        private async Task GetBingImage()
        {
            try
            {
                Console.WriteLine("Getting Bing Image 🔍");
                var dto = await NewTabService.GetImageOfDayDto();
                if (dto != null)
                {
                    ImageOfTheDay = dto.images[0];
                    if (ImageOfTheDay != null)
                        await ApplyImageOfTheDay(dto);
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

        private async Task ApplyImageOfTheDay(BingImageOfTheDayDto dto)
        {
            await SetBackgroundImage(ImageOfTheDay.url);
            UpdateMuseumCardForImageOfDay();
            StateHasChanged();
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

        private string GetImageNameForIndex(int index)
        {
            return index < 0 ? $"BingImageOfTheDay_{ImageOfTheDay.startdate}" : $"BingImageOfTheDay_{_bingArchiveImages[index].isoDate}";
        }

        private async Task SetBackgroundImage(string url)
        {
            var fullImagePath = "https://bing.com" + url;
            await JS.InvokeVoidAsync("setBackgroundImage1", fullImagePath).ConfigureAwait(false);
            ImageDownloadLink = fullImagePath.Replace("&pid=hp", "");
            Console.WriteLine("Image set 😊");
        }

        private void Enter()
        {
            if (FilterMode)
            {
                FilterQuickLinks();
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(SearchQuery)) FireSearchEngineQuery();
            }

        }

        private Task FireSearchEngineQuery()
        {
            try
            {
                if (FilterMode)
                {
                    FilterQuickLinks();
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(SearchQuery)) return Task.CompletedTask;
                    Console.WriteLine($"Performing a search with query : {SearchQuery}");
                    // The code below always throws an exception; we are using JS Interop to work around this for now...
                    // var searchProps = new SearchProperties {Query = SearchQuery};
                    //await WebExtensions.Search.Query(new QueryInfo {Text = SearchQuery, Disposition = Disposition.CurrentTab, AdditionalData = null, TabId = null}).ConfigureAwait(false);
                    JS.InvokeVoidAsync("runSearchQuery", SearchQuery).ConfigureAwait(false);
                }

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
                Filename = $"{GetImageNameForIndex(imageArchiveIndex)}.jpeg"
            });
        }

        private async Task SaveNewQuickLinkClickHandler()
        {
            await SetUpQuickLinks();
        }

        private async Task QuickLinksFolderChangedHandler()
        {
            await SetUpQuickLinks();
        }

        public async Task DeleteQuickLink(string quickLinkId)
        {
            await WebExtensions.Bookmarks.Remove(quickLinkId);
            await SetUpQuickLinks();
        }

        private void SearchVisibleChanged()
        {
            _searchVisible = SettingsMenuNewTab.SearchVisible;
        }

        private void QuickLinksVisibleChanged()
        {
            _quickLinksVisible = SettingsMenuNewTab.QuickLinksVisible;
        }
    }
}