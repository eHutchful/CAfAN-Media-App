using Newtonsoft.Json;

namespace DivineVerITies
{
    public class Video
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subtitle")]
        public string SubTitle { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("pubdate")]
        public string PubDate { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }

        [JsonProperty("imageurl")]
        public string ImageUrl { get; set; }
        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }
    }
}
