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
using System.IO;
using Newtonsoft.Json;

namespace DivineVerITies.Helpers
{
    public class FileChecker
    {
        private string type;
        private AudioList selectedAudio;
        private Video selectedVideo;
        private string audioFileName;
        private string videoFileName;
        private Context context;

        public FileChecker(Context context, string type, AudioList audio)
        {
            this.context = context;
            this.type = type;
            selectedAudio = audio;
        }

        public FileChecker(Context context, string type, Video video)
        {
            this.context = context;
            this.type = type;
            selectedVideo = video;
        }

        public bool FileExists()
        {
            string filetype;
            string specificDir;
            if (type == "audio")
            {
                filetype = ".mp3";
                specificDir = "cafan/Podcasts/audio/";
                string path = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
                string filename = path + selectedAudio.Title + filetype;
                if (!Directory.Exists(path))
                {

                    Directory.CreateDirectory(path);

                }
                if (File.Exists(path + selectedAudio.Title + filetype))
                {
                    return true;
                }

                audioFileName = filename;
            }
            else
            {
                filetype = ".mp4";
                specificDir = "cafan/Podcasts/video/";
                string path = System.IO.Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
                string filename = path + selectedVideo.Title + filetype;
                if (!Directory.Exists(path))
                {

                    Directory.CreateDirectory(path);

                }
                if (File.Exists(path + selectedVideo.Title + filetype))
                {
                    return true;
                }

                videoFileName = filename;
            }

            return false;
        }

        private void deleteFile(string filename)
        {
            if (File.Exists(filename))
                File.Delete(filename);
        }

        public void CreateAndShowDialog(AudioList audio)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(context);
            builder.SetTitle("Confirm Download")
           .SetMessage(audio.Title + " already exists. Do you wish to overwrite the existing file?")
           .SetPositiveButton("Yes", delegate
           {
               //string filetype = ".mp3";
               //string specificDir = "cafan/Podcasts/audio/";
               //string path = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
               //string filename = path + audio.Title + filetype;
               deleteFile(audioFileName);

               //Download file
               //new DownloadTask(context, audio).Execute(audio.Link);
               var dwnIntent = new Intent(context, typeof(DownloadService));
               dwnIntent.PutExtra("url", audio.Link);
               dwnIntent.PutExtra("type", "audio");
               dwnIntent.PutExtra("selectedAudio", JsonConvert.SerializeObject(audio));
               context.StartService(dwnIntent);

               builder.Dispose();
           })
           .SetNegativeButton("No", delegate { builder.Dispose(); });
            builder.Create().Show();

        }
        public void CreateAndShowDialog(Video video)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(context);
            builder.SetTitle("Confirm Download")
           .SetMessage(video.Title + " already exists. Do you wish to overwrite the existing file?")
           .SetPositiveButton("Yes", delegate
           {
               //string filetype = ".mp4";
               //string specificDir = "cafan/Podcasts/video/";
               //string path = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, specificDir);
               //string filename = path + video.Title + filetype;
               deleteFile(videoFileName);

               //Download file here
               //new DownloadTask(context, video).Execute(video.Link);
               
               var dwnIntent = new Intent(context, typeof(DownloadService));
               dwnIntent.PutExtra("url", video.Link);
               dwnIntent.PutExtra("type", "video");
               dwnIntent.PutExtra("selectedVideo", JsonConvert.SerializeObject(video));
               context.StartService(dwnIntent);


               builder.Dispose();
           })
           .SetNegativeButton("No", delegate { builder.Dispose(); });
            builder.Create().Show();

        }
    }
}