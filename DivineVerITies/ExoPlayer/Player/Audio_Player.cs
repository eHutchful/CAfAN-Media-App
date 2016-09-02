
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Media;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Media.Session;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using DivineVerITies.Helpers;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;

namespace DivineVerITies.ExoPlayer.Player
{

    [Activity(Theme = "@style/Theme.DesignDemo")]
    public class Audio_Player : AppCompatActivity
    {
        public static AudioList selectedAudio;
        public static ProgressBar loadingBar;
        private ImageView artworkView;

        private View audio_player_view;
        private SupportToolbar toolBar;
        private TextView position;
        private TextView duration;

        //private bool shouldSetDuration;
        //private bool userInteracting;

        private SeekBar seekbar;        
        private ImageButton rewButton;
        public static ImageButton playPauseButton;
        private ImageButton forButton;
        private ImageButton stopButton;
        private ImageButton downloadButton;

        public bool isBound = false;
        public MediaPlayerServiceBinder binder;
        MediaPlayerServiceConnection mediaPlayerServiceConnection;
        private Intent audioServiceIntent;
        public event StatusChangedEventHandler StatusChanged;
        public event CoverReloadedEventHandler CoverReloaded;
        public event PlayingEventHandler Playing;
        public event BufferingEventHandler Buffering;
        

        protected override void OnCreate(Bundle savedInstanceState)
        {
            
            base.OnCreate(savedInstanceState);
           

            // Create your application here
            SetContentView(Resource.Layout.newtest);
            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            // Change To SubTitle Later
            SupportActionBar.Title = MediaPlayerService.selectedAudio.Title;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            if (mediaPlayerServiceConnection == null)
                InitilizeMedia();
            if(playPauseButton == null)
            {
                playPauseButton = FindViewById<ImageButton>(Resource.Id.audio_player_play_pause);
                var image = GetDrawable(Resource.Drawable.ic_play_arrow);
                Audio_Player.playPauseButton.SetImageDrawable(image);                
                playPauseButton.SetBackgroundColor(Color.Transparent);                
            }           
            
            playPauseButton.Click += async (sender, args) =>
            {
                loadingBar.Visibility = ViewStates.Visible;
                
                if (binder.GetMediaPlayerService().mediaPlayer != null && binder.GetMediaPlayerService().MediaPlayerState == PlaybackStateCompat.StatePlaying)
                {
                    await binder.GetMediaPlayerService().Pause();
                    
                }
                else
                {
                    await binder.GetMediaPlayerService().Play();
                    
                }
                    
            };

            rewButton = FindViewById<ImageButton>(Resource.Id.audio_player_reverse);
            //rewButton.SetImageBitmap(BitmapFactory.DecodeResource(Resources,Android.Resource.Drawable.IcMediaRew));
            rewButton.Click += async (sender, args) =>
            {
                if (binder.GetMediaPlayerService().mediaPlayer != null && 
                binder.GetMediaPlayerService().MediaPlayerState != PlaybackStateCompat.StateStopped)
                    await binder.GetMediaPlayerService().PlayPrevious();
            };

            forButton = FindViewById<ImageButton>(Resource.Id.audio_player_forward);
            //forButton.SetImageBitmap(BitmapFactory.DecodeResource(Resources, Android.Resource.Drawable.IcMediaFf));
            forButton.Click += async (sender, args) =>
            {
                if (binder.GetMediaPlayerService().mediaPlayer != null &&
                binder.GetMediaPlayerService().MediaPlayerState != PlaybackStateCompat.StateStopped)
                    await binder.GetMediaPlayerService().PlayNext();
            };
            stopButton = FindViewById<ImageButton>(Resource.Id.audio_player_stop);
            //stopButton.SetImageBitmap(BitmapFactory.DecodeResource(Resources, Android.Resource.Drawable.IcMediaFf));
            stopButton.Click += async (sender, args) =>
            {
                if (binder.GetMediaPlayerService().mediaPlayer != null &&
                binder.GetMediaPlayerService().MediaPlayerState != PlaybackStateCompat.StateStopped)
                    await binder.GetMediaPlayerService().Stop();
            };
            position = FindViewById<TextView>(Resource.Id.audio_player_position);
            duration = FindViewById<TextView>(Resource.Id.audio_player_duration);
            seekbar = FindViewById<SeekBar>(Resource.Id.audio_player_seek);
            artworkView = FindViewById<ImageView>(Resource.Id.audio_player_image);
            Glide.With(this)
                    .Load(MediaPlayerService.selectedAudio.ImageUrl)
                    .Placeholder(Resource.Drawable.Logo_trans192)
                    .Error(Resource.Drawable.Logo_trans192)
                    .DiskCacheStrategy(DiskCacheStrategy.All)
                    .Into(artworkView);
            loadingBar = FindViewById<ProgressBar>(Resource.Id.audio_player_loading);
            loadingBar.Visibility = ViewStates.Invisible;

            Playing += (object sender, EventArgs e) => {
                seekbar.Max = binder.GetMediaPlayerService().Duration;
                seekbar.Progress = binder.GetMediaPlayerService().Position;

                position.Text = GetFormattedTime(binder.GetMediaPlayerService().Position);
                duration.Text = GetFormattedTime(binder.GetMediaPlayerService().Duration);
                loadingBar.Visibility = ViewStates.Invisible;
            };

            Buffering += (object sender, EventArgs e) => {
                seekbar.SecondaryProgress = binder.GetMediaPlayerService().Buffered;
                loadingBar.Visibility = ViewStates.Visible;
            };

            StatusChanged += (object sender, EventArgs e) => {
                var metadata = binder.GetMediaPlayerService().mediaControllerCompat.Metadata;
                if (metadata != null)
                {
                    RunOnUiThread(() => {
                        //title.Text = metadata.GetString(MediaMetadata.MetadataKeyTitle);
                        //subtitle.Text = metadata.GetString(MediaMetadata.MetadataKeyArtist);
                        playPauseButton.Selected = binder.GetMediaPlayerService().mediaControllerCompat.PlaybackState.State == PlaybackStateCompat.StatePlaying;
                    });
                }
            };

            downloadButton = FindViewById<ImageButton>(Resource.Id.audio_download);
            downloadButton.Click += downloadButton_Click; 
            audio_player_view = FindViewById(Resource.Id.audioPlayerView);
           
            //ShowAudioSnackBar();
        }
        private string GetFormattedTime(int value)
        {
            var span = TimeSpan.FromMilliseconds(value);
            if (span.Hours > 0)
            {
                return string.Format("{0}:{1:00}:{2:00}", (int)span.TotalHours, span.Minutes, span.Seconds);
            }
            else
            {
                return string.Format("{0}:{1:00}", (int)span.Minutes, span.Seconds);
            }
        }
        private void InitilizeMedia()
        {
            audioServiceIntent = new Intent(ApplicationContext, typeof(MediaPlayerService));
            mediaPlayerServiceConnection = new MediaPlayerServiceConnection(this);
            BindService(audioServiceIntent, mediaPlayerServiceConnection, Bind.AutoCreate);
        }
        private class MediaPlayerServiceConnection : Java.Lang.Object, IServiceConnection
        {
            Audio_Player instance;

            public MediaPlayerServiceConnection(Audio_Player mediaPlayer)
            {
                this.instance = mediaPlayer;
            }

            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                var mediaPlayerServiceBinder = service as MediaPlayerServiceBinder;
                if (mediaPlayerServiceBinder != null)
                {
                    var binder = (MediaPlayerServiceBinder)service;
                    instance.binder = binder;
                    instance.isBound = true;

                    binder.GetMediaPlayerService().CoverReloaded += (object sender, EventArgs e) => { if (instance.CoverReloaded != null) instance.CoverReloaded(sender, e); };
                    binder.GetMediaPlayerService().StatusChanged += (object sender, EventArgs e) => { if (instance.StatusChanged != null) instance.StatusChanged(sender, e); };
                    binder.GetMediaPlayerService().Playing += (object sender, EventArgs e) => { if (instance.Playing != null) instance.Playing(sender, e); };
                    binder.GetMediaPlayerService().Buffering += (object sender, EventArgs e) => { if (instance.Buffering != null) instance.Buffering(sender, e); };
                }
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                instance.isBound = false;
            }
        }
        public void downloadButton_Click(object sender, System.EventArgs e)
        {
            var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle("Confirm Download")
           .SetMessage("Are You Sure You Want To Download" + " " + DivineVerITies.Helpers.AudioService.selectedAudio.SubTitle)
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
            if (!Directory.Exists(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/cafan/Podcasts/audio/"))
                Directory.CreateDirectory(Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/cafan/Podcasts/audio/");
            
            return Android.OS.Environment.ExternalStorageDirectory.AbsolutePath + @"/cafan/Podcasts/audio/"
                + DivineVerITies.Helpers.AudioService.selectedAudio.Title + ".mp3";
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
            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(this)
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
            const int notificationId = 0;
            notificationManager.Notify(notificationId, builder.Build());
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
            //var text = "";
            //switch (pState)
            //{ 
            //    case mState.Prepared:
            //        text = "prepared";
            //        break;
            //    case mState.End:
            //        text = "ended";
            //        break;
            //    case mState.Idle:
            //        text = "idle";
            //        break;
            //    case mState.Preparing:
            //        text = "preparing";
            //        break;
            //    case mState.Initialized:
            //        text = "initialized";
            //        break;
            //    default:
            //        text = "unknown";
            //        break;
            //}
            
            //if (text != string.Empty)
            //{
            //    Snackbar.Make(audio_player_view, text, Snackbar.LengthLong)
            //        .Show();
            //}
        }       
        protected override void OnStart()
        {
            base.OnStart();
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