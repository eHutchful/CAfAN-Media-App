
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
    public class PodcastDetails : AppCompatActivity
    {
        //bool isVisible = true;

        AudioList selectedAudio;
        TextView mAudioDescription;
        TextView mAudioCategory;
        TextView mAudioSize;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.PodcastDetails);

            selectedAudio = JsonConvert.DeserializeObject<AudioList>(Intent.GetStringExtra("selectedItem"));

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            CollapsingToolbarLayout collapsingToolBar = FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);
            collapsingToolBar.Title = selectedAudio.Title;
            collapsingToolBar.SetExpandedTitleColor(Android.Resource.Color.Transparent);

            //FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            //fab.Click += (o, e) =>
            //{
            //    MainApp.visibility = ViewStates.Visible;
            //    // View anchor = o as View;
            //    Intent intent = new Intent(this, typeof(MainApp));
            //    MediaPlayerService.selectedAudio = selectedAudio;
            //    try { StopService(new Intent(this, typeof(MediaPlayerService))); }
            //    catch (Exception es) { }
            //    StartActivity(intent);
            //};
            mAudioDescription = FindViewById<TextView>(Resource.Id.AudioDescription);
            mAudioDescription.Text = selectedAudio.Description;

            mAudioCategory = FindViewById<TextView>(Resource.Id.category);
            mAudioCategory.Text = selectedAudio.Category;

            mAudioSize = FindViewById<TextView>(Resource.Id.size);
            mAudioSize.Text = ((int)ConvertBytesToMegabytes(long.Parse(selectedAudio.Size))).ToString() + " MB";

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

                //case Resource.Id.action_Download:
                //    MyService.selectedAudio = selectedAudio;
                //    MyService.contxt = this;
                //    var intent = new Intent(this, typeof(MyService));
                //    intent.SetAction(MyService.StartD);
                //    StartService(intent);
                //    return true;
            }

            return base.OnOptionsItemSelected(item);
        }
        //public override bool OnCreateOptionsMenu(IMenu menu)
        //{
        //    MenuInflater.Inflate(Resource.Menu.menu_album, menu);
        //    return true;
        //}
        protected override void OnStart()
        {
            base.OnStart();            
            ImageView mAlbumArt = FindViewById<ImageView>(Resource.Id.backdrop);
           
             Glide.With(this)
                    .Load(selectedAudio.ImageUrl)
                    .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                    .Error(Resource.Drawable.ChurchLogo_Gray)                   
                    //.DiskCacheStrategy(DiskCacheStrategy.All)
                    .Into(mAlbumArt);
                ProgressBar pBar= FindViewById<ProgressBar>(Resource.Id.image_loading);
                pBar.Visibility = ViewStates.Gone;                
        }

    }
}