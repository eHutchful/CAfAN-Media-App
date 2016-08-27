using Android.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DivineVerITies.Helpers
{
    class Initialize
    {
        public async Task<List<AudioList>> getAudioList()
        {
            string feed = "http://feeds.soundcloud.com/users/soundcloud:users:247218071/sounds.rss";
            var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            string response = await httpClient.GetStringAsync(feed);               
            var xdoc = XDocument.Parse(response);
            return (from item in xdoc.Descendants("item")
                    select new AudioList
                    {
                        Title = (string)item.Element("title"),
                        Description = (string)item.Element("description"),
                        ImageUrl = (string)item.Elements().Where(y => y.Name.LocalName == "image").FirstOrDefault().Attribute("href"),
                        Link = (string)item.Element("enclosure").Attribute("url"),
                        SubTitle = (string)item.Elements().Where(y => y.Name.LocalName == "subtitle").FirstOrDefault(),
                        PubDate = (string)item.Element("pubDate")
                    }).ToList<AudioList>();
        }

        public Bitmap GetImageBitmapFromUrl(string Url)
        {
            Bitmap imageBitmap = null;
            var memoryStream = new MemoryStream();
            byte[] imageBytes=null;
            using (var client = new HttpClient())
            {
                Task.Run(
                    async () =>
                    {
                        imageBytes = await client.GetByteArrayAsync(Url);                        
                    }
                    ).Wait();
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                }
            }
            return imageBitmap;
        }
    }
}