using Android;
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
    [IntentFilter(new[] { Cancel, StartD, Startvd })]
    public class MyService : IntentService
    {
        private static int previousPer=0;
        bool choiceMade = true;
        public static Context contxt;
        public static CancellationTokenSource cts= new CancellationTokenSource();
        public static AudioList selectedAudio;
        public static Video selectedVideo;
        private static int notificationId = 2;        
        public static Dictionary<string, int> notificationIds=new Dictionary<string,int>();
        public static Dictionary<string, CancellationTokenSource> cancellations=new Dictionary<string,CancellationTokenSource>();
        public static string filename;
        public static Queue<string> typeQueue = new Queue<string>();
        public static Queue<AudioList> audioQueue = new Queue<AudioList>();
        public static Queue<Video> videoQueue = new Queue<Video>();

        
        public const string Cancel = "com.xamarin.action.CANCEL";
        public const string StartD = "com.xamarin.action.STARTD";
        public const string Startvd = "com.xamarin.action.STARTVD";
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        //public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        //{
        //    switch (intent.Action)
        //    {
        //        case Cancel:
        //            try
        //            {
        //                cts.Cancel();
        //            }
        //            catch (Exception e)
        //            {

        //            }
        //            finally { cancel(); }
                    
        //            break;
        //        case StartD:
        //            startDownload();
        //            break;
        //        case Startvd:
        //            startDownload();
        //            break;
        //    }
        //    return StartCommandResult.Sticky;
        //}

        private void startVideoDownload()
        {
            var dwn = new VideoDownloader();

            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(contxt);
            builder.SetTitle("Confirm Download")
           .SetMessage("Are You Sure You Want To Download" + " " + selectedVideo.Title)
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
               if ((await videoFileCheck()).Equals("yes"))
               {
                   if (!notificationIds.ContainsKey(filename))
                   {
                       notificationIds.Add(filename, notificationId);
                       notificationId++;
                       cancellations.Add(filename, dwn.cts);
                   }
                   DownLoadItemNotification(filename);
                   await dwn.DownloadFileAsync(selectedVideo.Link, filename, progressReporter, contxt);

               };
           })
           .SetNegativeButton("No", delegate { builder.Dispose(); });
            builder.Create().Show();
        }
        private void cancel()
        {
            if (File.Exists(filename))
                File.Delete(filename);
            NotificationManager notificationManager =
                        GetSystemService(NotificationService) as NotificationManager;

            // Publish the notification:
            //const int notificationId = 0;
            notificationManager.Cancel(notificationIds[filename]);
       
        }
        public static void cancel(int id)
        {
            NotificationManager notificationManager =
                       contxt.GetSystemService(NotificationService) as NotificationManager;

            // Publish the notification:
            //const int notificationId = 0;
            notificationManager.Cancel(id);
        }
        public void startDownload()
        {
            if (typeQueue.Count == 0)
                return;
            var dwn = new VideoDownloader();
            string type="";
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(contxt);
            builder.SetTitle("Confirm Download");
            builder.SetNegativeButton("No", delegate { builder.Dispose(); });

           

            type = typeQueue.Peek();
            if (type == "audio")
            {
                selectedAudio = audioQueue.Peek();
                builder.SetMessage("Are You Sure You Want To Download" + " " + selectedAudio.Title);
            }
            else
            {
                selectedVideo = videoQueue.Peek();
                builder.SetMessage("Are You Sure You Want To Download" + " " + selectedVideo.Title);
            }
            builder.SetPositiveButton("Yes", async delegate
            {
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
                    }
                    catch (KeyNotFoundException kn)
                    {

                    }

                };
                builder.Dispose();
                if ((await FileCheck(type)).Equals("yes"))
                {
                    if (!notificationIds.ContainsKey(filename))
                    {
                        notificationIds.Add(filename, notificationId);
                        notificationId++;
                        cancellations.Add(filename, dwn.cts);
                    }
                    DownLoadItemNotification(filename);
                    //await dwn.CreateDownloadTask(MyService.selectedAudio.Link, filename, progressReporter, contxt);
                    if (type == "audio")
                    {
                        //await dwn.DownloadFileAsync(selectedAudio.Link, filename, progressReporter, contxt);
                        //await dwn.CreateDownloadTask(MyService.selectedAudio.Link, filename, progressReporter, contxt);
                    }
                    else
                    {
                        //await dwn.DownloadFileAsync(selectedVideo.Link, filename, progressReporter, contxt);
                        //await dwn.CreateDownloadTask(MyService.selectedAudio.Link, filename, progressReporter, contxt);
                    }


                };
            });
            builder.Create().Show();
        }
        private async Task<string> FileCheck(string type)
        {
            string choice="yes";
            string filetype;
            string specificDir;
            if (type == "audio")
            {
                filetype = ".mp3";
                specificDir = "cafan/Podcasts/audio/";
            }
            else
            {
                filetype = ".mp4";
                specificDir = "cafan/Podcasts/video/";
            }   
            string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
            if (!Directory.Exists(path))
            {
                
                Directory.CreateDirectory(path);

            }
            if (File.Exists(path + selectedAudio.Title + filetype)) 
            { 
                choiceMade = false;
                CreateAndShowDialog(choice); }

            filename = path + selectedAudio.Title + filetype;
            while (!choiceMade)
            {
                await Task.Delay(1000);
            }
            return choice;
        }
        private async Task<string> videoFileCheck()
        {
            string choice = "yes";
            if (!Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/cafan/Podcasts/video/"))
                Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/cafan/Podcasts/video/");

            if (File.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/cafan/Podcasts/video/"
                + selectedVideo.Title + ".mp4"))
            {
                choiceMade = false;
                videoCreateAndShowDialog(choice);
            }

            filename = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + "/cafan/Podcasts/video/"
                + selectedVideo.Title + ".mp4";
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
           .SetMessage(selectedAudio.Title + " already exists. Do you wish to overwrite the existing file?")
           .SetPositiveButton("Yes", delegate
           {
               deleteFile(filename);
               choice = "yes";
               choiceMade = true;
               builder.Dispose();
               
               
           })
           .SetNegativeButton("No", delegate {
               choice = "no";
           choiceMade = true;
               builder.Dispose(); });
            builder.Create().Show();
            
        }
        private void videoCreateAndShowDialog(string choice)
        {
            choice = string.Empty;
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(contxt);
            builder.SetTitle("Confirm Download")
           .SetMessage(selectedVideo.Title + " already exists. Do you wish to overwrite the existing file?")
           .SetPositiveButton("Yes", delegate
           {
               deleteFile(filename);
               choice = "yes";
               choiceMade = true;
               builder.Dispose();


           })
           .SetNegativeButton("No", delegate
            {
                choice = "no";
                choiceMade = true;
                builder.Dispose();
            });
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
            if(typeQueue.Count != 0)
            {
                var type = typeQueue.Dequeue();
                if (type == "audio")
                    audioQueue.Dequeue();
                else
                    videoQueue.Dequeue();
                startDownload();
            }
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
            if (typeQueue.Count != 0)
            {
                var type = typeQueue.Dequeue();
                if (type == "audio")
                    audioQueue.Dequeue();
                else
                    videoQueue.Dequeue();
                startDownload();
            }
        }

        protected override void OnHandleIntent(Intent intent)
        {
            throw new NotImplementedException();
        }
    }
}