using System.Collections.Generic;
using WebExtensions.Net.Storage;

namespace BlazorLiquidSeperation.Models
{
    public static class ExtensionSettings
    {
        public const string SearchRegion = "SearchRegion";
        public const string DisplayMode = "DisplayMode";
        public const string ShowWebSearch = "ShowWebSearch";
        public const string ShowQuickLinks = "ShowQuickLinks";
        public const string ShowRandomImages = "ShowRandomImages";

        public static Dictionary<string, string> SettingsDictionary = new()
        {
            {SearchRegion, "US"},
            {DisplayMode, "1"},
            {ShowWebSearch, "True"},
            {ShowQuickLinks, "True"},
            {ShowRandomImages, "False"}
        };

        public static StorageAreaSyncGetKeys StorageKeys = new(
            new List<string>
            {
                SearchRegion,
                DisplayMode,
                ShowWebSearch,
                ShowQuickLinks,
                ShowRandomImages
            }
        );
    }
}