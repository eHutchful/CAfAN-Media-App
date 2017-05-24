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
using System.IO;
using Android.Graphics;
using Android.Support.V4.App;
using Java.Lang;
using System.Threading.Tasks;

namespace DivineVerITies.Helpers
{
    public class DownloadTask : AsyncTask<string, int, string>
    {

        private Context context;
        private PowerManager.WakeLock mWakeLock;
        private AudioList selectedAudio;
        private Video selectedVideo;
        private string type;
        NotificationCompat.Builder builder;
        NotificationManager notificationManager;
        private int id = 200;
        private string filetype;
        private string specificDir;
        private string filename;

        public DownloadTask(Context context, AudioList audio)
        {
            this.context = context;
            selectedAudio = audio;
            type = "audio";
        }

        public DownloadTask(Context context, Video video)
        {
            this.context = context;
            selectedVideo = video;
            type = "video";
        }

        public DownloadTask(Context context)
        {
            this.context = context;
        }

        //private bool FileExists()
        //{
        //    string filetype;
        //    string specificDir;
        //    if (type == "audio")
        //    {
        //        filetype = ".mp3";
        //        specificDir = "cafan/Podcasts/audio/";
        //        string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
        //        string filename = path + selectedAudio.Title + filetype;
        //        if (!Directory.Exists(path))
        //        {

        //            Directory.CreateDirectory(path);

        //        }
        //        if (File.Exists(path + selectedAudio.Title + filetype))
        //        {
        //            return true;             
        //        }

        //        audioFileName = filename;
        //    }
        //    else
        //    {
        //        filetype = ".mp4";
        //        specificDir = "cafan/Podcasts/video/";
        //        string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
        //        string filename = path + selectedVideo.Title + filetype;
        //        if (!Directory.Exists(path))
        //        {

        //            Directory.CreateDirectory(path);

        //        }
        //        if (File.Exists(path + selectedVideo.Title + filetype))
        //        {
        //            return true;
        //        }

        //        videoFileName = filename;
        //    }

        //    return false;
        //}

        protected override string RunInBackground(params string[] sUrl)
        {
            Stream input = null;
            Stream output = null;
            HttpURLConnection connection = null;
            try
            {
                URL url = new URL(sUrl[0]);
                connection = (HttpURLConnection)url.OpenConnection();
                connection.Connect();

                // expect HTTP 200 OK, so we don't mistakenly save error report
                // instead of the file
                if (connection.ResponseCode != HttpStatus.Ok)
                {
                    return "Server returned HTTP " + connection.ResponseCode + " " + connection.ResponseMessage;
                }

                // this will be useful to display download percentage
                // might be -1: server did not report the length
                int fileLength = connection.ContentLength;

                // download the file
                input = connection.InputStream;

                if (type == "audio")
                {
                    //filetype = ".mp3";
                    //specificDir = "cafan/Podcasts/audio/";
                    //string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
                    //filename = path + selectedAudio.Title + filetype;
                    output = new FileStream(filename, FileMode.Create, FileAccess.Write);
                }
                else if (type == "video")
                {
                    //filetype = ".mp4";
                    //specificDir = "cafan/Podcasts/video/";
                    //string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
                    //string filename = path + selectedVideo.Title + filetype;
                    output = new FileStream(filename, FileMode.Create, FileAccess.Write);
                }

                byte[] data = new byte[4096];
                long total = 0;
                int count;
                while ((count = input.Read(data, 0, data.Length)) != -1 && fileLength > total)
                {
                    // allow canceling with back button
                    if (IsCancelled)
                    {
                        input.Close();
                        return "Download Cancelled";
                    }
                    total += count;
                    // publishing the progress....
                    
                    if (fileLength > 0) // only if total length is known
                    {
                        var per = (int)(total * 100 / fileLength);
                        PublishProgress(per);
                        builder.SetProgress(100, per, false);
                        builder.SetDefaults(NotificationCompat.PriorityDefault);
                        builder.SetContentText($"Downloaded ({ per }%)");
                        notificationManager.Notify(id, builder.Build());
                    }
                    output.Write(data, 0, count);

                   
                }
            }
            catch (System.Exception e)
            {
                return e.ToString();
            }
            finally
            {
                try
                {
                    if (output != null)
                    {
                        output.Close();
                    }
                    if (input != null)
                    {
                        input.Close();
                    }
                }
                catch (IOException)
                {
                }

                if (connection != null)
                {
                    connection.Disconnect();
                }
            }
            return "File downloaded";
        }

        protected override void OnPreExecute()
        {
            base.OnPreExecute();

            // take CPU lock to prevent CPU from going off if the user 
            // presses the power button during download
            PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
            mWakeLock = pm.NewWakeLock(WakeLockFlags.Partial, GetType().FullName);
            mWakeLock.Acquire();
                
            //Display notification
            var intent = new Intent(context, typeof(CancelReceiver));
            builder = new NotificationCompat.Builder(context);
            if (type == "audio")
            {
                filetype = ".mp3";
                specificDir = "cafan/Podcasts/audio/";
                string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
                filename = path + selectedAudio.Title + filetype;
                intent.PutExtra("filename", filename);
                builder.SetContentTitle(selectedAudio.Title);
            }
            else if (type == "video")
            {
                filetype = ".mp4";
                specificDir = "cafan/Podcasts/video/";
                string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
                string filename = path + selectedVideo.Title + filetype;
                intent.PutExtra("filename", filename);
                builder.SetContentTitle(selectedVideo.Title);
            }

            intent.SetAction(CancelReceiver.cancel);
            PendingIntent pIntent = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);

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
                context.GetSystemService(Context.NotificationService) as NotificationManager;

            notificationManager.Notify(id, builder.Build());
           
        }

        protected override void OnPostExecute(string result)
        {
            base.OnPostExecute(result);
            mWakeLock.Release();

            builder.SetContentText("Download complete");
            // Removes the progress bar
            builder.SetProgress(0, 0, false);
            builder.SetDefaults(NotificationCompat.DefaultAll);
            notificationManager.Notify(id, builder.Build());

            if (result == "File Downloaded" || result == "Download Cancelled")
            {
                Toast.MakeText(context, result, ToastLength.Long).Show();                
            }
            else
            {
                Toast.MakeText(context, "Download error: " + result, ToastLength.Long).Show();
            }
        }

        protected override void OnProgressUpdate(params int[] values)
        {
            base.OnProgressUpdate(values);
            // Update progress
        }
    }
}