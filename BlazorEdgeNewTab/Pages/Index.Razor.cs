﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using BlazorEdgeNewTab.Constants;
using BlazorEdgeNewTab.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using WebExtensions.Net.Bookmarks;
using WebExtensions.Net.Downloads;

namespace BlazorEdgeNewTab.Pages;

public partial class Index
{
    private List<BingImageOfTheDay>       _bingImages;
    private bool              _filterMode;
    private int               _imageArchiveIndex = -1;
    private BingImageOfTheDay _imageOfTheDay;
    private List<QuickLink>   _quickLinks { get; set; }
    private bool              _quickLinksVisible { get; set; }
    private bool              _searchVisible;

    protected ElementReference SearchBoxElement;
    private   string           SearchQuery        { get; set; }
    public    string           ImageDownloadLink  { get; set; }
    public    string           MuseumCardText     { get; set; }
    public    string           MuseumCardText2    { get; set; }
    public    string           MuseumLink         { get; set; }
    public    string           MuseumLink2        { get; set; }
    public    SettingsMenu     SettingsMenuNewTab { get; set; }

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
        await JS.InvokeVoidAsync("SetFocusToElement", SearchBoxElement);
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
                    quickLink.Visible =
                        quickLink.QuickLinkTitle.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase);

                StateHasChanged();
            }
            else
            {
                _quickLinks.ForEach(x => x.Visible = true);
            }
        }
    }

    public async Task SetUpQuickLinks()
    {
        _quickLinks = new List<QuickLink>();
        var bookMarkNode =
            await WebExtensions.Bookmarks.Search(Settings.GetSettingValue(SettingsValues.QuickLinkBookMarkFolder));
        var bookmarkTreeNodes = bookMarkNode.ToList();
        if (!bookmarkTreeNodes.Any())
        {
            var folderName = Settings.GetSettingValue(SettingsValues.QuickLinkBookMarkFolder);
            await WebExtensions.Bookmarks.Create(new CreateDetails
                { Title = folderName });
        }
        else
        {
            var quickLinkBookMarks = await WebExtensions.Bookmarks.GetChildren(bookmarkTreeNodes.First().Id);
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
    }

    private async Task GetBingImage()
    {
        try
        {
            var dto = await NewTabService.GetImageOfDayDto();
            if (dto != null)
            {
                _imageOfTheDay = dto.images[0];
                if (_imageOfTheDay != null)
                {
                    await ApplyImageOfTheDay(dto);
                    _bingImages = dto.images.Skip(1).Take(7).ToList();
                }
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
        if (_imageArchiveIndex >= _bingImages.Count - 1)
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
        switch (_imageArchiveIndex)
        {
            case -1:
                _imageArchiveIndex = _bingImages.Count - 1;
                await SetBackgroundImage(GetImageUrlForIndex(_imageArchiveIndex));
                UpdateMuseumCardForArchiveImage();
                break;
            case 0:
                _imageArchiveIndex = -1;
                await SetBackgroundImage(_imageOfTheDay.url);
                UpdateMuseumCardForImageOfDay();
                break;
            default:
                _imageArchiveIndex--;
                await SetBackgroundImage(GetImageUrlForIndex(_imageArchiveIndex));
                UpdateMuseumCardForArchiveImage();
                break;
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
        MuseumCardText = _bingImages[_imageArchiveIndex].title;
        MuseumLink = _bingImages[_imageArchiveIndex].copyrightlink;
        MuseumCardText2 = _bingImages[_imageArchiveIndex].copyright;
        MuseumLink2 = $"https://bing.com{_bingImages[_imageArchiveIndex].quiz}";
    }

    private string GetImageUrlForIndex(int index)
    {
        return _bingImages[index].url;
    }

    private string GetImageNameForIndex(int index)
    {
        return index < 0
            ? $"BingImageOfTheDay_{_imageOfTheDay.startdate}"
            : $"BingImageOfTheDay_{_bingImages[index].startdate}";
    }

    private async Task SetBackgroundImage(string url)
    {
        var fullImagePath = "https://bing.com" + url;
        await JS.InvokeVoidAsync("setBackgroundImage1", fullImagePath).ConfigureAwait(false);
        ImageDownloadLink = fullImagePath.Replace("&pid=hp", "");
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



    private async Task OnDownloadImageClick()
    {
        await WebExtensions.Downloads.Download(new DownloadOptions
        {
            Url = ImageDownloadLink,
            SaveAs = true,
            Filename = $"{GetImageNameForIndex(_imageArchiveIndex)}.jpeg"
        });
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
        Settings.UpdateSetting(SettingsValues.ReQueryImagesAfterTime,
            DateTime.Now.AddDays(-1).ToString(CultureInfo.InvariantCulture));
        await Settings.SaveAsync();
        GetBingImagesForNewTab();
        StateHasChanged();
    }
}