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
using System.IO;

namespace DivineVerITies.Helpers
{
    [Service]
    public class DownloadService : IntentService
    {
        public DownloadService() : base("DownloadService")
        {

        }
        protected override void OnHandleIntent(Intent intent)
        {
            string filename = intent.GetStringExtra("filename");
            string destination = intent.GetStringExtra("destination");
            string uri = intent.GetStringExtra("uri");
            Progress<DownloadBytesProgress> progressReporter = new Progress<DownloadBytesProgress>();
            progressReporter.ProgressChanged += (s, args) =>
            {
                var name = filename;
                try
                {
                    if (args.IsFinished)
                    {
                        dComplete(name);
                    }
                    else if (MyService.cancellations[name].IsCancellationRequested)
                    {
                        if (MyService.notificationIds.ContainsKey(name))
                        {
                            pBarCancelled(name);
                            MyService.cancellations.Remove(name);
                            if (File.Exists(name))
                            {
                                File.Delete(name);
                            }
                        }

                    }
                    else
                    {

                        int per = (int)(100 * args.PercentComplete);
                        if ((per - args.prevPer) >= 2)
                        {
                            ChangePBar(per, name);
                            args.prevPer = per;
                        }
                    }
                }
                catch (KeyNotFoundException kn)
                {

                }

            };
            DownLoadItemNotification(filename);
            StartDownload(uri, filename, progressReporter);
        }
        private void StartDownload(string uri, string filename, Progress<DownloadBytesProgress> reporter)
        {
            var downloader = new Download();
            downloader.CreateDownloadTask(uri, filename, reporter, ApplicationContext);

        }
        private void dComplete(string name)
        {
            MyService.cancellations.Remove(name);
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(ApplicationContext)
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
            notificationManager.Notify(MyService.notificationIds[name], builder.Build());
            MyService.notificationIds.Remove(name);
            MyService.cancellations.Remove(name);
        }
        private void ChangePBar(int per, string name)
        {
            var intent = new Intent(ApplicationContext, typeof(CancelReceiver));
            intent.PutExtra("filename", name);
            intent.SetAction(CancelReceiver.cancel);
            PendingIntent pIntent = PendingIntent.GetBroadcast(ApplicationContext, MyService.notificationIds[name], intent, PendingIntentFlags.UpdateCurrent);
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(ApplicationContext)
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
            notificationManager.Notify(MyService.notificationIds[name], builder.Build());
        }
        private void DownLoadItemNotification(string name)
        {
            var intent = new Intent(ApplicationContext, typeof(CancelReceiver));

            intent.PutExtra("filename", name);
            intent.SetAction(CancelReceiver.cancel);
            PendingIntent pIntent = PendingIntent.GetBroadcast(ApplicationContext, MyService.notificationIds[name], intent, PendingIntentFlags.UpdateCurrent);
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(ApplicationContext)
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
            notificationManager.Notify(MyService.notificationIds[name], notification);
        }
        private void pBarCancelled(string name)
        {
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(ApplicationContext)
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
            notificationManager.Notify(MyService.notificationIds[name], builder.Build());
            if (MyService.notificationIds.ContainsKey(name) && MyService.cancellations.ContainsKey(name))
            {
                MyService.notificationIds.Remove(name);
                MyService.cancellations.Remove(name);
            }
        }
    }
}