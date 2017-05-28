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
using Java.Net;
using Android.Support.V4.App;
using Android.Graphics;
using System.IO;
using Newtonsoft.Json;
using System.Threading;

namespace DivineVerITies.Helpers
{
    [Service]
    public class DownloadService : IntentService
    {
        //public const int UPDATE_PROGRESS = 8344;
        public static CancellationTokenSource cancellation;
        private string filename;
        private string filetype;
        private string specificDir;
        NotificationCompat.Builder builder;
        NotificationManager notificationManager;
        public Context context;
        public AudioList selectedAudio;
        public Video selectedVideo;
        //Bundle resultData;
        int id = 2000;

        public DownloadService() : base("DownloadService")
        {
        }

        protected override void OnHandleIntent(Intent intent)
        {
            string urlToDownload = intent.GetStringExtra("url");
            string type = intent.GetStringExtra("type");
            if (type == "audio")
            {
                selectedAudio = JsonConvert.DeserializeObject<AudioList>(intent.GetStringExtra("selectedAudio"));
            }
            else if (type == "video")
            {
                selectedVideo = JsonConvert.DeserializeObject<Video>(intent.GetStringExtra("selectedVideo"));
            }
            
            context = ApplicationContext;
            //ResultReceiver receiver = (ResultReceiver)intent.GetParcelableExtra("receiver");

            var pintent = new Intent(context, typeof(CancelReceiver));
            builder = new NotificationCompat.Builder(context);
            if (type == "audio")
            {
                filetype = ".mp3";
                specificDir = "cafan/Podcasts/audio/";
                string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
                filename = path + selectedAudio.Title + filetype;
                pintent.PutExtra("filename", filename);
                builder.SetContentTitle(selectedAudio.Title);
            }
            else if (type == "video")
            {
                filetype = ".mp4";
                specificDir = "cafan/Podcasts/video/";
                string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
                filename = path + selectedVideo.Title + filetype;
                pintent.PutExtra("filename", filename);
                builder.SetContentTitle(selectedVideo.Title);
            }

            intent.SetAction(CancelReceiver.cancel);
            PendingIntent pIntent = PendingIntent.GetBroadcast(context, 0, pintent, PendingIntentFlags.UpdateCurrent);

            builder
                .SetContentText("Download Starting")
                .SetLargeIcon(BitmapFactory.DecodeResource(context.Resources, Resource.Drawable.Logo_trans192))
                .SetSmallIcon(Resource.Drawable.ic_cloud_download)
                .SetPriority(NotificationCompat.PriorityMax)
                .SetDefaults(NotificationCompat.DefaultAll)
                .AddAction(Resource.Drawable.ic_cancel, "Cancel", pIntent)
                .SetProgress(0, 0, true);

            if ((int)Build.VERSION.SdkInt >= 21)
            {
                builder.SetCategory(NotificationCompat.CategoryProgress);
                builder.SetVisibility(NotificationCompat.VisibilityPublic);
            }
            else
            {
                builder.SetVisibility(3);
                builder.SetCategory(Notification.CategoryProgress);
            }

            notificationManager =
                context.GetSystemService(NotificationService) as NotificationManager;

            notificationManager.Notify(id, builder.Build());


            try
            {
                cancellation = new CancellationTokenSource();
                URL url = new URL(urlToDownload);
                URLConnection connection = url.OpenConnection();
                connection.Connect();
                // this will be useful so that you can show a typical 0-100% progress bar
                int fileLength = connection.ContentLength;

                // download the file
                Stream input = new BufferedStream(connection.InputStream);
                Stream output = new FileStream(filename, FileMode.Create, FileAccess.Write);

                cancellation.Token.Register(input.Close);
                byte[] data = new byte[1024];
                long total = 0;
                int count;
                while ((count = input.Read(data, 0, data.Length)) != -1 && fileLength > total)
                {
                    if (cancellation.IsCancellationRequested)
                    {
                        builder.SetContentTitle("Download Interrupted")
                       .SetContentText("Download was Cancelled")
                       .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192))
                       .SetSmallIcon(Resource.Drawable.ic_cloud_download)
                       .SetVisibility(3)
                       .SetCategory(NotificationCompat.CategoryError)
                       .SetDefaults(3);
    
                        notificationManager.Notify(id, builder.Build());
                        if (File.Exists(filename))
                        {
                            File.Delete(filename);
                        }
                        return;
                    }

                    total += count;
                    // publishing the progress....
                    //resultData = new Bundle();
                    //resultData.PutInt("progress", (int)(total * 100 / fileLength));
                    //receiver.Send(UPDATE_PROGRESS, resultData);

                    var per = (int)(total * 100 / fileLength);
                    
                    builder.SetProgress(100, per, false);
                    builder.SetDefaults(NotificationCompat.PriorityDefault);
                    builder.SetContentText($"Downloaded ({ per }%)");
                    notificationManager.Notify(id, builder.Build());

                    output.Write(data, 0, count);
                }

                output.Flush();
                output.Close();
                input.Close();
            }
            catch (IOException e)
            {
                Console.WriteLine(e.ToString());
                Console.Write(e.StackTrace);
            }
            catch(ObjectDisposedException o)
            {
                builder.SetContentTitle("Download Interrupted")
                      .SetContentText("Download was Cancelled")
                      .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192))
                      .SetSmallIcon(Resource.Drawable.ic_cloud_download)
                      .SetCategory(NotificationCompat.CategoryError)
                      .SetVisibility(3)
                      .SetDefaults(3);

                notificationManager.Notify(id, builder.Build());
                if (File.Exists(filename))
                {
                    File.Delete(filename);
                }
            }
            catch(Exception ex)
            {
                Toast.MakeText(context, ex.Message, ToastLength.Long).Show();
            }

            //resultData = new Bundle();
            //resultData.PutInt("progress", 100);
            //receiver.send(UPDATE_PROGRESS, resultData);

            builder.SetContentText("Download complete");
            // Removes the progress bar
            builder.SetProgress(0, 0, false);
            builder.SetDefaults(NotificationCompat.DefaultAll);
            notificationManager.Notify(id, builder.Build());
        }
    }

}
