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

namespace DivineVerITies.Helpers
{
    [Service]
    [IntentFilter(new[] { ActionPlay, ActionPause, ActionStop, ActionFastForward, ActionRewind })]
    public class AudioService : Service, AudioManager.IOnAudioFocusChangeListener
    {
        private MediaPlayer player;
        private AudioManager audioManager;
        private WifiManager wifiManager;
        private WifiManager.WifiLock wifiLock;
        private bool paused;
        private Android.Support.V4.Media.Session.MediaSessionCompat mSession;        
        private const int NotificationId = 1;
        private ComponentName remoteComponentName;
        private RemoteControlClient remoteControlClient;
        public static Bitmap image = null;
        public static AudioList selectedAudio = null;
        //Actions
        public const string ActionPlay = "com.xamarin.action.PLAY";
        public const string ActionPause = "com.xamarin.action.PAUSE";
        public const string ActionStop = "com.xamarin.action.STOP";
        public const string ActionFastForward = "com.xamarin.action.FastForward";
        public const string ActionRewind = "com.xamarin.action.Rewind";
        public static bool isPrepared = false;
        public static bool failed = false;


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
        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            switch (intent.Action)
            {
                case ActionPlay: Play(); break;
                case ActionStop: Stop(); break;
                case ActionPause: Pause(); break;
                case ActionFastForward: FastForward(); break;
                case ActionRewind: Rewind(); break;
            }
            //set stick because of long running operation
            return StartCommandResult.Sticky;
        }        
        /// <summary>
        /// Dont do anythin on bind.
        /// </summary>
        /// <param name="intent"></param>
        /// <returns></returns>
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        /// <summary>
        /// Instantiate player and set necessary events
        /// </summary>
        private void InitializePlayer()
        {
            player = new MediaPlayer();            
            mSession =  new Android.Support.V4.Media.Session.MediaSessionCompat(ApplicationContext, "Controls");
            
            
            //streaming player
            player.SetAudioStreamType(Stream.Music);
            //keep player running under lock screen
            player.SetWakeMode(ApplicationContext, WakeLockFlags.Partial);
            player.Prepared += async(sender, args) =>
            {
                isPrepared = true;
                while (image == null && !failed)
                {
                    await Task.Delay(2000);
                }
                Audio_Player.loadingBar.Visibility = ViewStates.Gone;
                if (remoteControlClient != null)
                    remoteControlClient.SetPlaybackState(RemoteControlPlayState.Playing);
                UpdateMetadata();
                player.Start();
                StartForeground();

            };
            player.Completion += (sender, args) =>
            {
                Stop();
            };
            player.Error += (sender, args) =>
             {
                 if (remoteControlClient != null)
                     remoteControlClient.SetPlaybackState(RemoteControlPlayState.Error);
                 Console.WriteLine("Error in playback resetting: " + args.What);
                 Stop();
             };
        }
        private async void Play()
        {
            if (paused && player != null)
            {
                paused = false;
                player.Start();
                StartForeground();
                Audio_Player.loadingBar.Visibility = ViewStates.Gone;
                //Update remote client now that we are playing
                RegisterRemoteClient();
                remoteControlClient.SetPlaybackState(RemoteControlPlayState.Playing);
                UpdateMetadata();
                return;
            }
            if (player == null)
            {
                InitializePlayer();
            }
            if (player.IsPlaying)
                return;
            try
            {
                await player.SetDataSourceAsync(ApplicationContext,
                    Android.Net.Uri.Parse(Audio_Player.selectedAudio.Link));
                var focusResult = audioManager.RequestAudioFocus(this, Stream.Music,
                    AudioFocus.Gain);
                if (focusResult != AudioFocusRequest.Granted)
                {
                    Console.WriteLine("Could not get audio focus");
                }

                player.PrepareAsync();
                AcquireWifiLock();
                RegisterRemoteClient();
                remoteControlClient.SetPlaybackState(RemoteControlPlayState.Buffering);
                UpdateMetadata();

            }
            catch (Exception ex)
            {
                Console.WriteLine("Unable to start playback: " + ex);
            }
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
        ///<summary>
        /// Presents notification to user
        /// Pressing notification sends back to main activity
        /// </summary>
        private void StartForeground()
        {            
            var intentContent = new Intent(ApplicationContext, typeof(Audio_Player));
            Audio_Player.stackBuilder.AddNextIntent(intentContent);
            var intentPlay = new Intent(ApplicationContext, typeof(DivineVerITies.Helpers.AudioService));
            intentPlay.SetAction(DivineVerITies.Helpers.AudioService.ActionPlay);
            PendingIntent playPendingIntent = PendingIntent.GetService(this, 0, intentPlay, PendingIntentFlags.UpdateCurrent);
            var intentPause = new Intent(ApplicationContext, typeof(DivineVerITies.Helpers.AudioService)).SetAction(DivineVerITies.Helpers.AudioService.ActionPause);
            PendingIntent pausePendingIntent = PendingIntent.GetService(this, 0, intentPause, PendingIntentFlags.UpdateCurrent);
            var intentStop = new Intent(ApplicationContext, typeof(DivineVerITies.Helpers.AudioService)).SetAction(DivineVerITies.Helpers.AudioService.ActionStop);            
            PendingIntent stopPendingIntent = PendingIntent.GetService(this, 0, intentStop, PendingIntentFlags.UpdateCurrent);
            var intentff = new Intent(ApplicationContext, typeof(DivineVerITies.Helpers.AudioService)).SetAction(DivineVerITies.Helpers.AudioService.ActionFastForward);
            PendingIntent pIntentff = PendingIntent.GetService(this, 0, intentStop, PendingIntentFlags.UpdateCurrent);
            var intentrw = new Intent(ApplicationContext, typeof(DivineVerITies.Helpers.AudioService)).SetAction(DivineVerITies.Helpers.AudioService.ActionRewind);
            PendingIntent pIntentrw = PendingIntent.GetService(this, 0, intentStop, PendingIntentFlags.UpdateCurrent);
            intentContent.AddFlags(ActivityFlags.ClearTop);
            PendingIntent pIntent = Audio_Player.stackBuilder.GetPendingIntent(0, PendingIntentFlags.UpdateCurrent);
            
            
            // Instantiate the notification builder and set notification elements:
            NotificationCompat.Builder builder = new NotificationCompat.Builder(this);
            builder.SetSmallIcon(Resource.Drawable.Logo_trans192);
            builder.SetVisibility(3);
            builder.AddAction(Resource.Drawable.ic_fast_rewind_white, "Rewind", pIntentrw);//#0
            if (player.IsPlaying)
            {
                builder.AddAction(Resource.Drawable.ic_done, "Pause", pausePendingIntent);// #1
            }
            else
            {
                builder.AddAction(Resource.Drawable.ic_play_arrow, "Play", playPendingIntent);// #1
            } 
            builder.AddAction(Resource.Drawable.ic_stop, "Stop", stopPendingIntent);//#2
            builder.AddAction(Resource.Drawable.ic_fast_forward_white, "Rewind", pIntentff);//#3
            builder.SetTicker(new Java.Lang.String(selectedAudio.Title));
            builder.SetOngoing(true);
            builder.SetContentTitle("Podcast Streaming...");
            builder.SetStyle(new Android.Support.V7.App.NotificationCompat.MediaStyle().SetShowActionsInCompactView(0, 1, 2)
                .SetMediaSession(mSession.SessionToken));
            builder.SetLargeIcon(image);
            builder.SetContentIntent(pIntent);           
            Notification notification = builder.Build();
            notification.Flags |= NotificationFlags.OngoingEvent; 
            StartForeground(NotificationId, notification);                       
        }
        private void Pause()
        {
            if (player == null)
                return;
            if (player.IsPlaying)
                player.Pause();
            //StopForeground(true);
            paused = true;
            StartForeground();
            remoteControlClient.SetPlaybackState(RemoteControlPlayState.Paused);
        }
        private void Stop()
        {
            if (player == null)
                return;
            if (player.IsPlaying)
            {
                player.Stop();
                if (remoteControlClient != null)
                    remoteControlClient.SetPlaybackState(RemoteControlPlayState.Stopped);
            }
                

            player.Reset();
            paused = false;
            StopForeground(true);
            ReleaseWifiLock();
        }
        private void Rewind()
        {

        }
        private void FastForward()
        {

        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            if (player != null)
            {
                player.Release();
                player = null;

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
                        paused = false;
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
        private void RegisterRemoteClient()
        {
            remoteComponentName = new ComponentName(PackageName, new RemoteControlBroadcastReceiver().ComponentName);
            try
            {
                if (remoteControlClient == null)
                {
                    audioManager.RegisterMediaButtonEventReceiver(remoteComponentName);
                    //Create a new pending intent that we want triggered by remote control client
                    var mediaButtonIntent = new Intent(Intent.ActionMediaButton);
                    mediaButtonIntent.SetComponent(remoteComponentName);
                    // Create new pending intent for the intent
                    var mediaPendingIntent = PendingIntent.GetBroadcast(this, 0, mediaButtonIntent, 0);
                    // Create and register the remote control client
                    remoteControlClient = new RemoteControlClient(mediaPendingIntent);
                    audioManager.RegisterRemoteControlClient(remoteControlClient);
                }
                //add transport control flags we can to handle
                remoteControlClient.SetTransportControlFlags(RemoteControlFlags.Play |
                                         RemoteControlFlags.Pause |                                         
                                         RemoteControlFlags.Stop |
                                         RemoteControlFlags.FastForward |
                                         RemoteControlFlags.Rewind);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private void UnregisterRemoteClient()
        {
            try
            {
                audioManager.UnregisterMediaButtonEventReceiver(remoteComponentName);
                audioManager.UnregisterRemoteControlClient(remoteControlClient);
                remoteControlClient.Dispose();
                remoteControlClient = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
        private void UpdateMetadata()
        {
            if (remoteControlClient == null)
                return;

            var metadataEditor = remoteControlClient.EditMetadata(true);
            metadataEditor.PutString(MetadataKey.Album, selectedAudio.SubTitle);
            metadataEditor.PutString(MetadataKey.Artist, "Pastor");
            metadataEditor.PutString(MetadataKey.Albumartist, "Pastor");
            metadataEditor.PutString(MetadataKey.Title, selectedAudio.Title );
            var coverArt = image;
            metadataEditor.PutBitmap(BitmapKey.Artwork, coverArt);
            metadataEditor.Apply();
        }

    }

    [BroadcastReceiver]
    [Android.App.IntentFilter(new[] { Intent.ActionMediaButton})]
    public class RemoteControlBroadcastReceiver:BroadcastReceiver
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