using System.Text.Json.Serialization;

namespace BlazorEdgeNewTab.Models
{
    public class AllOriginsStatus
    {
        [JsonPropertyName("url")]
        public string Url          { get; set; }
        [JsonPropertyName("content_type")]
        public string ContentType  { get; set; }
        [JsonPropertyName("http_code")]
        public int    HttpCode     { get; set; }
        [JsonPropertyName("response_time")]
        public int    ResponseTime { get; set; }
        [JsonPropertyName("content_length")]
        public int      ContentLength            { get; set; }
    }
}
