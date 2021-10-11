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
using BlazorEdgeNewTab.Services.Interfaces;

namespace BlazorEdgeNewTab.Services
{
    public class NewTabService : INewTabService
    {
        private HttpClient _httpClient;
        public async Task<BingImageOfTheDayDto> GetImageOfDayDto()
        {
            BingImageOfTheDayDto dto;
            var reQueryTime = Convert.ToDateTime(Settings.GetSettingValue(SettingsValues.ReQueryImagesAfterTime)
                                                 ?? DateTime.Now.AddDays(-1).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine($"Requery date/time for image of day: {reQueryTime.ToString()}");
            if (Settings.GetSettingValue(SettingsValues.ImageOfTheDayCache) != null &&
                DateTime.Now.Date <= reQueryTime.Date)
            {
                Console.WriteLine("Getting image of the day data from cache...");
                dto = JsonSerializer.Deserialize<BingImageOfTheDayDto>(
                    Settings.GetSettingValue(SettingsValues.ImageOfTheDayCache));
            }
            else
            {
                var bingImageOfTheDayUrl =
                    "https://api.allorigins.win/raw?url=https%3A//www.bing.com/HPImageArchive.aspx%3Fformat%3Djs%26idx%3D0%26n%3D1%26mkt%3Den-US";
                _httpClient ??= new HttpClient();
                dto = await _httpClient.GetFromJsonAsync<BingImageOfTheDayDto>(bingImageOfTheDayUrl);
                Settings.UpdateSetting(SettingsValues.ImageOfTheDayCache, JsonSerializer.Serialize(dto));
                Settings.UpdateSetting(SettingsValues.ReQueryImagesAfterTime, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                await Settings.SaveAsync();
            }

            return dto;
        }
        
        public async Task<List<Image>> GetBingImageArchive()
        {
            List<Image> imageArchives;
            var reQueryTime = Convert.ToDateTime(Settings.GetSettingValue(SettingsValues.ReQueryArchiveAfterTime)
                                                 ?? DateTime.Now.AddDays(-1).ToString(CultureInfo.InvariantCulture));
            Console.WriteLine($"Requery date/time for image archive: {reQueryTime.ToString()}");
            if (Settings.GetSettingValue(SettingsValues.ImageArchiveCache) != null &&
                DateTime.Now.Date <= reQueryTime.Date)
            {
                Console.WriteLine("Getting image archive data from cache...");
                imageArchives =
                    JsonSerializer.Deserialize<List<Image>>(Settings.GetSettingValue(SettingsValues.ImageArchiveCache));
            }
            else
            {
                var dateString = $"{DateTime.Now.Year}{DateTime.Now.Month:00}{DateTime.Now.Day:00}_0700&";
                Console.WriteLine($"Querying bing with date string - {dateString}");
                var bingImageArchiveUrl =
                    $"https://api.allorigins.win/raw?url=https://www.bing.com/hp/api/v1/imagegallery?format=json&ssd={dateString}";
                _httpClient ??= new HttpClient();
                var dto = await _httpClient.GetFromJsonAsync<BingImageArchiveDto>(bingImageArchiveUrl);
                imageArchives = dto?.data.images.ToList();
                Console.WriteLine("Caching image archive data....");
                Settings.UpdateSetting(SettingsValues.ImageArchiveCache, JsonSerializer.Serialize(imageArchives));
                Settings.UpdateSetting(SettingsValues.ReQueryArchiveAfterTime, DateTime.Now.ToString(CultureInfo.InvariantCulture));
                await Settings.SaveAsync();
            }

            return imageArchives;
        }

    }
}