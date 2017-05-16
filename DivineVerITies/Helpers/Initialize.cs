using Android.Content;
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
            string feed = //"http://feeds.soundcloud.com/users/soundcloud:users:247218071/sounds.rss";
                "http://versolstore.blob.core.windows.net/cafan/audios/audio.rss";

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
                    }).ToList();
        }

        public async Task<List<Video>> getVideoList()
        {
            string feed = //"http://feeds.soundcloud.com/users/soundcloud:users:247218071/sounds.rss";
                "http://versolstore.blob.core.windows.net/cafan/videos/video.rss";

            var httpClient = new HttpClient();
            //httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/atom+xml"));
            //httpClient.DefaultRequestHeaders.Add("x-ms-date", DateTime.UtcNow.ToString("R", CultureInfo.InvariantCulture));
            //httpClient.DefaultRequestHeaders.Add("x-ms-version", "2009-09-19");
            //httpClient.DefaultRequestHeaders.Add("Authorization", "");
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
                    }).ToList();
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

        public static  List<Album> getAlbumList(List<AudioList>list)
        {
            List<Album> albums = new List<Album>();
            List<AudioList> holder = new List<AudioList>();
            holder.AddRange(list);
            while(holder.Count != 0)
            {
                var album = (from audio in holder where audio.SubTitle == holder[0].SubTitle select audio).ToList();
                for(int i = 0; i < album.Count; i++)
                {
                    holder.Remove(album[i]);
                }
                albums.Add(
                    new Album {
                        Title= album[0].SubTitle,
                        PubDate=album[0].PubDate,
                        ImageUrl=album[0].ImageUrl,
                        members=album,
                        Count=album.Count
                });
            }
            return albums;
        }
        public static List<AudioList> getFavouritesList(List<AudioList> list, Context context)
        {
            List<AudioList> albums = new List<AudioList>();
            List<AudioList> holder = new List<AudioList>();
            holder.AddRange(list);
            var pref = context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            var favourites = pref.GetStringSet("Favourites", new List<string>()).ToArray();
            for (int i = 0; i<favourites.Length; i++){
                var album = (from audio in holder where audio.Description == favourites[i] select audio).ToList();
                albums.AddRange(album);
            }
            
            return albums;
        }
        public static List<Video> getFavouritesList(List<Video> list, Context context)
        {
            List<Video> albums = new List<Video>();
            List<Video> holder = new List<Video>();
            holder.AddRange(list);
            var pref = context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            var favourites = pref.GetStringSet("Favourites", new List<string>()).ToArray();
            for (int i = 0; i < favourites.Length; i++)
            {
                var album = (from video in holder where video.Description == favourites[i] select video).ToList();
                albums.AddRange(album);
            }

            return albums;
        }
    }

    public class Album
    {
        public string Title;
        public string PubDate;
        public string ImageUrl;
        public int Count;
        public List<AudioList> members;
    }

}