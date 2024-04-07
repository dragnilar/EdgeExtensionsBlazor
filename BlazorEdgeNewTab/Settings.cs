using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorEdgeNewTab.Constants;
using WebExtensions.Net;
using WebExtensions.Net.Storage;

namespace BlazorEdgeNewTab;

public static class Settings
{
    private static Dictionary<string, string> SettingsDictionary = InitializeSettings();

    private static readonly StorageAreaGetKeys StorageKeys = new(
        new List<string>
        {
            SettingsValues.SearchRegion,
            SettingsValues.DisplayMode,
            SettingsValues.ShowWebSearch,
            SettingsValues.ShowQuickLinks,
            SettingsValues.ShowRandomImages,
            SettingsValues.QuickLinkBookMarkFolder,
            SettingsValues.ImageOfTheDayCache,
            SettingsValues.ReQueryImagesAfterTime,
            SettingsValues.UseQuickLinksFilter
        }
    );

    public static StorageArea Storage { get; set; }

    public static Dictionary<string, string> InitializeSettings()
    {
        return new Dictionary<string, string>
        {
            { SettingsValues.SearchRegion, "US" },
            { SettingsValues.DisplayMode, "1" },
            { SettingsValues.ShowWebSearch, "True" },
            { SettingsValues.ShowQuickLinks, "True" },
            { SettingsValues.ShowRandomImages, "False" },
            { SettingsValues.QuickLinkBookMarkFolder, SettingsValues.DefaultBookmarkFolderName },
            { SettingsValues.ImageOfTheDayCache, null },
            { SettingsValues.ReQueryImagesAfterTime, DateTime.Now.ToString(CultureInfo.InvariantCulture) },
            { SettingsValues.UseQuickLinksFilter, "False" }
        };
    }

    public static void ResetSettings()
    {
        SettingsDictionary = InitializeSettings();
    }

    public static string GetSettingValue(string settingKey)
    {
        var foundSetting = SettingsDictionary.ContainsKey(settingKey) ? SettingsDictionary[settingKey] : null;
        return foundSetting;
    }

    /// <summary>
    ///     Updates the setting indicated by the provided key with the provided value
    /// </summary>
    /// <param name="settingKey"></param>
    /// <param name="settingValue"></param>
    public static void UpdateSetting(string settingKey, object settingValue)
    {
        if (settingValue != null)
            SettingsDictionary[settingKey] = settingValue.ToString();
        else
            SettingsDictionary[settingKey] = null;
    }

    /// <summary>
    ///     Saves the settings dictionary back to the browser locals storage
    /// </summary>
    /// <returns>A completed ValueTask object</returns>
    public static ValueTask SaveAsync()
    {
        Console.WriteLine("Saving settings...");
        return Storage?.Set(SettingsDictionary) ?? ValueTask.CompletedTask;
    }

    /// <summary>
    ///     Loads the settings dictionary for the settings class from the browser storage
    /// </summary>
    /// <param name="webApi"></param>
    /// <returns></returns>
    public static async Task LoadSettingsAsync(IWebExtensionsApi webApi)
    {
        Console.WriteLine("Loading settings...");
        Storage ??= webApi.Storage.Local;
        var jsonElement = await Storage.Get(StorageKeys);
        if (jsonElement.ToString() != "{}")
        {
            Console.WriteLine("Settings found");
            var dictionary = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonElement.GetRawText());
            UpdateDictionary(dictionary);
        }
        else
        {
            Console.WriteLine("Settings do not exist exist, setting and saving defaults....");
            await SaveAsync();
        }
    }

    private static void UpdateDictionary(Dictionary<string, string> keyValuePairs)
    {
        SettingsDictionary = keyValuePairs;
    }
}