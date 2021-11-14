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
        private int _imageArchiveIndex = -1;
        private BingImageOfTheDay _imageOfTheDay;
        private bool _searchVisible;
        private bool _quickLinksVisible;
        private bool _filterMode;
        private string SearchQuery { get; set; }
        public string ImageDownloadLink { get; set; }
        public string MuseumCardText { get; set; }
        public string MuseumCardText2 { get; set; }
        public string MuseumLink { get; set; }
        public string MuseumLink2 { get; set; }
        public SettingsMenu SettingsMenuNewTab { get; set; }

        protected ElementReference SearchBoxElement;

        public bool FilterMode
        {
            get => _filterMode;
            set
            {
                _filterMode = value;
                UpdateSearchEmptyText();
                Settings.UpdateSetting(SettingsValues.UseQuickLinksFilter, value.ToString());
                Settings.SaveAsync();
            }
        }

        public string SearchEmptyText { get; set; } = "Search the web...";

        protected override async Task OnInitializedAsync()
        {
            //NOTE - Do NOT get settings until the quick links are set up. The elements need to exist in the dom first or the
            //QuickLinksVisible property will cause a nasty exception because it's trying to work on an HTML element that isn't drawn yet.
            await Settings.LoadSettingsAsync(WebExtensions).ContinueWith(_ => SetUpQuickLinks());
            await GetBingImagesForNewTab();
            _filterMode = Convert.ToBoolean(Settings.GetSettingValue(SettingsValues.UseQuickLinksFilter));
            if (_filterMode) UpdateSearchEmptyText();
            JS.InvokeVoidAsync("SetFocusToElement", SearchBoxElement);
        }
        
        private void UpdateSearchEmptyText()
        {
            SearchEmptyText = _filterMode
                ? "Type in a filter and hit enter to apply it. Blank filter resets the filter."
                : "Search the web...";
        }

        private async Task GetBingImagesForNewTab()
        {
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
            if (_filterMode)
            {
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    foreach (var quickLink in _quickLinks)
                    {
                        quickLink.Visible = quickLink.QuickLinkTitle.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);
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
                    _imageOfTheDay = dto.images[0];
                    if (_imageOfTheDay != null)
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
            await SetBackgroundImage(_imageOfTheDay.url);
            UpdateMuseumCardForImageOfDay();
            StateHasChanged();
        }

        private async Task GetNextImage()
        {
            Console.WriteLine($"Getting next image 🖼, current image index is: {_imageArchiveIndex}");
            if (_imageArchiveIndex >= _bingArchiveImages.Count - 1)
            {
                _imageArchiveIndex = -1;
                await SetBackgroundImage(_imageOfTheDay.url);
                UpdateMuseumCardForImageOfDay();
            }
            else
            {
                _imageArchiveIndex++;
                await SetBackgroundImage(GetImageUrlForIndex(_imageArchiveIndex));
                UpdateMuseumCardForArchiveImage();
            }
        }

        private async Task GetPreviousImage()
        {
            Console.WriteLine($"Getting previous image 🖼, current image index is: {_imageArchiveIndex}");
            if (_imageArchiveIndex == -1)
            {
                _imageArchiveIndex = _bingArchiveImages.Count - 1;
                await SetBackgroundImage(GetImageUrlForIndex(_imageArchiveIndex));
                UpdateMuseumCardForArchiveImage();
            }
            else if (_imageArchiveIndex == 0)
            {
                _imageArchiveIndex = -1;
                await SetBackgroundImage(_imageOfTheDay.url);
                UpdateMuseumCardForImageOfDay();
            }
            else
            {
                _imageArchiveIndex--;
                await SetBackgroundImage(GetImageUrlForIndex(_imageArchiveIndex));
                UpdateMuseumCardForArchiveImage();
            }
        }

        private void UpdateMuseumCardForImageOfDay()
        {
            MuseumCardText = _imageOfTheDay.title;
            MuseumLink = _imageOfTheDay.copyrightlink;
            MuseumCardText2 = _imageOfTheDay.copyright;
            MuseumLink2 = $"https://bing.com{_imageOfTheDay.quiz}";
        }

        private void UpdateMuseumCardForArchiveImage()
        {
            MuseumCardText = _bingArchiveImages[_imageArchiveIndex].title;
            MuseumLink = $"https://bing.com{_bingArchiveImages[_imageArchiveIndex].clickUrl}";
            MuseumCardText2 = _bingArchiveImages[_imageArchiveIndex].description;
            MuseumLink2 = $"https://bing.com{_bingArchiveImages[_imageArchiveIndex].clickUrl}";
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
            return index < 0 ? $"BingImageOfTheDay_{_imageOfTheDay.startdate}" : $"BingImageOfTheDay_{_bingArchiveImages[index].isoDate}";
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
            if (_filterMode)
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
                if (_filterMode)
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
                Filename = $"{GetImageNameForIndex(_imageArchiveIndex)}.jpeg"
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

        private async Task RefreshImageOfDayHandler()
        {
            Settings.UpdateSetting(SettingsValues.ReQueryImagesAfterTime, DateTime.Now.AddDays(-1).ToString(CultureInfo.InvariantCulture));
            Settings.UpdateSetting(SettingsValues.ReQueryArchiveAfterTime, DateTime.Now.AddDays(-1).ToString(CultureInfo.InvariantCulture));
            Settings.SaveAsync();
            GetBingImagesForNewTab();
            StateHasChanged();
        }
    }
}