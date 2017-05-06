using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Media;
using DivineVerITies.ExoPlayer.Player;
using Android.Net.Wifi;
using Android.Graphics;
using System.Threading.Tasks;
using Android.Support.V7.App;
using Android.Support.V4.Media.Session;
using Android.Support.V4.Media;
using System.Collections.Generic;
using Android.Support.V4.App;
using DivineVerITies.Fragments;

namespace DivineVerITies.Helpers
{
   

    [Service]
    [IntentFilter(new[] { ActionPlay, ActionPause, ActionStop, ActionFastForward, ActionRewind, ActionTogglePlayback })]
    public class AudioService : Service, AudioManager.IOnAudioFocusChangeListener,
        MediaPlayer.IOnBufferingUpdateListener,
        MediaPlayer.IOnCompletionListener,
        MediaPlayer.IOnErrorListener,
        MediaPlayer.IOnPreparedListener,
        MediaPlayer.IOnSeekCompleteListener
    {
        public MediaPlayer player;
        private AudioManager audioManager;
        private WifiManager wifiManager;
        private WifiManager.WifiLock wifiLock;


        private MediaSessionCompat mSession;
        public MediaControllerCompat mediaControllerCompat;
        private const int NotificationId = 1;
        private ComponentName remoteComponentName;
        private RemoteControlClient remoteControlClient;

        public static AudioList selectedAudio = null;
        //Actions
        public const string ActionPlay = "com.xamarin.action.PLAY";
        public const string ActionPause = "com.xamarin.action.PAUSE";
        public const string ActionStop = "com.xamarin.action.STOP";
        public const string ActionFastForward = "com.xamarin.action.FastForward";
        public const string ActionRewind = "com.xamarin.action.Rewind";
        public const string ActionTogglePlayback = "com.xamarin.action.TOGGLEPLAYBACK";
        public static bool isPrepared = false;
        public static bool failed = false;

        public int MediaPlayerState
        {
            get
            {
                //return (mediaControllerCompat.PlaybackState != null ?
                //    mediaControllerCompat.PlaybackState.State : PlaybackStateCompat.StateNone);
                if (mediaControllerCompat != null)
                {
                    if (mediaControllerCompat != null && mediaControllerCompat.PlaybackState != null)
                        return mediaControllerCompat.PlaybackState.State;
                    else
                        return PlaybackStateCompat.StateNone;
                }
                else
                    return PlaybackStateCompat.StateNone;
            }
        }
        public event StatusChangedEventHandler StatusChanged;
        public event CoverReloadedEventHandler CoverReloaded;
        public event PlayingEventHandler Playing;
        public event BufferingEventHandler Buffering;
        private Handler PlayingHandler;
        private Java.Lang.Runnable PlayingHandlerRunnable;

        public AudioService()
        {
            // Create an instance for a runnable-handler
            PlayingHandler = new Handler();

            // Create a runnable, restarting itself if the status still is "playing"
            PlayingHandlerRunnable = new Java.Lang.Runnable(() =>
            {
                OnPlaying(EventArgs.Empty);

                if (MediaPlayerState == PlaybackStateCompat.StatePlaying)
                {
                    PlayingHandler.PostDelayed(PlayingHandlerRunnable, 250);
                }
            });

            // On Status changed to PLAYING, start raising the Playing event
            StatusChanged += (object sender, EventArgs e) =>
            {
                if (MediaPlayerState == PlaybackStateCompat.StatePlaying)
                {
                    PlayingHandler.PostDelayed(PlayingHandlerRunnable, 0);
                }
            };
        }
        protected virtual void OnStatusChanged(EventArgs e)
        {
            if (StatusChanged != null)
                StatusChanged(this, e);
        }
        protected virtual void OnCoverReloaded(EventArgs e)
        {
            if (CoverReloaded != null)
            {
                CoverReloaded(this, e);
                StartNotification();
                UpdateMediaMetadataCompat();
            }
        }
        protected virtual void OnPlaying(EventArgs e)
        {
            if (Playing != null)
                Playing(this, e);
        }
        protected virtual void OnBuffering(EventArgs e)
        {
            if (Buffering != null)
                Buffering(this, e);
        }
        /// <summary>
        /// On create simply detect audio manager and wifi manager.
        /// </summary>
        public override void OnCreate()
        {
            base.OnCreate();
            audioManager = (AudioManager)GetSystemService(AudioService);
            wifiManager = (WifiManager)GetSystemService(WifiService);
            remoteComponentName = new ComponentName(PackageName, new RemoteControlBroadcastReceiver().ComponentName);

        }
        private void InitMediaSession()
        {
            try
            {
                if (mSession == null)
                {
                    Intent nIntent = new Intent(ApplicationContext, typeof(MainActivity));
                    PendingIntent pIntent = PendingIntent.GetActivity(ApplicationContext, 0, nIntent, 0);

                    remoteComponentName = new ComponentName(PackageName, new RemoteControlBroadcastReceiver().ComponentName);

                    mSession = new MediaSessionCompat(ApplicationContext, "XamarinStreamingAudio", remoteComponentName, pIntent);
                    mediaControllerCompat = new MediaControllerCompat(ApplicationContext, mSession.SessionToken);
                }

                mSession.Active = true;
                mSession.SetCallback(new MediaSessionCallback((AudioServiceBinder)binder));

                mSession.SetFlags(MediaSessionCompat.FlagHandlesMediaButtons | MediaSessionCompat.FlagHandlesTransportControls);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        [Obsolete("deprecated")]
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            HandleIntent(intent);
            return base.OnStartCommand(intent, flags, startId);
        }
        private void HandleIntent(Intent intent)
        {
            if (intent == null || intent.Action == null)
                return;

            String action = intent.Action;

            if (action.Equals(ActionPlay))
            {
                mediaControllerCompat.GetTransportControls().Play();
            }
            else if (action.Equals(ActionPause))
            {
                mediaControllerCompat.GetTransportControls().Pause();
            }
            else if (action.Equals(ActionFastForward))
            {
                mediaControllerCompat.GetTransportControls().FastForward();
            }
            else if (action.Equals(ActionRewind))
            {
                mediaControllerCompat.GetTransportControls().Rewind();
            }
            else if (action.Equals(ActionStop))
            {
                mediaControllerCompat.GetTransportControls().Stop();
            }
        }
        /// <summary>
        /// Dont do anythin on bind.
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public override IBinder OnBind(Intent intent)
        {
            binder = new AudioServiceBinder(this);
            return binder;
        }
        public override bool OnUnbind(Intent intent)
        {
            StopNotification();
            return base.OnUnbind(intent);
        }
        /// <summary>
        /// Instantiate player and set necessary events
        /// </summary>
        private void InitializePlayer()
        {
            player = new MediaPlayer();
            //streaming player
            player.SetAudioStreamType(Stream.Music);
            //keep player running under lock screen
            player.SetWakeMode(ApplicationContext, WakeLockFlags.Partial);
            player.SetOnBufferingUpdateListener(this);
            player.SetOnCompletionListener(this);
            player.SetOnErrorListener(this);
            player.SetOnPreparedListener(this);

        }
        public void OnBufferingUpdate(MediaPlayer mp, int percent)
        {
            int duration = 0;
            if (MediaPlayerState == PlaybackStateCompat.StatePlaying || MediaPlayerState == PlaybackStateCompat.StatePaused)
                duration = mp.Duration;

            int newBufferedTime = duration * percent / 100;
            if (newBufferedTime != Buffered)
            {
                Buffered = newBufferedTime;
            }
        }
        public async void OnCompletion(MediaPlayer mp)
        {
            //await PlayNext();
            Stop();
            Fragment3.chosenView.Visibility = ViewStates.Visible;
        }
        public bool OnError(MediaPlayer mp, MediaError what, int extra)
        {

            UpdatePlaybackState(PlaybackStateCompat.StateError);
            Stop();
            return true;
        }
        public void OnSeekComplete(MediaPlayer mp)
        {
            //TODO: Implement buffering on seeking
        }
        public async void OnPrepared(MediaPlayer mp)
        {
            isPrepared = true;
            while (cover == null && !failed)
            {
                await Task.Delay(2000);
            }

            Audio_Player.loadingBar.Visibility = ViewStates.Gone;
            //Mediaplayer is prepared start track playback
            mp.Start();
            UpdatePlaybackState(PlaybackStateCompat.StatePlaying);
        }
        public int Position
        {
            get
            {
                if (player == null
                    || (MediaPlayerState != PlaybackStateCompat.StatePlaying
                        && MediaPlayerState != PlaybackStateCompat.StatePaused))
                    return -1;
                else
                    return player.CurrentPosition;
            }
        }
        public int Duration
        {
            get
            {
                if (player == null
                    || (MediaPlayerState != PlaybackStateCompat.StatePlaying
                        && MediaPlayerState != PlaybackStateCompat.StatePaused))
                    return 0;
                else
                    return player.Duration;
            }
        }
        private int buffered = 0;
        public int Buffered
        {
            get
            {
                if (player == null)
                    return 0;
                else
                    return buffered;
            }
            private set
            {
                buffered = value;
                OnBuffering(EventArgs.Empty);
            }
        }
        public static Bitmap cover;
        //public object Cover
        //{
        //    get
        //    {
        //        if (cover == null)
        //            cover = BitmapFactory.DecodeResource(Resources, Resource.Drawable.Logo_trans192);
        //        return cover;
        //    }
        //    private set
        //    {
        //        cover = value as Bitmap;
        //        OnCoverReloaded(EventArgs.Empty);
        //    }
        //}
        public async Task Play()
        {
            if ((MediaPlayerState == PlaybackStateCompat.StatePaused) && (player != null))
            {

                player.Start();
                UpdatePlaybackState(PlaybackStateCompat.StatePlaying);
                StartNotification();
                Audio_Player.loadingBar.Visibility = ViewStates.Gone;
                //Update remote client now that we are playing                
                UpdateMediaMetadataCompat();
                return;
            }
            if (player == null)
            {
                InitializePlayer();
            }
            if (mSession == null) { InitMediaSession(); }
            if (player.IsPlaying) { UpdatePlaybackState(PlaybackStateCompat.StatePlaying); return; }
            try
            {
                //MediaMetadataRetriever metaRetriever = new MediaMetadataRetriever();
                //await player.SetDataSourceAsync(ApplicationContext,
                //    Android.Net.Uri.Parse(Audio_Player.selectedAudio.Link));
                await setDataSourceAsync();
                //await metaRetriever.SetDataSourceAsync(Audio_Player.selectedAudio.Link, new Dictionary<string, string>());
                var focusResult = audioManager.RequestAudioFocus(this, Stream.Music,
                    AudioFocus.Gain);
                if (focusResult != AudioFocusRequest.Granted)
                {
                    Console.WriteLine("Could not get audio focus");
                }
                UpdatePlaybackState(PlaybackStateCompat.StateBuffering);
                player.PrepareAsync();
                AcquireWifiLock();

                UpdateMediaMetadataCompat();
                //byte[] imageByteArray = metaRetriever.GetEmbeddedPicture();
                //if (imageByteArray == null)
                //    Cover = await BitmapFactory.DecodeResourceAsync(Resources, Resource.Drawable.Logo_trans192);
                //else
                //    Cover = await BitmapFactory.DecodeByteArrayAsync(imageByteArray, 0, imageByteArray.Length);

            }
            catch (Exception ex)
            {
                UpdatePlaybackState(PlaybackStateCompat.StateStopped);

                player.Reset();
                player.Release();
                player = null;
                Console.WriteLine("Unable to start playback: " + ex);
            }
        }

        private async Task setDataSourceAsync()
        {
            //MediaMetadataRetriever metaretriever
            player.SetDataSource(ApplicationContext,Android.Net.Uri.Parse(selectedAudio.Link));
            //metaretriever.SetDataSource(Audio_Player.selectedAudio.Link, new Dictionary<string, string>());
        }

        public async Task Seek(int position)
        {
            await Task.Run(() =>
            {
                if (player != null)
                {
                    player.SeekTo(position);
                }
            });
        }
        private void AcquireWifiLock()
        {
            if (wifiLock == null)
            {
                wifiLock = wifiManager.CreateWifiLock(Android.Net.WifiMode.Full,
                    "xamarin_wifi_lock");
            }
            wifiLock.Acquire();
        }
        private void ReleaseWifiLock()
        {
            if (wifiLock == null)
            {
                return;
            }
            wifiLock.Release();
            wifiLock = null;
        }
        private void StartNotification()
        {
            if (mSession == null)
                return;

            var pendingIntent = PendingIntent.GetActivity(ApplicationContext, 0, new Intent(ApplicationContext, typeof(Audio_Player)), PendingIntentFlags.UpdateCurrent);
            MediaMetadataCompat currentTrack = mediaControllerCompat.Metadata;

            Android.Support.V7.App.NotificationCompat.MediaStyle style = new Android.Support.V7.App.NotificationCompat.MediaStyle();
            style.SetMediaSession(mSession.SessionToken);

            Intent intent = new Intent(ApplicationContext, typeof(AudioService));
            intent.SetAction(ActionStop);
            PendingIntent pendingCancelIntent = PendingIntent.GetService(ApplicationContext, 1, intent, PendingIntentFlags.CancelCurrent);

            style.SetShowCancelButton(true);
            style.SetCancelButtonIntent(pendingCancelIntent);

            Android.Support.V4.App.NotificationCompat.Builder builder = new Android.Support.V4.App.NotificationCompat.Builder(ApplicationContext);
            builder.SetStyle(style);
            builder.SetContentTitle(currentTrack.GetString(MediaMetadata.MetadataKeyTitle));
            builder.SetContentText(currentTrack.GetString(MediaMetadata.MetadataKeyArtist));
            builder.SetContentInfo(currentTrack.GetString(MediaMetadata.MetadataKeyAlbum));
            builder.SetSmallIcon(Resource.Drawable.Logo_trans72);
            builder.SetLargeIcon(cover as Bitmap);
            builder.SetContentIntent(pendingIntent);
            builder.SetShowWhen(false);
            builder.SetOngoing(MediaPlayerState == PlaybackStateCompat.StatePlaying);
            builder.SetVisibility(Android.Support.V4.App.NotificationCompat.VisibilityPublic);

            builder.AddAction(GenerateActionCompat(Android.Resource.Drawable.IcMediaRew, "", ActionRewind));
            AddPlayPauseActionCompat(builder);
            builder.AddAction(GenerateActionCompat(Android.Resource.Drawable.IcMediaFf, "", ActionFastForward));
            style.SetShowActionsInCompactView(0, 1, 2);

            NotificationManagerCompat.From(ApplicationContext).Notify(NotificationId, builder.Build());
        }
        private Android.Support.V4.App.NotificationCompat.Action GenerateActionCompat(int icon, String title, String intentAction)
        {
            Intent intent = new Intent(ApplicationContext, typeof(AudioService));
            intent.SetAction(intentAction);

            PendingIntentFlags flags = PendingIntentFlags.UpdateCurrent;
            if (intentAction.Equals(ActionStop))
                flags = PendingIntentFlags.CancelCurrent;

            PendingIntent pendingIntent = PendingIntent.GetService(ApplicationContext, 1, intent, flags);

            return new Android.Support.V4.App.NotificationCompat.Action.Builder(icon, title, pendingIntent).Build();
        }
        private void AddPlayPauseActionCompat(Android.Support.V4.App.NotificationCompat.Builder builder)
        {
            if (MediaPlayerState == PlaybackStateCompat.StatePlaying)
            {
                builder.AddAction(GenerateActionCompat(Android.Resource.Drawable.IcMediaPause, "", ActionPause));
                var image = GetDrawable(Android.Resource.Drawable.IcMediaPause);
                Audio_Player.playPauseButton.SetImageDrawable(image);
                Audio_Player.playPauseButton.SetBackgroundColor(Color.Transparent);

            }
            else
            {
                builder.AddAction(GenerateActionCompat(Android.Resource.Drawable.IcMediaPlay, "", ActionPlay));
                var image = GetDrawable(Resource.Drawable.ic_play_arrow);
                Audio_Player.playPauseButton.SetImageDrawable(image);
                Audio_Player.playPauseButton.SetBackgroundColor(Color.Transparent);
            }
        }
        public void StopNotification()
        {
            NotificationManagerCompat nm = NotificationManagerCompat.From(ApplicationContext);
            nm.CancelAll();
        }
        private void UpdateMediaMetadataCompat(MediaMetadataRetriever metaRetriever = null)
        {
            if (mSession == null)
                return;

            MediaMetadataCompat.Builder builder = new MediaMetadataCompat.Builder();

            if (metaRetriever != null)
            {
                builder
                .PutString(MediaMetadata.MetadataKeyAlbum, metaRetriever.ExtractMetadata(MetadataKey.Album))
                .PutString(MediaMetadata.MetadataKeyArtist, metaRetriever.ExtractMetadata(MetadataKey.Artist))
                .PutString(MediaMetadata.MetadataKeyTitle, metaRetriever.ExtractMetadata(MetadataKey.Title));
            }
            else
            {
                builder
                    .PutString(MediaMetadata.MetadataKeyAlbum, mSession.Controller.Metadata.GetString(MediaMetadata.MetadataKeyAlbum))
                    .PutString(MediaMetadata.MetadataKeyArtist, mSession.Controller.Metadata.GetString(MediaMetadata.MetadataKeyArtist))
                    .PutString(MediaMetadata.MetadataKeyTitle, mSession.Controller.Metadata.GetString(MediaMetadata.MetadataKeyTitle));
            }
            builder.PutBitmap(MediaMetadata.MetadataKeyAlbumArt, cover as Bitmap);

            mSession.SetMetadata(builder.Build());
        }
        private void UnregisterMediaSessionCompat()
        {
            try
            {
                if (mSession != null)
                {
                    mSession.Dispose();
                    mSession = null;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        IBinder binder;

        public async Task Pause()
        {
            await Task.Run(() =>
            {
                if (player == null)
                    return;
                if (player.IsPlaying)
                    player.Pause();

                UpdatePlaybackState(PlaybackStateCompat.StatePaused);
            });
        }
        public async Task Stop()
        {
            await Task.Run(() =>
            {
                if (player == null)
                    return;
                if (player.IsPlaying)
                {
                    player.Stop();
                    if (remoteControlClient != null)
                        remoteControlClient.SetPlaybackState(RemoteControlPlayState.Stopped);
                }

                UpdatePlaybackState(PlaybackStateCompat.StateStopped);
                player.Reset();
                StopNotification();
                Audio_Player.loadingBar.Visibility = ViewStates.Gone;
                var image = GetDrawable(Resource.Drawable.ic_play_arrow);
                Audio_Player.playPauseButton.SetImageDrawable(image);
                StopForeground(true);
                ReleaseWifiLock();
                UnregisterMediaSessionCompat();
            });
        }
        private void UpdatePlaybackState(int state)
        {
            if (mSession == null || player == null)
                return;

            try
            {
                PlaybackStateCompat.Builder stateBuilder = new PlaybackStateCompat.Builder()
                    .SetActions(
                        PlaybackStateCompat.ActionPause |
                        PlaybackStateCompat.ActionPlay |
                        PlaybackStateCompat.ActionPlayPause |
                        PlaybackStateCompat.ActionFastForward |
                        PlaybackStateCompat.ActionRewind |
                        PlaybackStateCompat.ActionStop
                    )
                    .SetState(state, Position, 1.0f, SystemClock.ElapsedRealtime());

                mSession.SetPlaybackState(stateBuilder.Build());

                //Used for backwards compatibility
                if (Build.VERSION.SdkInt < BuildVersionCodes.Lollipop)
                {
                    if (mSession.RemoteControlClient != null && mSession.RemoteControlClient.Equals(typeof(RemoteControlClient)))
                    {
                        RemoteControlClient remoteControlClient = (RemoteControlClient)mSession.RemoteControlClient;

                        RemoteControlFlags flags = RemoteControlFlags.Play
                            | RemoteControlFlags.Pause
                            | RemoteControlFlags.PlayPause
                            | RemoteControlFlags.FastForward
                            | RemoteControlFlags.Rewind
                            | RemoteControlFlags.Stop;

                        remoteControlClient.SetTransportControlFlags(flags);
                    }
                }

                OnStatusChanged(EventArgs.Empty);

                if (state == PlaybackStateCompat.StatePlaying || state == PlaybackStateCompat.StatePaused)
                {
                    StartNotification();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        public async Task Rewind()
        {
            if (player != null)
            {
                player.Reset();
                player.Release();
                player = null;
            }

            UpdatePlaybackState(PlaybackStateCompat.StateRewinding);

            await Seek(player.CurrentPosition + 5);
        }
        public async Task FastForward()
        {
            if (player != null)
            {
                player.Reset();
                player.Release();
                player = null;
            }

            UpdatePlaybackState(PlaybackStateCompat.StateFastForwarding);

            await Seek(player.CurrentPosition + 5);
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (player != null)
            {
                player.Release();
                player = null;

                StopNotification();
                StopForeground(true);
                ReleaseWifiLock();
                UnregisterMediaSessionCompat();
            }
        }
        /// <summary>
        /// For a good user experience we should account for when audio focus has changed.
        /// There is only 1 audio output there may be several media services trying to use it so
        /// we should act correctly based on this.  "duck" to be quiet and when we gain go full.
        /// All applications are encouraged to follow this, but are not enforced.
        /// </summary>
        /// <param name="focusChange"></param>
        public void OnAudioFocusChange([GeneratedEnum] AudioFocus focusChange)
        {
            switch (focusChange)
            {
                case AudioFocus.Gain:
                    if (player == null)
                        InitializePlayer();

                    if (!player.IsPlaying)
                    {
                        player.Start();

                    }

                    player.SetVolume(1.0f, 1.0f);//Turn it up!
                    break;
                case AudioFocus.Loss:
                    //We have lost focus stop!
                    Stop();
                    break;
                case AudioFocus.LossTransient:
                    //We have lost focus for a short time, but likely to resume so pause
                    Pause();
                    break;
                case AudioFocus.LossTransientCanDuck:
                    //We have lost focus but should till play at a muted 10% volume
                    if (player.IsPlaying)
                        player.SetVolume(.1f, .1f);//turn it down!
                    break;

            }
        }
        public class MediaSessionCallback : MediaSessionCompat.Callback
        {

            private AudioServiceBinder mediaPlayerService;
            public MediaSessionCallback(AudioServiceBinder service)
            {
                mediaPlayerService = service;
                
            }
            public override void OnPause()
            {
                mediaPlayerService.GetMediaPlayerService().Pause();
                base.OnPause();
            }

            public override void OnPlay()
            {
                mediaPlayerService.GetMediaPlayerService().Play();
                base.OnPlay();
            }

            public override void OnFastForward()
            {
                mediaPlayerService.GetMediaPlayerService().FastForward();
                base.OnFastForward();
            }

            public override void OnRewind()
            {
                mediaPlayerService.GetMediaPlayerService().Rewind();
                base.OnRewind();
            }

            public override void OnStop()
            {
                mediaPlayerService.GetMediaPlayerService().Stop();
                base.OnStop();
            }

        }
    }
    public class AudioServiceBinder : Binder
    {
        private AudioService service;

        public AudioServiceBinder(AudioService service)
        {
            this.service = service;
        }

        public AudioService GetMediaPlayerService()
        {
            return service;
        }
    }

    [BroadcastReceiver]
    [Android.App.IntentFilter(new[] { Intent.ActionMediaButton })]
    public class RemoteControlBroadcastReceiver : BroadcastReceiver
    {
        public string ComponentName { get { return this.Class.Name; } }
        public override void OnReceive(Context context, Intent intent)
        {
            if (intent.Action != Intent.ActionMediaButton)
                return;
            var key = (KeyEvent)intent.GetParcelableExtra(Intent.ExtraKeyEvent);
            if (key.Action != KeyEventActions.Down)
                return;
            var action = AudioService.ActionPlay;
            switch (key.KeyCode)
            {
                case Keycode.Headsethook:
                case Keycode.MediaPlay: action = AudioService.ActionPlay; break;
                case Keycode.MediaPause: action = AudioService.ActionPause; break;
                case Keycode.MediaStop: action = AudioService.ActionStop; break;
                case Keycode.MediaFastForward: action = AudioService.ActionFastForward; break;
                case Keycode.MediaRewind: action = AudioService.ActionRewind; break;
                default: return;
            }
            var remoteIntent = new Intent(context, typeof(DivineVerITies.Helpers.AudioService));
            intent.SetAction(action);
            context.StartService(intent);

        }
    }
}