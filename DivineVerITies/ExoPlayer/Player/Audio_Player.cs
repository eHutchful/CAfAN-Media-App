
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
using System.Threading;
using System.Threading.Tasks;
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
        public string filename;
        private View audio_player_view;
        private SupportToolbar toolBar;
        private TextView position;
        private TextView duration;
        
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

            MyService.contxt = this;
            // Create your application here
            SetContentView(Resource.Layout.BottomSheetPlayer);

            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            // Change To SubTitle Later
            SupportActionBar.Title = "Audio Playlist"; //MediaPlayerService.selectedAudio.Title;
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);
            SupportActionBar.SetHomeButtonEnabled(true);

            FrameLayout AudioPlayerLayout = FindViewById<FrameLayout>(Resource.Id.audioPlayerView);
            BottomSheetBehavior BSB = BottomSheetBehavior.From(AudioPlayerLayout);
            BSB.PeekHeight = 300;
            BSB.Hideable = false;
            BSB.SetBottomSheetCallback(new MyBottomSheetCallBack());

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
            fab.Click += (o, e) =>
            {
                if (BSB.State == BottomSheetBehavior.StateCollapsed)
                {
                    BSB.State = BottomSheetBehavior.StateExpanded;
                    fab.SetImageResource(Resource.Drawable.ic_expand_more);
                }
                else
                {
                    BSB.State = BottomSheetBehavior.StateCollapsed;
                    fab.SetImageResource(Resource.Drawable.ic_expand_more);
                }
            };

            if (mediaPlayerServiceConnection == null)
                InitilizeMedia();
            playPauseButton = FindViewById<ImageButton>(Resource.Id.audio_player_play_pause);            
            playPauseButton.Click += async (sender, args) =>
            {
                                
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
                if (mediaPlayerServiceConnection != null)
                    UnbindService(mediaPlayerServiceConnection);
                if (binder.GetMediaPlayerService() != null)
                    binder.GetMediaPlayerService().StopSelf();
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
            

            Playing += (object sender, EventArgs e) => {
                seekbar.Max = binder.GetMediaPlayerService().Duration;
                seekbar.Progress = binder.GetMediaPlayerService().Position;

                position.Text = GetFormattedTime(binder.GetMediaPlayerService().Position);
                duration.Text = GetFormattedTime(binder.GetMediaPlayerService().Duration);
                loadingBar.Visibility = ViewStates.Gone;
            };

            Buffering += (object sender, EventArgs e) => {
                loadingBar.Visibility = ViewStates.Visible;
                seekbar.SecondaryProgress = binder.GetMediaPlayerService().Buffered;
                
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

        public class MyBottomSheetCallBack : BottomSheetBehavior.BottomSheetCallback
        {
            public override void OnSlide(View bottomSheet, float slideOffset)
            {
                //Sliding
            }

            public override void OnStateChanged(View bottomSheet, int newState)
            {
                //State changed
            }
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
            new Task(() => {
                StartService(audioServiceIntent);
                BindService(audioServiceIntent, mediaPlayerServiceConnection, Bind.AutoCreate);
            }).Start();
                       
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
                    Audio_Player.playPauseButton.SetImageResource(binder.GetMediaPlayerService().playImage);
                    Audio_Player.loadingBar.Visibility = binder.GetMediaPlayerService().pbarState;
                    binder.GetMediaPlayerService().sContext = SynchronizationContext.Current;
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
            MyService.selectedAudio = selectedAudio;
            var intent = new Intent(ApplicationContext, typeof(MyService));
            intent.SetAction(MyService.StartD);
            StartService(intent);
            
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
        //public override bool OnCreateOptionsMenu(IMenu menu)
        //{
        //    MenuInflater.Inflate(Resource.Menu.audio_list, menu);
        //    return true;
        //}
        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                //case Resource.Id.action_audio_list:
                //    Intent intent = new Intent(this, typeof(MainApp));
                //    StartActivity(intent);
                //    break;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}