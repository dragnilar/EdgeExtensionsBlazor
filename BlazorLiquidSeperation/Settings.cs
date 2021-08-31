using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using BlazorLiquidSeperation.Constants;
using WebExtensions.Net;
using WebExtensions.Net.Storage;

namespace BlazorLiquidSeperation
{
    public static class Settings
    {
        private static Dictionary<string, string> SettingsDictionary = new()
        {
            { SettingsValues.SearchRegion, "US" },
            { SettingsValues.DisplayMode, "1" },
            { SettingsValues.ShowWebSearch, "True" },
            { SettingsValues.ShowQuickLinks, "True" },
            { SettingsValues.ShowRandomImages, "False" },
            { SettingsValues.QuickLinkBookMarkFolder, SettingsValues.DefaultBookmarkFolderName }
        };

        private static readonly StorageAreaSyncGetKeys StorageKeys = new(
            new List<string>
            {
                SettingsValues.SearchRegion,
                SettingsValues.DisplayMode,
                SettingsValues.ShowWebSearch,
                SettingsValues.ShowQuickLinks,
                SettingsValues.ShowRandomImages,
                SettingsValues.QuickLinkBookMarkFolder
            }
        );

        public static StorageAreaSync Storage { get; set; }

        public static string GetSettingValue(string settingKey)
        {
            return SettingsDictionary[settingKey];
        }

        /// <summary>
        ///     Updates the setting indicated by the provided key with the provided value
        /// </summary>
        /// <param name="settingKey"></param>
        /// <param name="settingValue"></param>
        public static void UpdateSetting(string settingKey, object settingValue)
        {
            SettingsDictionary[settingKey] = settingValue.ToString();
        }

        /// <summary>
        ///     Saves the settings dictionary back to the browser sync storage
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
            Storage ??= await webApi.Storage.GetSync();
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
}