using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DivineVerITies.Helpers
{
    [Service]
    [IntentFilter(new[] { Cancel, StartD, Resume })]
    public class MyService : Service
    {
        private static int previousPer=0;
        bool choiceMade = true;
        public static Context contxt;
        public static CancellationTokenSource cts= new CancellationTokenSource();
        public static AudioList selectedAudio;
        private static int notificationId = 1;        
        public static Dictionary<string, int> notificationIds=new Dictionary<string,int>();
        public static Dictionary<string, CancellationTokenSource> cancellations=new Dictionary<string,CancellationTokenSource>();
        public static string filename;
        public const string Cancel = "com.xamarin.action.CANCEL";
        public const string StartD = "com.xamarin.action.STARTD";
        public const string Resume = "com.xamarin.action.RESUME";
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            switch (intent.Action)
            {
                case Cancel:
                    try
                    {
                        cts.Cancel();
                    }
                    catch (Exception e)
                    {

                    }
                    finally { cancel(); }
                    
                    break;
                case StartD:
                    startDownload();
                    break;
                case Resume:
                    break;
            }
            return StartCommandResult.Sticky;
        }
        private void cancel()
        {
            if (File.Exists(filename))
                File.Delete(filename);
            NotificationManager notificationManager =
                        GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            //const int notificationId = 0;
            notificationManager.Cancel(notificationIds[filename]);
       
        }
        public static void cancel(int id)
        {
            NotificationManager notificationManager =
                       contxt.GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            //const int notificationId = 0;
            notificationManager.Cancel(id);
        }
        public void startDownload()
        {
            var dwn = new Download();
            
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(contxt);
            builder.SetTitle("Confirm Download")
           .SetMessage("Are You Sure You Want To Download" + " " + MyService.selectedAudio.SubTitle)
           .SetPositiveButton("Yes", async delegate
            {
                Progress<DownloadBytesProgress> progressReporter = new Progress<DownloadBytesProgress>();
                progressReporter.ProgressChanged += (s, args) =>
                {
                    var name = filename;
                    if (args.IsFinished)
                    {
                        dComplete(name);
                    }
                    else if (cancellations[name].IsCancellationRequested)
                    {
                        if (notificationIds.ContainsKey(name)) 
                        {
                            pBarCancelled(name);
                            cancellations.Remove(name);
                            if (File.Exists(name))
                            {
                                File.Delete(name);
                            }
                        }
                        
                    }
                    else
                    {
                        
                        int per = (int)(100 * args.PercentComplete);
                        if ((per - previousPer) >= 2)
                        {
                            ChangePBar(per, name);
                            previousPer = per;
                        }
                    }
                };
                builder.Dispose();
                if ((await FileCheck()).Equals("yes"))
                {
                    if (!notificationIds.ContainsKey(filename))
                    {
                        notificationIds.Add(filename, notificationId);
                        notificationId++;
                        cancellations.Add(filename, dwn.cts);
                    }                   
                    DownLoadItemNotification(filename);                    
                    await dwn.CreateDownloadTask(MyService.selectedAudio.Link, filename, progressReporter, contxt);
                   
                }; 
            })
           .SetNegativeButton("No", delegate { builder.Dispose(); });
            builder.Create().Show();
        }
        private async Task<string> FileCheck()
        {
            string choice="yes";
            if (!Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/cafan/Podcasts/audio/"))
                Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/cafan/Podcasts/audio/");

            if (File.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/cafan/Podcasts/audio/"
                + DivineVerITies.Helpers.MyService.selectedAudio.Title + ".mp3")) 
            { 
                choiceMade = false;
                CreateAndShowDialog(choice); }

            filename = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/cafan/Podcasts/audio/"
                + DivineVerITies.Helpers.MyService.selectedAudio.Title + ".mp3";
            while (!choiceMade)
            {
                await Task.Delay(1000);
            }
            return choice;
        }
        private void deleteFile(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
        }
        private void CreateAndShowDialog(string choice)
        {
            choice=string.Empty;
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(contxt);
            builder.SetTitle("Confirm Download")
           .SetMessage(MyService.selectedAudio.SubTitle + " already exists. Do you wish to overwrite the existing file?")
           .SetPositiveButton("Yes", delegate
           {
               deleteFile(filename);
               choice = "yes";
               choiceMade = true;
               builder.Dispose();
               
               
           })
           .SetNegativeButton("No", delegate { choice = "no";
           choiceMade = true;
               builder.Dispose(); });
            builder.Create().Show();
            
        }
        private void DownLoadItemNotification(string name)
        {
            var intent = new Intent(contxt, typeof(CancelReceiver));
            
            intent.PutExtra("filename", name);
            intent.SetAction(CancelReceiver.cancel);
            PendingIntent pIntent = PendingIntent.GetBroadcast(contxt, notificationIds[name], intent, PendingIntentFlags.UpdateCurrent);
            
            // Instantiate the builder and set notification elements:
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(contxt)
                .SetContentTitle("Downloading Podcast")
                .SetContentText("Download Starting")
                .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192))
                .SetSmallIcon(Resource.Drawable.ic_cloud_download)
                //.SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate)
                .SetDefaults(3)
                .SetPriority(2)
                //.SetVisibility (NotificationVisibility.Public)
                .SetVisibility(3)
                .SetCategory(Notification.CategoryProgress)
                .AddAction(Resource.Drawable.ic_cancel, "Cancel", pIntent)                
                //.AddAction()
                //Initialize the download
                .SetProgress(0, 0, true);
            // Build the notification:
            Notification notification = builder.Build();
            // Get the notification manager:
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;
            // Publish the notification:
            
            notificationManager.Notify(notificationIds[name], notification);
        }
        private void ChangePBar(int per,string name)
        {
            var intent = new Intent(contxt,typeof(CancelReceiver));
            intent.PutExtra("filename", name);
            intent.SetAction(CancelReceiver.cancel);
            PendingIntent pIntent = PendingIntent.GetBroadcast(contxt, notificationIds[name], intent, PendingIntentFlags.UpdateCurrent);
            // Instantiate the builder and set notification elements:
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(contxt)
            .SetContentTitle("Downloading Podcast")
            .SetContentText("Downloaded (" + per + "% )")
            .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192))
            .SetSmallIcon(Resource.Drawable.ic_cloud_download)
            .SetPriority(0)
            .SetVisibility(3)
            .SetCategory(Notification.CategoryProgress)
            .AddAction(Resource.Drawable.ic_cancel, "Cancel", pIntent)
            .SetProgress(100, per, false);
            // Get the notification manager:
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;
            // Publish the notification:
            
            notificationManager.Notify(notificationIds[name], builder.Build());
        }
        private void dComplete(string name)
        {
            cancellations.Remove(name);
            
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(contxt)
                .SetContentTitle("Done")
                .SetContentText("Download complete")
                //.SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate)
                .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192))
                .SetSmallIcon(Resource.Drawable.ic_cloud_download)
                .SetVisibility(3)
                .SetCategory(Notification.CategoryProgress)
                .SetDefaults(3)
                .SetPriority(2)
                
                // Removes the progress bar
                .SetProgress(0, 0, false);
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;
            // Publish the notification:
            
            notificationManager.Notify(notificationIds[name], builder.Build());
            notificationIds.Remove(name);
            cancellations.Remove(name);
        }
        private void pBarCancelled(string name)
        {
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(contxt)
                .SetContentTitle("Download Interrupted")
                .SetContentText("Download was Cancelled")
                .SetLargeIcon(BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192))
                .SetSmallIcon(Resource.Drawable.ic_cloud_download)
                .SetVisibility(3)
                .SetCategory(Notification.CategoryProgress)
                .SetDefaults(3)
                .SetPriority(2)
                // Removes the progress bar
                
                .SetProgress(0, 0, false);
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;
            // Publish the notification:
            
            notificationManager.Notify(notificationIds[name], builder.Build());
            if (notificationIds.ContainsKey(name) && cancellations.ContainsKey(name))
            {
                notificationIds.Remove(name);
                cancellations.Remove(name);
            }
        }
    }
}