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
        private void InitializeVideoPlayer()
        {
            videoPlayer = FindViewById<VideoView>(Resource.Id.PlayerVideoView);
            mediaController = new MediaController(this, true);
            videoProgressBar = FindViewById<ProgressBar>(Resource.Id.VideoProgressBar);
            videoPlayer.SetMediaController(mediaController);

        }
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            SetContentView(Resource.Layout.video_player);
            startingPosition = (bundle != null) ? bundle.GetInt(VideoPositionKey):0;
            InitializeVideoPlayer();                     
            
        }
        protected override void OnStart()
        {
            base.OnStart();
            videoPlayer.Prepared += OnVideoPlayerPrepared;
            LaunchVideo();
        }
        protected override void OnStop()
        {
            base.OnStop();
            videoPlayer.Prepared -= OnVideoPlayerPrepared;
        }
        private void LaunchVideo()
        {
            string videoUri =selectedVideo.Link;
            videoPlayer.SetVideoURI(Android.Net.Uri.Parse(videoUri));
            videoPlayer.SeekTo(startingPosition);
            videoPlayer.Start();
        }
        private void OnVideoPlayerPrepared(object sender, EventArgs e)
        {
            videoProgressBar.Visibility = ViewStates.Gone;
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
            if (!videoPlayer.IsPlaying)
            {
                videoProgressBar.Visibility = (playVideo) ? ViewStates.Gone : ViewStates.Visible;
                videoPlayer.Visibility = ViewStates.Visible;
            }

        }

        public override void OnConfigurationChanged(Android.Content.Res.Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            UpdateVideoPlayerState(false);
        }

    }


}
