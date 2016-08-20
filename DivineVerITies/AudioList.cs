using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;

namespace DivineVerITies
{
    public class AudioList
    {
        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("subtitle")]
        public string SubTitle { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("pubdate")]
        public string PubDate { get; set; }

        [JsonProperty("imageurl")]
        public string ImageUrl { get; set; }

        [JsonProperty("link")]
        public string Link { get; set; }
    }
}