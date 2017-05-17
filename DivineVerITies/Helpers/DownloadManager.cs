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
using System.Threading;
using System.Net.Http;
using ModernHttpClient;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace DivineVerITies.Helpers
{
    [Service]
    public class DownloadManager : IntentService
    {
        public static CancellationTokenSource cancellation;
        private static int prevper = 0;
        public DownloadManager() : base("Downloader")
        {

        }
        protected override async void OnHandleIntent(Intent intent)
        {
            string filename = MyService.filename;
            string link = "";
            if (MyService.selectedAudio == null)
            {
                link = MyService.selectedVideo.Link;
            }
            else
            {

                link = MyService.selectedAudio.Link;
            }
            Progress<DownloadBytesProgress> progressReporter = new Progress<DownloadBytesProgress>();
            progressReporter.ProgressChanged += (s, args) =>
            {
                try
                {
                    if (args.IsFinished)
                    {
                        dComplete();
                    }
                    else if (cancellation.IsCancellationRequested)
                    {
                            pBarCancelled();
                            if (File.Exists(filename))
                            {
                                File.Delete(filename);
                            }
                        return;
                    }
                    else
                    {

                        int per = (int)(100 * args.PercentComplete);
                        if ((per - prevper) >= 2)
                        {
                            ChangePBar(per,filename);
                            prevper = per;
                        }
                    }
                }
                catch (KeyNotFoundException kn)
                {

                }

            };
            cancellation = new CancellationTokenSource();
            DownLoadItemNotification(filename);
            //WebClient client = new WebClient();
            Stream stream;
            
            int receivedBytes = 0;
            int totalBytes = 0;
            List<byte> file = new List<byte>();
            try
            {

                HttpWebRequest aRequest = (HttpWebRequest)WebRequest.Create(link);
                HttpWebResponse aResponse = (HttpWebResponse)aRequest.GetResponse();
                stream = aResponse.GetResponseStream();
                cancellation.Token.Register(stream.Close);
                byte[] buffer = new byte[4096];
                totalBytes = Int32.Parse(aResponse.Headers[HttpRequestHeader.ContentLength]); 
                var filestream = File.Create(filename, totalBytes);
                for (;;)
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    file.AddRange(buffer);
                    if (bytesRead == 0)
                    {
                        break;
                    }
                    filestream.Write(buffer, 0, bytesRead);
                    receivedBytes += bytesRead;
                    if (progressReporter != null)
                    {
                        DownloadBytesProgress args = new DownloadBytesProgress(link, receivedBytes, totalBytes);
                        ((IProgress<DownloadBytesProgress>)progressReporter).Report(args);
                    }
                }
                stream.Close();
                filestream.Close();
                if (cancellation.IsCancellationRequested)
                {
                    pBarCancelled();
                    if (File.Exists(filename))
                    {
                        File.Delete(filename);
                    }
                    return;
                }
            }

            catch (ObjectDisposedException e)
            {
                pBarCancelled();
                return;
            }

            catch (Exception b)
            {
                downloadError();
                return;

            }

        }

        private void DownLoadItemNotification(string name)
        {
            var intent = new Intent(this, typeof(CancelReceiver));
            intent.PutExtra("filename", name);
            intent.SetAction(CancelReceiver.cancel);
            PendingIntent pIntent = PendingIntent.GetBroadcast(this, 100, intent, PendingIntentFlags.UpdateCurrent);
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(this)
                .SetContentTitle("Downloading Podcast")
                .SetContentText("Download Starting")
                .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192))
                .SetSmallIcon(Resource.Drawable.ic_cloud_download)
                .SetDefaults(3)
                .SetPriority(2)
                .SetVisibility(3)
                .SetCategory(Notification.CategoryProgress)
                .AddAction(Resource.Drawable.ic_cancel, "Cancel", pIntent)
                .SetProgress(0, 0, true);
            Notification notification = builder.Build();
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager.Notify(100, notification);
        }
        private void ChangePBar(int per, string name)
        {
            var intent = new Intent(this, typeof(CancelReceiver));
            intent.PutExtra("filename", name);
            intent.SetAction(CancelReceiver.cancel);
            PendingIntent pIntent = PendingIntent.GetBroadcast(this, 100, intent, PendingIntentFlags.UpdateCurrent);
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(this)
            .SetContentTitle("Downloading Podcast")
            .SetContentText("Downloaded (" + per + "% )")
            .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192))
            .SetSmallIcon(Resource.Drawable.ic_cloud_download)
            .SetPriority(0)
            .SetVisibility(3)
            .SetCategory(Notification.CategoryProgress)
            .AddAction(Resource.Drawable.ic_cancel, "Cancel", pIntent)
            .SetProgress(100, per, false);
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager.Notify(100, builder.Build());
        }
        private void dComplete()
        {
            
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(this)
                .SetContentTitle("Done")
                .SetContentText("Download complete")
                .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192))
                .SetSmallIcon(Resource.Drawable.ic_cloud_download)
                .SetVisibility(3)
                .SetCategory(Notification.CategoryProgress)
                .SetDefaults(3)
                .SetPriority(2)
                .SetProgress(0, 0, false);
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager.Notify(100, builder.Build());
           
           
        }
        private void pBarCancelled()
        {
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(this)
                .SetContentTitle("Download Interrupted")
                .SetContentText("Download was Cancelled")
                .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192))
                .SetSmallIcon(Resource.Drawable.ic_cloud_download)
                .SetVisibility(3)
                .SetCategory(Notification.CategoryProgress)
                .SetDefaults(3)
                .SetPriority(2)
                .SetProgress(0, 0, false);
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager.Notify(100, builder.Build());
           
        }
        private void downloadError()
        {
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(this)
               .SetContentTitle("Download Interrupted")
               .SetContentText("Download Error")
               .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192))
               .SetSmallIcon(Resource.Drawable.ic_cloud_download)
               .SetVisibility(3)
               .SetCategory(Notification.CategoryProgress)
               .SetDefaults(3)
               .SetPriority(2)
               .SetProgress(0, 0, false);
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;
            notificationManager.Notify(100, builder.Build());

        }
    }
}