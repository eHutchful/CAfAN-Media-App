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
using Android.Graphics;

namespace DivineVerITies.Helpers
{
    public class Media
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public Bitmap AlbumArt { get; set; }
        public string Album { get; set; }
        public string Genre { get; set; }
        public string Location { get; set; }

    }
}