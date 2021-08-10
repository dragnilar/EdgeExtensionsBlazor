# EdgeExtensionsBlazor
This repo contains some Edge/Chromium Extensions that use Blazor (Web Assembly).

## Blazor-Seperation
This is a new tab page extension that is similar to the new tab page for Microsoft Edge. 
- Background image uses the Bing image of the day and updates along with Bing
- Allows you to view previous bing images of the day
- Creates quick links via a specified Book Mark folder. Unlike with Edge, you can have as many quick links as you want.*
- Features app link menu for Microsoft 365. Some of the links are different than the one for Edge (I.E. Visual Studio and Azure).
- Allows you to search using your default search engine.

* Currently the bookmark folder name is hardcoded as Edge Quick Links (case matters). 

# How To Build:

- This project has no external dependencies besides the ones Blazor/Asp.NET rely upon and one specific NuGet package for allowing Blazor to to run as a Browser Extension.
- You can build it using either the Dot Net 5 SDK CLI, JetBrains Rider or Visual Studio 2019 (latest version; edition does not matter).
- When you build the project, the output folder will contain a wwwroot folder. This is the folder that you need to load for the unpacked extension via your extensions menu in Edge/Chromium
