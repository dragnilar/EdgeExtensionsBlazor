using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BlazorEdgeNewTab.Constants;
using BlazorEdgeNewTab.Models;
using BlazorEdgeNewTab.Services.Interfaces;

namespace BlazorEdgeNewTab.Services;

public class NewTabService : INewTabService
{
    private HttpClient _httpClient;
    private readonly string BingImageOfDayUri =
        "https://api.allorigins.win/get?url=https%3A//www.bing.com/HPImageArchive.aspx%3Fformat%3Djs%26idx%3D0%26n%3D7";

    public async Task<BingImageOfTheDayDto> GetImageOfDayDto()
    {
        BingImageOfTheDayDto dto = new BingImageOfTheDayDto();
        var reQueryTime = Convert.ToDateTime(Settings.GetSettingValue(SettingsValues.ReQueryImagesAfterTime)
                                             ?? DateTime.Now.AddDays(-1).ToString(CultureInfo.InvariantCulture));
        Console.WriteLine($"Re-Query Date for Bing Image Of Day In Cache Is: {reQueryTime.Date}");
        Console.WriteLine($"Current Date Is: {DateTime.Now.Date}");
        if (DateTime.Now.Date > reQueryTime.Date || Settings.GetSettingValue(SettingsValues.ImageOfTheDayCache) == null)
        {
            Console.WriteLine("Getting Bing Image of Day from Bing");
            Console.WriteLine($"Using URL to get image of the day: {BingImageOfDayUri}");
            _httpClient ??= new HttpClient();
            var responseDto = await _httpClient.GetFromJsonAsync<AllOriginsResultDto>(BingImageOfDayUri);
            if (responseDto != null)
            {
                var correctedJson = responseDto.contents.Replace("\\", "");
                dto = JsonSerializer.Deserialize<BingImageOfTheDayDto>(correctedJson, new JsonSerializerOptions
                {
                    
                });
                Console.WriteLine($"Dto from bing: {dto.images[0].fullstartdate}");
                var imageDate = DateTime.ParseExact(dto.images[0].fullstartdate, "yyyyMMddHHmm", CultureInfo.InvariantCulture);
                Settings.UpdateSetting(SettingsValues.ImageOfTheDayCache, JsonSerializer.Serialize(dto));
                Settings.UpdateSetting(SettingsValues.ReQueryImagesAfterTime,
                    imageDate.ToString(CultureInfo.InvariantCulture));
                await Settings.SaveAsync();
                return dto;
            }
            else
            {
                Console.WriteLine($"Received no or a null response back from All Origins. :(");
            }
        }
        else
        {
            Console.WriteLine($"Using cached Bing Image Of Day.");
            dto = JsonSerializer.Deserialize<BingImageOfTheDayDto>(
                Settings.GetSettingValue(SettingsValues.ImageOfTheDayCache));
        }
        
        return dto;
    }
}