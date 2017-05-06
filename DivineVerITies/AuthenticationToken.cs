using System;
using Newtonsoft.Json;

namespace DivineVerITies
{
    public class AuthenticationToken
    {
        [JsonProperty("guid")]
        public string UserID { get; set; }
        public string Access_Token { get; set; }
        public string UserName { get; set; }
        public string Token_Type { get; set; }
        [JsonProperty(".expires")]
        public DateTime Expires { get; set; }
    }
}