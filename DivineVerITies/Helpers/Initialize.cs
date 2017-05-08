using Android.Graphics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ModernHttpClient;
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
            string feed = "http://versolstore.blob.core.windows.net/verity/audios/audio.rss";
            var httpClient = new HttpClient(new NativeMessageHandler());
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            string response = await httpClient.GetStringAsync(feed);
            var xdoc = XDocument.Parse(response);
            var result = (from item in xdoc.Descendants("item")
                          select new AudioList
                          {
                              Title = (string)item.Element("title"),
                              Description = (string)item.Element("description"),
                              ImageUrl = (string)item.Elements().Where(y => y.Name.LocalName == "image").FirstOrDefault().Attribute("href"),
                              Link = (string)item.Element("enclosure").Attribute("url"),
                              SubTitle = (string)item.Elements().Where(y => y.Name.LocalName == "subtitle").FirstOrDefault(),
                              PubDate = (string)item.Element("pubDate")
                          }).ToList<AudioList>();
            return result;
        }

        public async Task<List<Video>> getVideoList()
        {

            string feed = "http://versolstore.blob.core.windows.net/verity/videos/video.rss";
            var httpClient = new HttpClient(new NativeMessageHandler());
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            string response = await httpClient.GetStringAsync(feed);
            var xdoc = XDocument.Parse(response);
            return (from item in xdoc.Descendants("item")
                    select new Video
                    {
                        Title = (string)item.Element("title"),
                        Description = (string)item.Element("description"),
                        ImageUrl = (string)item.Elements().Where(y => y.Name.LocalName == "image").FirstOrDefault().Attribute("href"),
                        Link = (string)item.Element("enclosure").Attribute("url"),
                        //ImageUrl = "http://www.blubrry.com/coverart/orig/379169-504006.png",
                        //Link = (string)item.Element("link") + (string)item.Element("enclosure").Attribute("url"),
                        SubTitle = (string)item.Elements().Where(y => y.Name.LocalName == "subtitle").FirstOrDefault(),
                        PubDate = (string)item.Element("pubDate")
                    }).ToList<Video>();
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