using Android.Graphics;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ModernHttpClient;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace DivineVerITies.Helpers
{
    class Initialize
    {
        public async Task<List<AudioList>> getAudioList()
        {
            string feed = "http://versolstore.blob.core.windows.net/cafan/audios/audio.rss";

            var httpClient = new HttpClient(new NativeMessageHandler());
            //httpClient.DefaultRequestHeaders.Add("x-ms-date", DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture));
            //httpClient.DefaultRequestHeaders.Add("x-ms-version", "2009-09-19");
            //httpClient.DefaultRequestHeaders.Add("DataServiceVersion", "1.0;NetFx");
            //httpClient.DefaultRequestHeaders.Add("MaxDataServiceVersion", "1.0;NetFx");
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

        public async Task<List<Video>> getVideoList()
        {
            string feed = "http://versolstore.blob.core.windows.net/cafan/videos/video.rss";

            var httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/atom+xml"));
            httpClient.DefaultRequestHeaders.Add("x-ms-date", DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture));
            httpClient.DefaultRequestHeaders.Add("x-ms-version", "2009-09-19");
            httpClient.DefaultRequestHeaders.Add("Authorization", "");
            //httpClient.DefaultRequestHeaders.Add("DataServiceVersion", "1.0;NetFx");
            //httpClient.DefaultRequestHeaders.Add("MaxDataServiceVersion", "1.0;NetFx");
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
                        //Link = (string)item.Element("link")+(string)item.Element("enclosure").Attribute("url"),
                        SubTitle = (string)item.Elements().Where(y => y.Name.LocalName == "subtitle").FirstOrDefault(),
                        PubDate = (string)item.Element("pubDate")
                    }).ToList<Video>();
        }

        private static string SignThis(string StringToSign)
        {
            string signature = string.Empty;
            byte[] unicodeKey = Convert.FromBase64String("4iCTvxoew7To5edkL8o9NEdsDNxb/UHUKnxq5KuvzCATEgrnyZAxARTIc/ylA/3GvvjTs0rAtyxnxAQssJV9Hg==");
            using (HMACSHA256 hmacSha256 = new HMACSHA256(unicodeKey))
            {
                byte[] dataToHmac = System.Text.Encoding.UTF8.GetBytes(StringToSign);
                signature = Convert.ToBase64String(hmacSha256.ComputeHash(dataToHmac));
            }

            string authorizationHeader = string.Format(
                  CultureInfo.InvariantCulture,
                  "{0} {1}:{2}",
                  "SharedKey",
                  "versolstore",
                  signature);

            return authorizationHeader;
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