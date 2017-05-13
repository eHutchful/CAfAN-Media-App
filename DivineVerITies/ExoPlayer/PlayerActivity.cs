using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Text;
using Android.Views;
using Android.Views.Accessibility;
using Android.Widget;
using Com.Google.Android.Exoplayer;
using Com.Google.Android.Exoplayer.Audio;
using Com.Google.Android.Exoplayer.Drm;
using Com.Google.Android.Exoplayer.Text;
using Com.Google.Android.Exoplayer.Util;
using DivineVerITies.ExoPlayer.Player;
using Java.Interop;
using Java.Lang;
using Java.Net;
using Java.Util;
using System;
using System.Collections.Generic;
using Exception = Java.Lang.Exception;
using Exoplayer1 = Com.Google.Android.Exoplayer.ExoPlayer;
using String = Java.Lang.String;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace DivineVerITies.ExoPlayer
{
    /// <summary>
    /// An activity that plays media using <see cref="VideoPlayer"/>.
    /// </summary>
    [Activity(
       // Name = "DivineVerITies.ExoPlayer.PlayerActivity",
		ConfigurationChanges = ConfigChanges.KeyboardHidden | ConfigChanges.Keyboard | ConfigChanges.Orientation | ConfigChanges.ScreenSize,
		LaunchMode = LaunchMode.SingleInstance,
        Label = "@string/ApplicationName",
        Theme = "@style/Theme.DesignDemo"
		)]
    [IntentFilter(new[] { "divineVerITies.ExoPlayer.action.VIEW" }, Categories = new[] { "android.intent.category.DEFAULT" }, DataScheme = "http")]
	class PlayerActivity : AppCompatActivity, ISurfaceHolderCallback, View.IOnClickListener,
		VideoPlayer.IListener, VideoPlayer.ICaptionListener, VideoPlayer.ID3MetadataListener,
		AudioCapabilitiesReceiver.IListener
	{
        public static object selectedVideo;
        public ProgressBar mProgressBar;
		// For use within demo app code.
		public const string ContentIdExtra = "content_id";
		public const string ContentTypeExtra = "content_type";
		public const int TypeDash = 0;
		public const int TypeSs = 1;
		public const int TypeHls = 2;
		public const int TypeOther = 3;

		// For use when launching the demo app using adb.
		public const string ContentExtExtra = "type";
		private const string ExtDash = ".mpd";
		private const string ExtSs = ".ism";
		private const string ExtHls = ".m3u8";

		private const int MenuGroupTracks = 1;
		private const int IdOffset = 2;

		public static readonly CookieManager DefaultCookieManager;

		static PlayerActivity()
		{
			DefaultCookieManager = new CookieManager();
			DefaultCookieManager.SetCookiePolicy(CookiePolicy.AcceptOriginalServer);
		}

        private SupportToolbar toolBar;
		private EventLogger _eventLogger;
		private MediaController _mediaController;
		private View _debugRootView;
		private View _shutterView;
		private AspectRatioFrameLayout _videoFrame;
		private SurfaceView _surfaceView;
		//private TextView _debugTextView;
		//public TextView _playerStateTextView;
        private string _playerState;
		private SubtitleLayout _subtitleLayout;
		private Button _videoButton;
		private Button _audioButton;
		private Button _textButton;
		private Button _retryButton;

		public VideoPlayer _player;
		private DebugTextViewHelper _debugViewHelper;
		private bool _playerNeedsPrepare;

		public long _playerPosition;
		public bool _enableBackgroundAudio;

        public Uri _contentUri;
        public int _contentType;
		public string _contentId;

		public AudioCapabilitiesReceiver _audioCapabilitiesReceiver;

		#region Activity lifecycle

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.player_activity);

            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            
            try
            {
                var video = (Video)selectedVideo;
                SupportActionBar.Title = video.Title;
            }
            catch(InvalidCastException inv)
            {
                var video = (DivineVerITies.Helpers.Media)selectedVideo;
                SupportActionBar.Title = video.Title;
            }
            
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

			var root = FindViewById(Resource.Id.root);

			root.Touch += (sender, args) =>
			{
				var motionEvent = args.Event;
				switch (motionEvent.Action)
				{
					case MotionEventActions.Down:
						ToggleControlsVisibility();
						break;
					case MotionEventActions.Up:
						((View) sender).PerformClick();
						break;
				}
				args.Handled = true;
			};
			root.KeyPress += (sender, args) =>
			{
				var keyCode = args.KeyCode;
				if (keyCode == Keycode.Back || keyCode == Keycode.Escape
				    || keyCode == Keycode.Menu)
				{
					args.Handled = false;
				}
				else
				{
					_mediaController.DispatchKeyEvent(args.Event);
				}
			};

			_shutterView = FindViewById(Resource.Id.shutter);
			_debugRootView = FindViewById(Resource.Id.controls_root);

			_videoFrame = FindViewById<AspectRatioFrameLayout>(Resource.Id.video_frame);
			_surfaceView = FindViewById<SurfaceView>(Resource.Id.surface_view);
			_surfaceView.Holder.AddCallback(this);
			//_debugTextView = FindViewById<TextView>(Resource.Id.debug_text_view);

			//_playerStateTextView = FindViewById<TextView>(Resource.Id.player_state_view);
			_subtitleLayout = FindViewById<SubtitleLayout>(Resource.Id.subtitles);

			_mediaController = new MediaController(this);
			_mediaController.SetAnchorView(root);
			_retryButton = FindViewById<Button>(Resource.Id.retry_button);
			_retryButton.SetOnClickListener(this);
			_videoButton = FindViewById<Button>(Resource.Id.video_controls);
			_audioButton = FindViewById<Button>(Resource.Id.audio_controls);
			_textButton = FindViewById<Button>(Resource.Id.text_controls);

			var currentHandler = CookieHandler.Default;
			if (currentHandler != DefaultCookieManager)
			{
				CookieHandler.Default = DefaultCookieManager;
			}

			_audioCapabilitiesReceiver = new AudioCapabilitiesReceiver(this, this);
			_audioCapabilitiesReceiver.Register();

            mProgressBar = FindViewById<ProgressBar>(Resource.Id.video_player_loading);
		}

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.video_list, menu);
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

		protected override void OnNewIntent(Intent intent)
		{
			ReleasePlayer();
			_playerPosition = 0;
			Intent = intent;
		}

		protected override void OnResume()
		{
			base.OnResume();
			var intent = Intent;
			_contentUri = intent.Data;
			_contentType = intent.GetIntExtra(ContentTypeExtra,
				InferContentType(_contentUri, intent.GetStringExtra(ContentExtExtra)));
			_contentId = intent.GetStringExtra(ContentIdExtra);
			ConfigureSubtitleView();
			if (_player == null)
			{
				PreparePlayer(true);
			}
			else
			{
				_player.Backgrounded = false;
			}
		}

		protected override void OnPause()
		{
			base.OnPause();
			if (!_enableBackgroundAudio)
			{
				ReleasePlayer();
			}
			else
			{
				_player.Backgrounded = true;
			}
			_shutterView.Visibility = ViewStates.Visible;
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			_audioCapabilitiesReceiver.Unregister();
			ReleasePlayer();
		}

		#endregion

		#region OnClickListener methods

		public void OnClick(View view)
		{
			if (view == _retryButton)
			{
				PreparePlayer(true);
			}
		}

		#endregion

		#region AudioCapabilitiesReceiver.Listener methods

		public void OnAudioCapabilitiesChanged(AudioCapabilities audioCapabilities)
		{
			if (_player == null)
			{
				return;
			}
			var backgrounded = _player.Backgrounded;
			var playWhenReady = _player.PlayWhenReady;
			ReleasePlayer();
			PreparePlayer(playWhenReady);
			_player.Backgrounded = backgrounded;
		}

		#endregion

		#region Internal methods

		private VideoPlayer.IRendererBuilder GetRendererBuilder()
		{
			var userAgent = ExoPlayerUtil.GetUserAgent(this, "ExoPlayerDemo");
			switch (_contentType)
			{
                case TypeSs:
                    return new SmoothStreamingRendererBuilder(this, userAgent, _contentUri.ToString(),
                        new SmoothStreamingTestMediaDrmCallback());
                case TypeDash:
                    return new DashRendererBuilder(this, userAgent, _contentUri.ToString(),
                        new WidevineTestMediaDrmCallback(_contentId));
				case TypeHls:
					return new HlsRendererBuilder(this, userAgent, _contentUri.ToString());
				case TypeOther:
					return new ExtractorRendererBuilder(this, userAgent, _contentUri);
				default:
					throw new IllegalStateException("Unsupported type: " + _contentType);
			}
		}

		public void PreparePlayer(bool playWhenReady)
		{
			if (_player == null)
			{
				_player = new VideoPlayer(GetRendererBuilder());
				_player.AddListener(this);
				_player.SetCaptionListener(this);
				_player.SetMetadataListener(this);
				_player.SeekTo(_playerPosition);
				_playerNeedsPrepare = true;
				_mediaController.SetMediaPlayer(_player.PlayerControl);
				_mediaController.Enabled = true;
				_eventLogger = new EventLogger();
				_eventLogger.StartSession();
				_player.AddListener(_eventLogger);
				_player.SetInfoListener(_eventLogger);
				_player.SetInternalErrorListener(_eventLogger);
				//_debugViewHelper = new DebugTextViewHelper(_player, _debugTextView);
				//_debugViewHelper.Start();
			}
			if (_playerNeedsPrepare)
			{
				_player.Prepare();
				_playerNeedsPrepare = false;
				UpdateButtonVisibilities();
			}
			_player.Surface = _surfaceView.Holder.Surface;
			_player.PlayWhenReady = playWhenReady;
		}

		public void ReleasePlayer()
		{
			if (_player != null)
			{
                //_debugViewHelper.Stop();
                //_debugViewHelper = null;
				_playerPosition = _player.CurrentPosition;
				_player.Release();
				_player = null;
				_eventLogger.EndSession();
				_eventLogger = null;
			}
		}

		#endregion

		#region DemoPlayer.Listener implementation

		public void OnStateChanged(bool playWhenReady, int playbackState)
		{
            if (playbackState == Exoplayer1.StateEnded)
			{
				ShowControls();
                
			}

            if (playbackState == Exoplayer1.StateReady)
            {
                mProgressBar.Visibility = ViewStates.Gone;
                
            }
			string text ;
			switch (playbackState)
			{
                case Exoplayer1.StateBuffering:
					text = "Buffering";
					break;
                case Exoplayer1.StateEnded:
					text = "Ended";
					break;
                case Exoplayer1.StateIdle:
					text = "Idle";
					break;
                case Exoplayer1.StatePreparing:
					text = "Preparing";
					break;
                case Exoplayer1.StateReady:
					text = "Ready";
					break;
				default:
					text = "Unknown";
					break;
			}
			_playerState = text;
			UpdateButtonVisibilities();
            ShowTheSnackBar(_shutterView);
            
		}

        private void ShowTheSnackBar(View o)
        {
            Snackbar.Make(o, _playerState, Snackbar.LengthLong)
                    .Show();
        }

		public void OnError(Exception e)
		{
			var exception = e as UnsupportedDrmException;
			if (exception != null)
			{
				// Special case DRM failures.
				var stringId = ExoPlayerUtil.SdkInt < 18
					? Resource.String.drm_error_not_supported
					: exception.Reason == UnsupportedDrmException.ReasonUnsupportedScheme
						? Resource.String.drm_error_unsupported_scheme
						: Resource.String.drm_error_unknown;
				Toast.MakeText(ApplicationContext, stringId, ToastLength.Long).Show();
			}
			_playerNeedsPrepare = true;
			UpdateButtonVisibilities();
            ShowRetrySnackbar();
			ShowControls();
		}

        private void ShowRetrySnackbar()
        {
            Snackbar.Make(_shutterView, "Connection Error", Snackbar.LengthIndefinite)
                 .SetAction("Retry", v =>
                 {
                     mProgressBar.Visibility = ViewStates.Visible;
                     PreparePlayer(true);
                 }).Show();
        }

		public void OnVideoSizeChanged(
			int width,
			int height,
			int unappliedRotationDegrees,
			float pixelWidthAspectRatio)
		{
			_shutterView.Visibility = ViewStates.Gone;
			_videoFrame.SetAspectRatio(height == 0 ? 1 : (width*pixelWidthAspectRatio)/height);
		}

		#endregion

		#region User controls

		private void UpdateButtonVisibilities()
		{
			_retryButton.Visibility = _playerNeedsPrepare ? ViewStates.Visible : ViewStates.Gone;
			_videoButton.Visibility = HaveTracks(VideoPlayer.TypeVideo) ? ViewStates.Visible : ViewStates.Gone;
			_audioButton.Visibility = HaveTracks(VideoPlayer.TypeAudio) ? ViewStates.Visible : ViewStates.Gone;
			_textButton.Visibility = HaveTracks(VideoPlayer.TypeText) ? ViewStates.Visible : ViewStates.Gone;
		}

		private bool HaveTracks(int type)
		{
			return _player != null && _player.GetTrackCount(type) > 0;
		}

		[Export("showVideoPopup")]
		// ReSharper disable once UnusedMember.Global
		public void ShowVideoPopup(View v)
		{
			var popup = new PopupMenu(this, v);
			ConfigurePopupWithTracks(popup, null, VideoPlayer.TypeVideo);
			popup.Show();
		}

		[Export("showAudioPopup")]
		// ReSharper disable once UnusedMember.Global
		public void ShowAudioPopup(View v)
		{
			var popup = new PopupMenu(this, v);
			var menu = popup.Menu;
			menu.Add(Menu.None, Menu.None, Menu.None, Resource.String.enable_background_audio);
			var backgroundAudioItem = menu.FindItem(0);
			backgroundAudioItem.SetCheckable(true);
			backgroundAudioItem.SetChecked(_enableBackgroundAudio);

			Func<IMenuItem, bool> clickListener = item =>
			{
				if (item == backgroundAudioItem)
				{
					_enableBackgroundAudio = !item.IsChecked;
					return true;
				}
				return false;
			};
			ConfigurePopupWithTracks(popup, clickListener, VideoPlayer.TypeAudio);
			popup.Show();
		}

		[Export("showTextPopup")]
		// ReSharper disable once UnusedMember.Global
		public void ShowTextPopup(View v)
		{
			var popup = new PopupMenu(this, v);
			ConfigurePopupWithTracks(popup, null, VideoPlayer.TypeText);
			popup.Show();
		}

		[Export("showVerboseLogPopup")]
		// ReSharper disable once UnusedMember.Global
		public void ShowVerboseLogPopup(View v)
		{
			var popup = new PopupMenu(this, v);
			var menu = popup.Menu;
			menu.Add(Menu.None, 0, Menu.None, Resource.String.logging_normal);
			menu.Add(Menu.None, 1, Menu.None, Resource.String.logging_verbose);
			menu.SetGroupCheckable(Menu.None, true, true);
			menu.FindItem((VerboseLogUtil.AreAllTagsEnabled()) ? 1 : 0).SetChecked(true);

			popup.MenuItemClick += (sender, args) =>
			{
				var item = args.Item;
				VerboseLogUtil.SetEnableAllTags(item.ItemId != 0);
			};
			popup.Show();
		}

		private void ConfigurePopupWithTracks(PopupMenu popup, Func<IMenuItem, bool> customActionClickListener, int trackType)
		{
			if (_player == null)
			{
				return;
			}
			var trackCount = _player.GetTrackCount(trackType);
			if (trackCount == 0)
			{
				return;
			}

			popup.MenuItemClick += (sender, args) =>
			{
				var item = args.Item;
				args.Handled = (customActionClickListener != null
				                && customActionClickListener(item))
				               || OnTrackItemClick(item, trackType);
			};

			var menu = popup.Menu;
			// ID_OFFSET ensures we avoid clashing with Menu.NONE (which equals 0)
			menu.Add(MenuGroupTracks, VideoPlayer.TrackDisabled + IdOffset, Menu.None, Resource.String.off);
			for (var i = 0; i < trackCount; i++)
			{
				menu.Add(MenuGroupTracks, i + IdOffset, Menu.None,
					BuildTrackName(_player.GetTrackFormat(trackType, i)));
			}
			menu.SetGroupCheckable(MenuGroupTracks, true, true);
			menu.FindItem(_player.GetSelectedTrack(trackType) + IdOffset).SetChecked(true);
		}

		private static string BuildTrackName(MediaFormat format)
		{
			if (format.Adaptive)
			{
				return "auto";
			}
			string trackName;
			if (MimeTypes.IsVideo(format.MimeType))
			{
				trackName = JoinWithSeparator(JoinWithSeparator(BuildResolutionString(format),
					BuildBitrateString(format)), BuildTrackIdString(format));
			}
			else if (MimeTypes.IsAudio(format.MimeType))
			{
				trackName = JoinWithSeparator(JoinWithSeparator(JoinWithSeparator(BuildLanguageString(format),
					BuildAudioPropertyString(format)), BuildBitrateString(format)),
					BuildTrackIdString(format));
			}
			else
			{
				trackName = JoinWithSeparator(JoinWithSeparator(BuildLanguageString(format),
					BuildBitrateString(format)), BuildTrackIdString(format));
			}
			return trackName.Length == 0 ? "unknown" : trackName;
		}

		private static string BuildResolutionString(MediaFormat format)
		{
			return format.Width == MediaFormat.NoValue || format.Height == MediaFormat.NoValue
				? ""
				: format.Width + "x" + format.Height;
		}

		private static string BuildAudioPropertyString(MediaFormat format)
		{
			return format.ChannelCount == MediaFormat.NoValue || format.SampleRate == MediaFormat.NoValue
				? ""
				: format.ChannelCount + "ch, " + format.SampleRate + "Hz";
		}

		private static string BuildLanguageString(MediaFormat format)
		{
			return TextUtils.IsEmpty(format.Language) || "und".Equals(format.Language)
				? ""
				: format.Language;
		}

		private static string BuildBitrateString(MediaFormat format)
		{
			return format.Bitrate == MediaFormat.NoValue
				? ""
				: String.Format(Locale.Us, "%.2fMbit", format.Bitrate/1000000f);
		}

		private static string JoinWithSeparator(string first, string second)
		{
			return first.Length == 0 ? second : (second.Length == 0 ? first : first + ", " + second);
		}

		private static string BuildTrackIdString(MediaFormat format)
		{
			return format.TrackId == null
				? ""
				: String.Format(Locale.Us, " (%d)", int.Parse(format.TrackId));
		}

		private bool OnTrackItemClick(IMenuItem item, int type)
		{
			if (_player == null || item.GroupId != MenuGroupTracks)
			{
				return false;
			}
			_player.SetSelectedTrack(type, item.ItemId - IdOffset);
			return true;
		}

		private void ToggleControlsVisibility()
		{
			if (_mediaController.IsShowing)
			{
				_mediaController.Hide();
				_debugRootView.Visibility = ViewStates.Gone;
			}
			else
			{
				ShowControls();
			}
		}

		private void ShowControls()
		{
			_mediaController.Show(0);
			_debugRootView.Visibility = ViewStates.Visible;
		}

		#endregion

		#region DemoPlayer.CaptionListener implementation

		public void OnCues(IList<Cue> cues)
		{
			_subtitleLayout.SetCues(cues);
		}

		#endregion

		#region DemoPlayer.MetadataListener implementation

		public void OnId3Metadata(object metadata)
		{
			/*for (Map.Entry<String, Object> entry : metadata.entrySet()) {
      if (TxxxMetadata.TYPE.equals(entry.getKey())) {
        TxxxMetadata txxxMetadata = (TxxxMetadata) entry.getValue();
        Log.i(TAG, String.format("ID3 TimedMetadata %s: description=%s, value=%s",
            TxxxMetadata.TYPE, txxxMetadata.description, txxxMetadata.value));
      } else if (PrivMetadata.TYPE.equals(entry.getKey())) {
        PrivMetadata privMetadata = (PrivMetadata) entry.getValue();
        Log.i(TAG, String.format("ID3 TimedMetadata %s: owner=%s",
            PrivMetadata.TYPE, privMetadata.owner));
      } else if (GeobMetadata.TYPE.equals(entry.getKey())) {
        GeobMetadata geobMetadata = (GeobMetadata) entry.getValue();
        Log.i(TAG, String.format("ID3 TimedMetadata %s: mimeType=%s, filename=%s, description=%s",
            GeobMetadata.TYPE, geobMetadata.mimeType, geobMetadata.filename,
            geobMetadata.description));
      } else {
        Log.i(TAG, String.format("ID3 TimedMetadata %s", entry.getKey()));
      }
    }*/
		}

		#endregion

		#region SurfaceHolder.Callback implementation

		public void SurfaceCreated(ISurfaceHolder holder)
		{
			if (_player != null)
			{
				_player.Surface = holder.Surface;
			}
		}

		public void SurfaceChanged(ISurfaceHolder holder, Format format, int width, int height)
		{
			// Do nothing.
		}

		public void SurfaceDestroyed(ISurfaceHolder holder)
		{
			if (_player != null)
			{
				_player.BlockingClearSurface();
			}
		}

		#endregion

		public void ConfigureSubtitleView()
		{
			CaptionStyleCompat style;
			float fontScale;
            if (ExoPlayerUtil.SdkInt >= 19)
            {
                style = GetUserCaptionStyleV19();
                fontScale = GetUserCaptionFontScaleV19();
            }
            else
            {
				style = CaptionStyleCompat.Default;
				fontScale = 1.0f;
			}
			_subtitleLayout.SetStyle(style);
			_subtitleLayout.SetFractionalTextSize(SubtitleLayout.DefaultTextSizeFraction*fontScale);
		}

        private float GetUserCaptionFontScaleV19()
        {
            var captioningManager =
                (CaptioningManager)GetSystemService(CaptioningService);
            return captioningManager.FontScale;
        }

        private CaptionStyleCompat GetUserCaptionStyleV19()
        {
            var captioningManager =
                (CaptioningManager)GetSystemService(CaptioningService);
            return CaptionStyleCompat.CreateFromCaptionStyle(captioningManager.UserStyle);
        }

		/// <summary>
		/// Makes a best guess to infer the type from a media <see cref="Uri"/> and an optional overriding file extension.
		/// </summary>
		/// <param name="uri">The <see cref="Uri"/> of the media.</param>
		/// <param name="fileExtension">An overriding file extension.</param>
		/// <returns>The inferred type.</returns>
		public static int InferContentType(Uri uri, string fileExtension)
		{
			var lastPathSegment = !string.IsNullOrEmpty(fileExtension)
				? "." + fileExtension
				: uri.LastPathSegment;
			if (lastPathSegment == null)
			{
				return TypeOther;
			}
			if (lastPathSegment.EndsWith(ExtDash))
			{
				return TypeDash;
			}
			if (lastPathSegment.EndsWith(ExtSs))
			{
				return TypeSs;
			}
			if (lastPathSegment.EndsWith(ExtHls))
			{
				return TypeHls;
			}
			return TypeOther;
		}

		private class KeyCompatibleMediaController : MediaController
		{
			private IMediaPlayerControl _playerControl;

			public KeyCompatibleMediaController(Context context) : base(context)
			{
			}

			public override void SetMediaPlayer(IMediaPlayerControl playerControl)
			{
				base.SetMediaPlayer(playerControl);
				_playerControl = playerControl;
			}

			public override bool DispatchKeyEvent(KeyEvent ev)
			{
				var keyCode = ev.KeyCode;
				if (_playerControl.CanSeekForward() && (keyCode == Keycode.MediaFastForward || keyCode == Keycode.DpadRight))
				if (_playerControl.CanSeekForward() && keyCode == Keycode.MediaFastForward)
				{
					if (ev.Action == KeyEventActions.Down)
					{
						_playerControl.SeekTo(_playerControl.CurrentPosition + 15000); // milliseconds
						Show();
					}
					return true;
				}
				else if (_playerControl.CanSeekBackward() && (keyCode == Keycode.MediaRewind || keyCode == Keycode.DpadLeft))
				{
					if (ev.Action == KeyEventActions.Down)
					{
						_playerControl.SeekTo(_playerControl.CurrentPosition - 5000); // milliseconds
						Show();
					}
					return true;
				}
				return base.DispatchKeyEvent(ev);
			}
		}
	}
}