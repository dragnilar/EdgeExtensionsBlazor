using System.Text.Json.Serialization;

namespace BlazorLiquidSeperation.Models
{
    public class BingImage
    {

        public BingImage(string pageUrl, string thumbUrl, string title, string copyRight, string date, string fullUrl)
        {
            PageUrl = pageUrl;
            ThumbUrl = thumbUrl;
            Title = title;
            CopyRight = copyRight;
            Date = date;
            FullUrl = fullUrl;
        }

        [JsonPropertyName("pageUrl")]
        public string PageUrl { get; set; }
        [JsonPropertyName("thumbUrl")]
        public string ThumbUrl { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("copyright")]
        public string CopyRight { get; set; }
        [JsonPropertyName("date")]
        public string Date { get; set; }
        [JsonPropertyName("fullUrl")]
        public string FullUrl { get; set; }
    }
}
