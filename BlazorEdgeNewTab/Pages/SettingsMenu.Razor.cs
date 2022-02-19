using System;
using BlazorEdgeNewTab.Constants;
using Microsoft.AspNetCore.Components;

namespace BlazorEdgeNewTab.Pages;

public partial class SettingsMenu
{
    [Parameter]
    public EventCallback OnQuickLinksFolderChangedCallBack { get; set; }

    [Parameter]
    public EventCallback OnQuickLinksVisibleChangedCallBack { get; set; }

    [Parameter]
    public EventCallback OnSearchVisibleChangedCallBack { get; set; }

    [Parameter]
    public EventCallback OnRefreshImageOfDayCallBack { get; set; }

    private bool _quickLinksVisible { get; set; }

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
            OnQuickLinksVisibleChangedCallBack.InvokeAsync();
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
            OnSearchVisibleChangedCallBack.InvokeAsync();
        }
    }

    public string QuickLinksFolder
    {
        get => Settings.GetSettingValue(SettingsValues.QuickLinkBookMarkFolder);
        set
        {
            var currentFolderName = Settings.GetSettingValue(SettingsValues.QuickLinkBookMarkFolder);
            if (string.IsNullOrWhiteSpace(value) || currentFolderName == value) return;
            Console.WriteLine($"Setting QuickLinks Folder To {value}");
            Settings.UpdateSetting(SettingsValues.QuickLinkBookMarkFolder, value);
            Settings.SaveAsync();
            OnQuickLinksFolderChangedCallBack.InvokeAsync();
        }
    }

    protected override void OnAfterRender(bool firstRender)
    {
        QuickLinksVisible = Convert.ToBoolean(Settings.GetSettingValue(SettingsValues.ShowQuickLinks));
        SearchVisible = Convert.ToBoolean(Settings.GetSettingValue(SettingsValues.ShowWebSearch));
        base.OnAfterRender(firstRender);
    }

    private void ResetSettings()
    {
        Settings.ResetSettings();
        Settings.SaveAsync();
    }

    private void RefreshImageOfDay()
    {
        OnRefreshImageOfDayCallBack.InvokeAsync();
    }
}