using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using DivineVerITies.ExoPlayer;
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
        private TextView mAudioCategory;
        private TextView mAudioSize;

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
            fab.Visibility = ViewStates.Visible;

            fab.Click += (o, e) =>
            {
                View anchor = o as View;

                PlayerActivity.selectedVideo = selectedVideo;
                try { StopService(new Intent(this, typeof(MediaPlayerService))); }
                catch (Exception es) { }
                var mpdIntent = new Intent(this, typeof(PlayerActivity))
                    .SetData(Android.Net.Uri.Parse(selectedVideo.Link))
                    .PutExtra(PlayerActivity.ContentIdExtra, 3)
                    .PutExtra(PlayerActivity.ContentTypeExtra, PlayerActivity.TypeOther);
                StartActivity(mpdIntent);
            };
            TextView mAudioDescription = FindViewById<TextView>(Resource.Id.AudioDescription);
            mAudioDescription.Text = selectedVideo.Description;
            mAudioCategory = FindViewById<TextView>(Resource.Id.category);
            mAudioCategory.Text = selectedVideo.Category;

            mAudioSize = FindViewById<TextView>(Resource.Id.size);
            mAudioSize.Text = ((int)ConvertBytesToMegabytes(long.Parse(selectedVideo.Size))).ToString() + " MB";

        }

        static double ConvertBytesToMegabytes(long bytes)
        {
            return (bytes / 1024f) / 1024f;
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
        //public override bool OnCreateOptionsMenu(IMenu menu)
        //{
        //    MenuInflater.Inflate(Resource.Menu.options_only, menu);
        //    return true;
        //}
        protected override void OnStart()
        {
            base.OnStart();
            ImageView mAlbumArt = FindViewById<ImageView>(Resource.Id.backdrop);

            Glide.With(this)
                   .Load(selectedVideo.ImageUrl)
                   .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                   .Error(Resource.Drawable.ChurchLogo_Gray)
                   //.DiskCacheStrategy(DiskCacheStrategy.All)
                   .Into(mAlbumArt);
            ProgressBar pBar = FindViewById<ProgressBar>(Resource.Id.image_loading);
            pBar.Visibility = ViewStates.Gone;
        }

    }
}
