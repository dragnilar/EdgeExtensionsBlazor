using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebExtensions.Net.Storage;

namespace BlazorLiquidSeperation
{
    public static class Settings
    {
        public static StorageAreaSync Storage { get; set; }
        public const string SearchRegion = "SearchRegion";
        public const string DisplayMode = "DisplayMode";
        public const string ShowWebSearch = "ShowWebSearch";
        public const string ShowQuickLinks = "ShowQuickLinks";
        public const string ShowRandomImages = "ShowRandomImages";
        public const string QuickLinkBookMarkFolder = "QuickLinkBookmarkFolder";
        public const string DefualtBookmarkFolderName = "Edge Quick Links";
        private static Dictionary<string, string> SettingsDictionary = new()
        {
            { SearchRegion, "US" },
            { DisplayMode, "1" },
            { ShowWebSearch, "True" },
            { ShowQuickLinks, "True" },
            { ShowRandomImages, "False" },
            { QuickLinkBookMarkFolder, DefualtBookmarkFolderName }

        };

        public static StorageAreaSyncGetKeys StorageKeys = new(
            new List<string>
            {
                SearchRegion,
                DisplayMode,
                ShowWebSearch,
                ShowQuickLinks,
                ShowRandomImages,
                QuickLinkBookMarkFolder
            }
        );

        public static void UpdateDictionary(Dictionary<string, string> keyValuePairs)
        {
            SettingsDictionary = keyValuePairs;
        }

        public static string GetSettingValue(string settingKey)
        {
            return SettingsDictionary[settingKey];
        }

        public static void UpdateSetting(string settingKey, object settingValue)
        {
            SettingsDictionary[settingKey] = settingValue.ToString();
        }

        ///<summary>
        /// Saves the settings dictionary back to the browser sync storage
        ///</summary>
        /// <returns>A completed ValueTask object</returns>
        public static ValueTask SaveAsync()
        {
            Console.WriteLine("Saving settings...");
            return Storage?.Set(SettingsDictionary) ?? ValueTask.CompletedTask;
        }

    }
}
