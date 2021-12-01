# EdgeExtensionsBlazor
This repo contains some Edge/Chromium Extensions that use Blazor (Web Assembly).

All of these extensions make extensive use of [Blazor.BrowserExtension](https://github.com/mingyaulee/Blazor.BrowserExtension), and would not be possible otherwise. If you are interested in developing extensions using Blazor, I strongly encourage you to check it out.

## Blazor New Tab

![Blazor Edge New Tab Logo](https://github.com/dragnilar/EdgeExtensionsBlazor/blob/master/BlazorEdgeNewTab/wwwroot/icons/BlazorNewTab.png)

This is a new tab page extension that is similar to the new tab page for Microsoft Edge. 
- Background image uses the Bing image of the day and updates along with Bing.
- Allows you to view previous bing images of the day.
- Creates quick links via a specified Book Mark folder. Unlike with Edge, you can have as many quick links as you want.*
- Features app link menu for Microsoft 365. Some of the links are different than the one for Edge (I.E. Visual Studio and Azure).
- Allows you to search using your default search engine.

 *The bookmarks folder is defaulted to Edge Quick Links; you can change it in the settings menu

**Note:**  The extension uses the same APIs that Microsoft uses for Bing via a proxy at allorigins.win (due to Bing using CORS). As a result, it may at times take a little bit of time for the image of the day to show up when the extension fires up for the first time on a particular day. Also, allorigins.win sometimes may be down due to issues on their side, which is beyond my control. I am looking into alternatives but for now this is a known issue.

### Get Blazor New Tab from the Edge Add-Ons Website:

- If you are using Microsoft Edge, simply visit the link below to install the latest public release of the extension (only available in US / English):
- [Blazor Edge New Tab - Edge Add Ons](https://microsoftedge.microsoft.com/addons/detail/blazor-edge-new-tab/bfhdepjammnaoddhikhogfbnikmeocfj)


# How To Build:

- This project has no external dependencies besides the ones Blazor/Asp.NET rely upon and one specific NuGet package for allowing Blazor to to run as a Browser Extension.
- You can build it using either the Dot Net 5 SDK CLI, JetBrains Rider or Visual Studio 2019 (latest version; edition does not matter).
- When you build the project, the output folder will contain a browserextension folder. This is the folder that you need to load for the unpacked extension via your extensions menu in Edge/Chromium
