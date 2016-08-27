
using Android.App;
using Android.Graphics;
using Android.Net;
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
using System.Net.Http;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;

namespace DivineVerITies
{
    
    [Activity(Theme = "@style/Theme.DesignDemo")]
    public class PodcastDetails : AppCompatActivity
    {
        //bool isVisible = true;
        AudioList selectedAudio;
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

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            fab.Click += (o, e) =>
            {
                View anchor = o as View;

                //var mpdIntent = new Android.Content.Intent(this, typeof(PlayerActivity))
                //.SetData(Uri.Parse(selectedAudio.Link))
                //    .PutExtra(PlayerActivity.ContentIdExtra, 3)
                //.PutExtra(PlayerActivity.ContentTypeExtra, PlayerActivity.TypeOther);
                //StartActivity(mpdIntent);

                Android.Content.Intent intent = new Android.Content.Intent(this, typeof(Audio_Player));
                var serial = JsonConvert.SerializeObject(selectedAudio);
                intent.PutExtra("selectedItem", serial);

                StartActivity(intent);
            };

            

            TextView mAudioDescription = FindViewById<TextView>(Resource.Id.AudioDescription);
            mAudioDescription.Text = selectedAudio.Description;

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
            //if (isVisible)
            //{
            ImageView mAlbumArt = FindViewById<ImageView>(Resource.Id.backdrop);
            //    Bitmap imageBitmap=null;
            //    using (var client = new HttpClient())
            //    {                  
            //        var imageBytes = await client.GetByteArrayAsync(selectedAudio.ImageUrl);
                        
            //        if (imageBytes != null && imageBytes.Length > 0)
            //        {
            //            imageBitmap = BitmapFactory.DecodeByteArray(imageBytes, 0, imageBytes.Length);
            //        }
            //    }                
            //    mAlbumArt.SetImageBitmap(imageBitmap);

             Glide.With(this)
                    .Load(selectedAudio.ImageUrl)
                    .Placeholder(Resource.Drawable.Logo_trans192)
                    .Error(Resource.Drawable.Logo_trans192)
                    //.SkipMemoryCache(true)
                    .DiskCacheStrategy(DiskCacheStrategy.All)
                    .Into(mAlbumArt);
                ProgressBar pBar= FindViewById<ProgressBar>(Resource.Id.image_loading);
                pBar.Visibility = ViewStates.Gone;
                //isVisible = false;
           // }

        }

    }
}