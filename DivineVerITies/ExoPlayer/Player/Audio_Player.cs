
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using DivineVerITies.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;

namespace DivineVerITies.ExoPlayer.Player
{

    [Activity(Theme = "@style/Theme.DesignDemo")]
    public class Audio_Player : AppCompatActivity
    {
        //Bitmap imageBitmap = null;
        //private bool isVisible = true;
        private SupportToolbar toolBar;
        private ProgressBar loadingBar;
        private ImageView artworkView;
        //private TextView currentPositionView;
        //private TextView durationView;

        //private SeekBar seekBar;
        //private bool shouldSetDuration;
        //private bool userInteracting;
        private ImageButton previousButton;
        private ImageButton playPauseButton;
        private ImageButton nextButton;
        private ImageButton downloadButton;
        private mState pState;
        AudioList selectedAudio;
        private View audio_player_view;
        private MediaPlayer _player;

        private enum mState
        {
            Idle,
            Initialized,            
            Prepared,
            Started,
            Paused,
            Stopped,
            Preparing,
            End,
            Error,
            PlayBackCompleted
        }
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            selectedAudio = JsonConvert.DeserializeObject<AudioList>(Intent.GetStringExtra("selectedItem"));

            // Create your application here
            SetContentView(Resource.Layout.newtest);

            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            // Change To SubTitle Later
            SupportActionBar.Title = selectedAudio.Title;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            _player = new MediaPlayer();
            _player.SetAudioStreamType(Android.Media.Stream.Music);            
            pState = mState.Idle;

            playPauseButton = FindViewById<ImageButton>(Resource.Id.audio_player_play_pause);
            playPauseButton.Click += playPauseButton_Click;

            previousButton = FindViewById<ImageButton>(Resource.Id.audio_player_previous);
            previousButton.Click +=previousButton_Click;

            nextButton = FindViewById<ImageButton>(Resource.Id.audio_player_next);
            nextButton.Click += nextButton_Click;

            downloadButton = FindViewById<ImageButton>(Resource.Id.audio_download);
            downloadButton.Click += downloadButton_Click;
                

            _player.Prepared += (sender, args) => 
            {
                _player.Start();
                loadingBar = FindViewById<ProgressBar>(Resource.Id.audio_player_loading);
                loadingBar.Visibility = ViewStates.Gone;
               // artworkView.SetImageBitmap(imageBitmap); 
            };
            _player.Completion +=(sender,e)=>{_player.Stop();};

            audio_player_view = FindViewById(Resource.Id.audioPlayerView);
            ShowAudioSnackBar();
        }

         void downloadButton_Click(object sender, System.EventArgs e)
        {
            var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle("Confirm Download")
           .SetMessage("Are You Sure You Want To Download" + " " + selectedAudio.SubTitle)
           .SetPositiveButton("Yes", async delegate { 
                   
               Progress<DownloadBytesProgress> progressReporter = new Progress<DownloadBytesProgress>();
               DownLoadItemNotification();
               progressReporter.ProgressChanged += (s, args) =>
               {
                   if (args.IsFinished)
                   {
                       dComplete();
                   }
                   else if (MyService.cts.IsCancellationRequested)
                   {
                       pBarCancelled();
                   }
                   else
                   {
                       int per = (int)(100 * args.PercentComplete);
                       ChangePBar(per);
                   }
               };

               int bytesDownloaded = await Download.CreateDownloadTask(selectedAudio.Link, FileCheck(), progressReporter, this);
               
           })
           .SetNegativeButton("No", delegate { });
            builder.Create().Show();
        }

        private string FileCheck()
        {
            
            //var dir = new Java.IO.File(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/cafan/Podcasts/audio/");
            //if (!dir.Exists())
            //    dir.Mkdirs();
            if (!Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/cafan/Podcasts/audio/"))
                Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/cafan/Podcasts/audio/");
            //return dir.AbsolutePath +"/"+ selectedAudio.Title + ".mp3";
            return Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/cafan/Podcasts/audio/"
                + selectedAudio.Title + ".mp3";
        }
        
        private void DownLoadItemNotification()
        {
            var intent = new Intent(MyService.Cancel);
            PendingIntent pIntent = PendingIntent.GetService(this, 0, intent, PendingIntentFlags.UpdateCurrent);

            // Instantiate the builder and set notification elements:
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(this)
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
                .AddAction(Resource.Drawable.ic_cancel,"Cancel",pIntent)
                //.AddAction()
                //Initialize the download
                .SetProgress(0, 0, true);


            // Build the notification:
            Notification notification = builder.Build();

            // Get the notification manager:
            NotificationManager notificationManager =
                GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);


            
        }

        private void ChangePBar(int per)
        {

            var intent = new Intent(MyService.Cancel);
            PendingIntent pIntent = PendingIntent.GetService(this, 0, intent, PendingIntentFlags.UpdateCurrent);
            // Instantiate the builder and set notification elements:
           Android.Support.V4.App. NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(this)
           .SetContentTitle("Downloading Podcast")
                .SetContentText("Downloaded (" + per + "/100")
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
            const int notificationId = 0;
            notificationManager.Notify(notificationId, builder.Build());            
        }
        private void dComplete()
        {
            //download complete



            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(this).SetContentTitle("Done")
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
            const int notificationId = 0;
            notificationManager.Notify(notificationId, builder.Build());
        }
        void pBarCancelled()
        {
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(this).SetContentTitle("Download Interrupted")
                    .SetContentText("Download was Cancelled")
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
            const int notificationId = 0;
            notificationManager.Notify(notificationId, builder.Build());
        }
        private void ShowAudioSnackBar()
        {
            var text = "";
            switch (pState)
            { 
                case mState.Prepared:
                    text = "prepared";
                    break;
                case mState.End:
                    text = "ended";
                    break;
                case mState.Idle:
                    text = "idle";
                    break;
                case mState.Preparing:
                    text = "preparing";
                    break;
                case mState.Initialized:
                    text = "initialized";
                    break;
                default:
                    text = "unknown";
                    break;
            }
            
            if (text != string.Empty)
            {
                Snackbar.Make(audio_player_view, text, Snackbar.LengthLong)
                    .Show();
            }
        }

        void nextButton_Click(object sender, System.EventArgs e)
        {
            if(pState==mState.Prepared||pState==mState.Paused)
            {
                var i = _player.CurrentPosition;
                i += 1000;
                _player.SeekTo(i);

            }
            else if(pState==mState.PlayBackCompleted){}
            
        }

        private void previousButton_Click(object sender, System.EventArgs e)
        {
            if (pState == mState.Prepared || pState == mState.Paused)
            {
                var i = _player.CurrentPosition;
                i -= 1000;
                _player.SeekTo(i);

            }
            else if (pState == mState.PlayBackCompleted) { }
        }

        void playPauseButton_Click(object sender, System.EventArgs e)
        { 
            if(!_player.IsPlaying)
            {
                _player.Start();
                pState = mState.Started;
            }
            else
            {
                _player.Pause();
                pState = mState.Paused;
            }
        }

        protected async override void OnStart()
        {
            base.OnStart();
            //if (isVisible)
            //{

                artworkView = FindViewById<ImageView>(Resource.Id.audio_player_image);
                
                //using (var client = new HttpClient())
                //{
                //    var imageBytes = await client.GetByteArrayAsync(selectedAudio.ImageUrl);

                //    if (imageBytes != null && imageBytes.Length > 0)
                //    {
                //        imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
                //    }
                //}

                Glide.With(this)
                    .Load(selectedAudio.ImageUrl)
                    .Placeholder(Resource.Drawable.Logo_trans192)
                    .Error(Resource.Drawable.Logo_trans192)
                    //.SkipMemoryCache(true)
                    .DiskCacheStrategy(DiskCacheStrategy.All)
                    .Into(artworkView);

                //await _player.SetDataSourceAsync(ApplicationContext, Android.Net.Uri.Parse(selectedAudio.Link));
                //_player.PrepareAsync();
                
            //    isVisible = false;
            //}

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.audio_list, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                case Resource.Id.action_audio_list:
                    Intent intent = new Intent(this, typeof(MainApp));
                    StartActivity(intent);
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}