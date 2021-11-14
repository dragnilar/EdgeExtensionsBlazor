using System;

namespace BlazorEdgeNewTab.Constants
{
    public static class SettingsValues
    {
        public const string SearchRegion = "SearchRegion";
        public const string DisplayMode = "DisplayMode";
        public const string ShowWebSearch = "ShowWebSearch";
        public const string ShowQuickLinks = "ShowQuickLinks";
        public const string ShowRandomImages = "ShowRandomImages";
        public const string QuickLinkBookMarkFolder = "QuickLinkBookmarkFolder";
        public const string DefaultBookmarkFolderName = "Edge Quick Links";
        public const string ImageOfTheDayCache = "ImageOfTheDayCache";
        public const string ImageArchiveCache = "ImageArchiveCache";
        public const string ReQueryImagesAfterTime = "ReQueryImagesAfterTime";
        public const string ReQueryArchiveAfterTime = "ReQueryArchiveAfterTime";
        public const string UseQuickLinksFilter = "UseQuickLinksFilter";
        public static DateTime DefaultRequeryDateTime = new(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 6, 0, 0);
    }
}