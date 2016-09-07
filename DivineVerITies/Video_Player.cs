using System;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Plugin.Connectivity;
using Plugin.Connectivity.Abstractions;
using DivineVerITies.Helpers;
using Android.Content;

namespace DivineVerITies
{
    [Activity(Label = "VideoPlayer")]
    public class Video_Player : Activity
    {
        private const string VideoPositionKey = "VideoPosition";
        private int startingPosition;
        public static Video selectedVideo;
        private MediaController mediaController;
        private VideoView videoPlayer;
        private ProgressBar videoProgressBar;
        private TextView statusMessageTextView;
        private void InitializeVideoPlayer()
        {
            
            videoPlayer = FindViewById<VideoView>(Resource.Id.PlayerVideoView);
            mediaController = new MediaController(this, true);
            videoProgressBar = FindViewById<ProgressBar>(Resource.Id.VideoProgressBar);
            statusMessageTextView = FindViewById<TextView>(Resource.Id.StatusMessageTextView);
            videoPlayer.SetMediaController(mediaController);

        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);            
            SetContentView(Resource.Layout.VidPlayer);
            startingPosition = (bundle != null) ? bundle.GetInt(VideoPositionKey):0;
            InitializeVideoPlayer();                     
            
        }
        protected override void OnStart()
        {
            base.OnStart();
            CrossConnectivity.Current.ConnectivityChanged += OnCurrentConnectivityChanged;
            videoPlayer.Prepared += OnVideoPlayerPrepared;
            LaunchVideo();
        }
        protected override void OnStop()
        {
            base.OnStop();
            CrossConnectivity.Current.ConnectivityChanged -= OnCurrentConnectivityChanged;
            videoPlayer.Prepared -= OnVideoPlayerPrepared;
        }
        private void LaunchVideo()
        {
            if(CrossConnectivity.Current.IsConnected && !videoPlayer.IsPlaying)
            {
                string videoUri = selectedVideo.Link;
                videoPlayer.SetVideoURI(Android.Net.Uri.Parse(videoUri));
                videoPlayer.SeekTo(startingPosition);
                videoPlayer.Start();
            }
           
        }
        private void OnVideoPlayerPrepared(object sender, EventArgs e)
        {
            UpdateVideoPlayerState(true);
            mediaController.SetAnchorView(videoPlayer);
            //show media controls for 3 seconds when video starts to play
            mediaController.Show(3000);
        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            base.OnSaveInstanceState(outState);
            startingPosition = (videoPlayer.IsPlaying) ?
                videoPlayer.CurrentPosition :
                startingPosition;
            outState.PutInt(VideoPositionKey, startingPosition);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            base.OnRestoreInstanceState(savedInstanceState);
            if(!videoPlayer.IsPlaying)
            {
                startingPosition = (savedInstanceState !=null) ?
                   savedInstanceState.GetInt(VideoPositionKey):
                   0;
                UpdateVideoPlayerState(false);
                LaunchVideo();
            }
        }

        private void UpdateVideoPlayerState(bool playVideo)
        {
            if (!CrossConnectivity.Current.IsConnected)
            {
                statusMessageTextView.Visibility = ViewStates.Visible;
                videoProgressBar.Visibility = ViewStates.Gone;
                videoPlayer.Visibility = ViewStates.Gone;
            }
            else if (!videoPlayer.IsPlaying)
            {
                videoProgressBar.Visibility = (playVideo) 
                    ? ViewStates.Gone 
                    : ViewStates.Visible;
                videoPlayer.Visibility = ViewStates.Visible;
            }

        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            UpdateVideoPlayerState(false);
        }
        private void OnCurrentConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
        {
            UpdateVideoPlayerState(false);
            LaunchVideo();
        }        
    }


}
