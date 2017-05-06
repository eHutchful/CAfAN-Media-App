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
    public class AuthenticationToken
    {
        public string UserID { get; set; }
        public string Access_Token { get; set; }
        public string UserName { get; set; }
        public string Token_Type { get; set; }
        [JsonProperty(".expires")]
        public DateTime Expires { get; set; }
    }
}