using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Media.Session;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using Com.Sothree.Slidinguppanel;
using DivineVerITies.ExoPlayer;
using DivineVerITies.Fragments;
using DivineVerITies.Helpers;
using Gcm.Client;
using Java.Interop;
using System;
using System.Threading;
using System.Threading.Tasks;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;

namespace DivineVerITies
{
    public delegate void VisibilityChangedHandler(object sender,EventArgs args);
    [Activity(Theme = "@style/Theme.DesignDemo", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainApp : AppCompatActivity
    {

        #region playerFields
        public static AudioList selectedAudio;
        public static ProgressBar loadingBar;
        private static ImageView artworkView;
        public string filename;
        private View audio_player_view;
        private SupportToolbar toolBar;
        private TextView position;
        private TextView duration;
        public static event VisibilityChangedHandler visibilityChanged; 
        private SeekBar seekbar;
        private ImageButton rewButton;
        public static ImageButton playPauseButton;
        private ImageButton forButton;
        private ImageButton stopButton;
        private ImageButton downloadButton;
        public static ViewStates visibility { 
            set 
            {
                if (value==ViewStates.Visible)
                {
                    visibilityChanged(null,null);
                }
            }
        }
        public static RelativeLayout layout;
        public bool isBound = false;
        public MediaPlayerServiceBinder binder;
        MediaPlayerServiceConnection mediaPlayerServiceConnection;
        private Intent audioServiceIntent;
        public event StatusChangedEventHandler StatusChanged;
        public event CoverReloadedEventHandler CoverReloaded;
        public event PlayingEventHandler Playing;
        public event BufferingEventHandler Buffering;
        #endregion
        public static SlidingUpPanelLayout mLayout;
        private DrawerLayout mDrawerLayout;
        private TabLayout tabs;
        private CancelReceiver CR= new CancelReceiver();
        private int[] tabIcons = {
            Resource.Drawable.ic_library_music,
            Resource.Drawable.ic_video_library,
            Resource.Drawable.ic_favorite_border,
            Resource.Drawable.ic_cloud_download
            //Resource.Drawable.ic_explore
                                 };
        
        static MainApp instance = new MainApp();

        // Return the current activity instance.
        public static MainApp CurrentActivity
        {
            get
            {
                return instance;
            }
        }

        ViewPager viewPager;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            #region original
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.MainApp);
            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);
            SupportActionBar.Title = "DivineVerITies";
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            // Set the current instance of 
            instance = this;

            // Make sure the GCM client is set up correctly. 
            //GcmClient.CheckDevice(this);
            //GcmClient.CheckManifest(this);

            //// Register the app for push notifications. 
            //GcmClient.Register(this, PushBroadcastReceiver.senderIDs);

            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            if (navigationView != null)
            {
                SetUpDrawerContent(navigationView);
            }

            tabs = FindViewById<TabLayout>(Resource.Id.tabs);

            viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);

            SetUpViewPager(viewPager);

            tabs.SetupWithViewPager(viewPager);

            setupTabIcons();
            setSelectedTab();

            visibilityChanged += (sender, args) =>
            {
                if (MediaPlayerService.selectedAudio != null)
                {
                    mLayout.PanelHeight = toolBar.LayoutParameters.Height;
                    var txtrow1 = FindViewById<TextView>(Resource.Id.txtRow11);
                    txtrow1.Text = MediaPlayerService.selectedAudio.Title;
                    var txtrow2 = FindViewById<TextView>(Resource.Id.txtRow21);
                    txtrow2.Text = MediaPlayerService.selectedAudio.SubTitle;
                    var avatar = FindViewById<ImageView>(Resource.Id.avatar1);
                    Glide.With(this)
                        .Load(MediaPlayerService.selectedAudio.ImageUrl)
                        .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                        .Error(Resource.Drawable.ChurchLogo_Gray)
                        .DiskCacheStrategy(DiskCacheStrategy.All)
                        .Into(artworkView);
                    Glide.With(this)
                        .Load(MediaPlayerService.selectedAudio.ImageUrl)
                        .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                        .Error(Resource.Drawable.ChurchLogo_Gray )
                        .DiskCacheStrategy(DiskCacheStrategy.All)
                        .Into(avatar);
                }
               
                    layout.Visibility = ViewStates.Visible;
            };
            

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            //fab.Click += (o, e) =>
            //{
            //    View anchor = o as View;

            //    if (tabs.SelectedTabPosition == 0)
            //    {
            //        Snackbar.Make(anchor, "No Connection", Snackbar.LengthLong)
            //           .SetAction("Retry", v =>
            //           {
            //               //Do something here
            //               //Intent intent = new Intent(fab.Context, typeof(BottomSheetActivity));
            //               //StartActivity(intent);
            //           })
            //           .Show();
            //    }

            //    if (tabs.SelectedTabPosition == 1)
            //    {
            //        Snackbar.Make(anchor, "No Connection", Snackbar.LengthLong)
            //            .SetAction("Retry", v =>
            //            {
            //                //Do something here
            //                //Intent intent = new Intent(fab.Context, typeof(BottomSheetActivity));
            //                //StartActivity(intent);
            //            })
            //            .Show();
            //    }

            //    if (tabs.SelectedTabPosition == 2)
            //    {
            //        Android.Content.Intent intent = new Android.Content.Intent(this, typeof(SampleChooserActivity));
            //        StartActivity(intent);
            //    }
            //};

            //mOptions = FindViewById<ImageButton>(Resource.Id.img_options);
            #endregion
            #region slidingupcode
            mLayout = FindViewById<SlidingUpPanelLayout>(Resource.Id.sliding_layout);
            mLayout.SetParallaxOffset(100);
            mLayout.SetDragView(Resource.Id.dragview);
            layout = FindViewById<RelativeLayout>(Resource.Id.audioPlayerView);
            layout.Visibility=ViewStates.Gone;
            #endregion
            #region playerstuff
            MyService.contxt = this;
            
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
            if (MediaPlayerService.selectedAudio != null)
            {
                Glide.With(this)
                    .Load(MediaPlayerService.selectedAudio.ImageUrl)
                    .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                    .Error(Resource.Drawable.ChurchLogo_Gray)
                    .DiskCacheStrategy(DiskCacheStrategy.All)
                    .Into(artworkView);
            }
            
            loadingBar = FindViewById<ProgressBar>(Resource.Id.audio_player_loading);


            Playing += (object sender, EventArgs e) =>
            {
                seekbar.Max = binder.GetMediaPlayerService().Duration;
                seekbar.Progress = binder.GetMediaPlayerService().Position;

                position.Text = GetFormattedTime(binder.GetMediaPlayerService().Position);
                duration.Text = GetFormattedTime(binder.GetMediaPlayerService().Duration);
                loadingBar.Visibility = ViewStates.Gone;
            };

            Buffering += (object sender, EventArgs e) =>
            {
                loadingBar.Visibility = ViewStates.Visible;
                seekbar.SecondaryProgress = binder.GetMediaPlayerService().Buffered;

            };

            StatusChanged += (object sender, EventArgs e) =>
            {
                var metadata = binder.GetMediaPlayerService().mediaControllerCompat.Metadata;
                if (metadata != null)
                {
                    RunOnUiThread(() =>
                    {
                        //title.Text = metadata.GetString(MediaMetadata.MetadataKeyTitle);
                        //subtitle.Text = metadata.GetString(MediaMetadata.MetadataKeyArtist);
                        playPauseButton.Selected = binder.GetMediaPlayerService().mediaControllerCompat.PlaybackState.State == PlaybackStateCompat.StatePlaying;
                    });
                }
            };

            downloadButton = FindViewById<ImageButton>(Resource.Id.audio_download);
            downloadButton.Click += downloadButton_Click;
            audio_player_view = FindViewById(Resource.Id.audioPlayerView);
           
            #endregion
        }

        private void setSelectedTab()
        {
            // Fetch the selected tab index with default
            int selectedTabIndex = Intent.GetIntExtra("SELECTED_TAB_EXTRA_KEY", 0);
            // Switch to page based on index
            viewPager.SetCurrentItem(selectedTabIndex, true);
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
            new Task(() =>
            {
                StartService(audioServiceIntent);
                BindService(audioServiceIntent, mediaPlayerServiceConnection, Bind.AutoCreate);
            }).Start();

        }
        private class MediaPlayerServiceConnection : Java.Lang.Object, IServiceConnection
        {
            MainApp instance;

            public MediaPlayerServiceConnection(MainApp mediaPlayer)
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
                    MainApp.playPauseButton.SetImageResource(binder.GetMediaPlayerService().playImage);
                    MainApp.loadingBar.Visibility = binder.GetMediaPlayerService().pbarState;
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

        private void setupTabIcons()
        {
            tabs.GetTabAt(0).SetIcon(tabIcons[0]);
            tabs.GetTabAt(1).SetIcon(tabIcons[1]);
            tabs.GetTabAt(2).SetIcon(tabIcons[2]);
            tabs.GetTabAt(3).SetIcon(tabIcons[3]);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    mDrawerLayout.OpenDrawer((int)GravityFlags.Left);
                    return true;

                //case Resource.Id.action_sort:
                //    return true;

                //case Resource.Id.action_appSettings:
                //    return true;

                case Resource.Id.action_signOut:
                    ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
                    ISharedPreferencesEditor edit = pref.Edit();
                    edit.Clear();
                    edit.Apply();

                    Intent intent = new Intent(this, typeof(MainActivity));
                    StartActivity(intent);
                    Finish();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.sample_actions, menu);
            return true;
        }

        private void SetUpDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (object sender, NavigationView.NavigationItemSelectedEventArgs e) =>
            {
                e.MenuItem.SetChecked(true);

                mDrawerLayout.CloseDrawers();

                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.nav_recommended:
                        Intent intent = new Intent(this, typeof(SelectTopics));
                        StartActivity(intent);
                        break;

                    case Resource.Id.nav_password:
                        intent = new Intent(this, typeof(ChangePassword));
                        StartActivity(intent);
                        break;
                }
            };
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            visibilityChanged = null;
        }

        private void SetUpViewPager(ViewPager viewPager)
        {
            TabAdapter adapter = new TabAdapter(SupportFragmentManager);
            adapter.AddFragment(new Fragment3(), "AUDIOS");
            adapter.AddFragment(new Fragment4(), "VIDEOS");
            adapter.AddFragment(new Fragment5(), "FAVOURITES");
            adapter.AddFragment(new Fragment7(), "DOWNLOADED");

            viewPager.Adapter = adapter;
        }
    }
}