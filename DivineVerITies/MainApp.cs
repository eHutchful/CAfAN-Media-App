using Android;
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
using Android.Runtime;

namespace DivineVerITies
{
    public delegate void VisibilityChangedHandler(object sender,EventArgs args);
    [Activity(Theme = "@style/Theme.DesignDemo", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainApp : AppCompatActivity, TabLayout.IOnTabSelectedListener
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
        ISharedPreferences pref;
        ISharedPreferencesEditor edit;
        CoordinatorLayout sheet;
        public BottomSheetBehavior bottomSheetBehavior;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            #region original
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.MainApp);
            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
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

            pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            edit = pref.Edit();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M && !pref.GetBoolean("Permission_Write", false))
            {
                bool result = Utility.CheckPermission(this, Manifest.Permission.WriteExternalStorage);
                if (result)
                {
                    edit.PutBoolean("Permission_Write", true).Apply();
                }
            }

            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.userNameHeader).Text = pref.GetString("Username", "Username");
            if (navigationView != null)
            {
                SetUpDrawerContent(navigationView);
            }

            tabs = FindViewById<TabLayout>(Resource.Id.tabs);

            viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);

            SetUpViewPager(viewPager);

            tabs.SetupWithViewPager(viewPager);

            tabs.AddOnTabSelectedListener(this);

            setupTabIcons();
            setSelectedTab();

            visibilityChanged += (sender, args) =>
            {
                if (MediaPlayerService.selectedAudio != null)
                {
                    Media audio1;
                    AudioList audio2;
                    try
                    {
                        audio2 = (AudioList)MediaPlayerService.selectedAudio;
                        mLayout.PanelHeight = toolBar.LayoutParameters.Height;
                        var txtrow1 = FindViewById<TextView>(Resource.Id.txtRow11);
                        txtrow1.Text = audio2.Title;
                        var txtrow2 = FindViewById<TextView>(Resource.Id.txtRow21);
                        txtrow2.Text = audio2.SubTitle;
                        var avatar = FindViewById<ImageView>(Resource.Id.avatar1);
                        Glide.With(this)
                            .Load(audio2.ImageUrl)
                            .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                            .Error(Resource.Drawable.ChurchLogo_Gray)
                            .DiskCacheStrategy(DiskCacheStrategy.All)
                            .Into(artworkView);
                        Glide.With(this)
                            .Load(audio2.ImageUrl)
                            .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                            .Error(Resource.Drawable.ChurchLogo_Gray)
                            .DiskCacheStrategy(DiskCacheStrategy.All)
                            .Into(avatar);
                    }
                    catch(InvalidCastException inv)
                    {
                        audio1 = (Media)MediaPlayerService.selectedAudio;
                        mLayout.PanelHeight = toolBar.LayoutParameters.Height;
                        var txtrow1 = FindViewById<TextView>(Resource.Id.txtRow11);
                        txtrow1.Text = audio1.Title;
                        var txtrow2 = FindViewById<TextView>(Resource.Id.txtRow21);
                        txtrow2.Text = audio1.Album;
                        var avatar = FindViewById<ImageView>(Resource.Id.avatar1);
                        Glide.With(this)
                            .Load(audio1.AlbumArt)
                            .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                            .Error(Resource.Drawable.ChurchLogo_Gray)
                            .DiskCacheStrategy(DiskCacheStrategy.All)
                            .Into(artworkView);
                        Glide.With(this)
                            .Load(audio1.AlbumArt)
                            .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                            .Error(Resource.Drawable.ChurchLogo_Gray)
                            .DiskCacheStrategy(DiskCacheStrategy.All)
                            .Into(avatar);
                    }
                   
                }
               
                    layout.Visibility = ViewStates.Visible;
            };

            sheet = FindViewById<CoordinatorLayout>(Resource.Id.bottom_sheet);
            bottomSheetBehavior = BottomSheetBehavior.From(sheet);

            //bottomSheetBehavior.PeekHeight = 0;
            bottomSheetBehavior.Hideable = true;
            bottomSheetBehavior.State = BottomSheetBehavior.StateHidden;
           

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

            //var mOptions = FindViewById<ImageButton>(Resource.Id.img_options);
            //mOptions.Click += (s, e) =>
            //{
            //    //Android.Support.V7.Widget.PopupMenu Popup = new Android.Support.V7.Widget.PopupMenu(mOptions.Context, mOptions);
            //    //Popup.Inflate(Resource.Menu.slideupMenu);
            //    //Popup.MenuItemClick += (o, args) =>
            //    //{
            //    //    switch (args.Item.ItemId)
            //    //    {
            //    //        case Resource.Id.action_add_hide:
            //    //            layout.Visibility = ViewStates.Gone;
            //    //            break;
            //    //    }
            //    //}; Popup.Show();
                
            //};
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
                try
                {
                    var audio = (AudioList)MediaPlayerService.selectedAudio;
                    Glide.With(this)
                   .Load(audio.ImageUrl)
                   .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                   .Error(Resource.Drawable.ChurchLogo_Gray)
                   .DiskCacheStrategy(DiskCacheStrategy.All)
                   .Into(artworkView);
                   
                    
                }
                catch(InvalidCastException inv)
                {
                    var audio = (Media)MediaPlayerService.selectedAudio;
                    Glide.With(this)
                   .Load(audio.AlbumArt)
                   .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                   .Error(Resource.Drawable.ChurchLogo_Gray)
                   .DiskCacheStrategy(DiskCacheStrategy.All)
                   .Into(artworkView);
                }
               
            }
            downloadButton = FindViewById<ImageButton>(Resource.Id.audio_download);
            downloadButton.Click += downloadButton_Click;
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

            audio_player_view = FindViewById(Resource.Id.audioPlayerView);
           
            #endregion
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            switch (requestCode)
            {
                case Utility.MY_PERMISSIONS_REQUEST_WRITE_EXTERNAL_STORAGE:

                    if (grantResults[0] == Permission.Granted)
                    {
                        edit.PutBoolean("Permission_Write", true).Apply();
                    }
                    else
                    {
                        Snackbar.Make(toolBar, "External Storage Permission Denied", Snackbar.LengthLong)
                            .Show();
                    }

                    break;
            }
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
                return string.Format("{0}:{1:00}", span.Minutes, span.Seconds);
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
                instance = mediaPlayer;
            }

            public void OnServiceConnected(ComponentName name, IBinder service)
            {
                var mediaPlayerServiceBinder = service as MediaPlayerServiceBinder;
                if (mediaPlayerServiceBinder != null)
                {
                    var binder = (MediaPlayerServiceBinder)service;
                    instance.binder = binder;
                    instance.isBound = true;
                    playPauseButton.SetImageResource(binder.GetMediaPlayerService().playImage);
                    loadingBar.Visibility = binder.GetMediaPlayerService().pbarState;
                    binder.GetMediaPlayerService().sContext = SynchronizationContext.Current;
                    binder.GetMediaPlayerService().CoverReloaded += (object sender, EventArgs e) => { instance.CoverReloaded?.Invoke(sender, e); };
                    binder.GetMediaPlayerService().StatusChanged += (object sender, EventArgs e) => { instance.StatusChanged?.Invoke(sender, e); };
                    binder.GetMediaPlayerService().Playing += (object sender, EventArgs e) => { instance.Playing?.Invoke(sender, e); };
                    binder.GetMediaPlayerService().Buffering += (object sender, EventArgs e) => { instance.Buffering?.Invoke(sender, e); };
                }
            }

            public void OnServiceDisconnected(ComponentName name)
            {
                instance.isBound = false;
            }
        }
        public void downloadButton_Click(object sender, EventArgs e)
        {
           try
            {
                var audio = (AudioList)MediaPlayerService.selectedAudio;
                if (MyService.typeQueue.Count == 0)
                {
                    MyService.typeQueue.Enqueue("audio");
                    MyService.audioQueue.Enqueue(audio);
                    var intent = new Intent(ApplicationContext, typeof(MyService));
                    intent.SetAction(MyService.StartD);
                    StartService(intent);

                }
                else
                {
                    MyService.typeQueue.Enqueue("audio");
                    MyService.audioQueue.Enqueue(audio);
                }
                
            }catch(Exception ex)
            {

            }
            

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
                    if (edit == null)
                    {
                        pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
                        edit = pref.Edit();
                    }
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

                    case Resource.Id.nav_feedback:
                        intent = new Intent(this, typeof(Feedback));
                        StartActivity(intent);
                        break;

                    case Resource.Id.nav_privacy:
                        intent = new Intent(this, typeof(Privacy));
                        StartActivity(intent);
                        break;

                    case Resource.Id.nav_terms:
                        intent = new Intent(this, typeof(Terms));
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
            adapter.AddFragment(new AudioLibrary(), string.Empty);
            adapter.AddFragment(new VideoLibrary(), string.Empty);
            adapter.AddFragment(new Favourites(), string.Empty);
            adapter.AddFragment(new Downloaded(), string.Empty);          

            viewPager.Adapter = adapter;
        }

        public override void OnBackPressed()
        {
            if (bottomSheetBehavior.State != BottomSheetBehavior.StateHidden)
            {
                bottomSheetBehavior.State = BottomSheetBehavior.StateHidden;
            }
            else if (mDrawerLayout.IsDrawerOpen(GravityCompat.Start))
            {
                mDrawerLayout.CloseDrawer(GravityCompat.Start);
            }
            else
            {
                base.OnBackPressed();
            }
        }

        public void OnTabReselected(TabLayout.Tab tab)
        {
            
        }

        public void OnTabSelected(TabLayout.Tab tab)
        {
            switch (tabs.SelectedTabPosition)
            {
                case 0:
                    break;
                case 1:
                    //SlidingUpPanelLayout.PanelState state= mLayout.GetPanelState();
                    if (mLayout.GetPanelState() != SlidingUpPanelLayout.PanelState.Hidden)
                    {
                        mLayout.SetPanelState(SlidingUpPanelLayout.PanelState.Hidden);
                    }
                    break;
                case 2:
                    break;
                case 3:
                    break;
                default:
                    break;
            }
        }

        public void OnTabUnselected(TabLayout.Tab tab)
        {
            
        }
    }
}