using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using DivineVerITies.ExoPlayer.Player;
using DivineVerITies.Helpers;
using Newtonsoft.Json;
using System;
using System.Net;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;


namespace DivineVerITies
{
    [Activity(Theme = "@style/Theme.DesignDemo")]
    public class VideoCastDetails: AppCompatActivity
    {
        //bool isVisible = true;

        Video selectedVideo;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PodcastDetails);

            selectedVideo = JsonConvert.DeserializeObject<Video>(Intent.GetStringExtra("selectedItem"));

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            CollapsingToolbarLayout collapsingToolBar = FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);
            collapsingToolBar.Title = selectedVideo.Title;
            collapsingToolBar.SetExpandedTitleColor(Android.Resource.Color.Transparent);

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            fab.Click += (o, e) =>
            {
                View anchor = o as View;
                Android.Content.Intent intent = new Android.Content.Intent(this, typeof(Video_Player));
                Video_Player.selectedVideo = selectedVideo;
                StartActivity(intent);
            };
            TextView mAudioDescription = FindViewById<TextView>(Resource.Id.AudioDescription);
            mAudioDescription.Text = selectedVideo.Description;

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
        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.options_only, menu);
            return true;
        }
        protected async override void OnStart()
        {
            base.OnStart();
            ImageView mAlbumArt = FindViewById<ImageView>(Resource.Id.backdrop);

            Glide.With(this)
                   .Load(selectedVideo.ImageUrl)
                   .Placeholder(Resource.Drawable.Logo_trans192)
                   .Error(Resource.Drawable.Logo_trans192)
                   .DiskCacheStrategy(DiskCacheStrategy.All)
                   .Into(mAlbumArt);
            ProgressBar pBar = FindViewById<ProgressBar>(Resource.Id.image_loading);
            pBar.Visibility = ViewStates.Gone;
        }

    }
}