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
using Android.Support.V7.App;
using DivineVerITies.Helpers;
using Android.Support.V7.Widget;
using Android.Webkit;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;

namespace DivineVerITies
{
    [Activity(Theme = "@style/Theme.DesignDemo")]
    public class Terms : AppCompatActivity
    {
        private SupportToolbar toolBar;
        private ProgressBar mProgressBar;
        private CustomWebViewClient mWebClient;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Privacy);
            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);
            SupportActionBar.Title = "Terms of Use";
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            mWebClient = new CustomWebViewClient(this);

            mWebClient.mOnProgressChanged += (int state) =>
            {
                if (state == 0)
                {
                    //Done loading, hide progress bar
                    mProgressBar.Visibility = ViewStates.Invisible;
                }

                else
                {
                    mProgressBar.Visibility = ViewStates.Visible;
                }
            };

            WebView view = new WebView(this);
            view.VerticalScrollBarEnabled = false;

            (FindViewById<CardView>(Resource.Id.privacyCard)).AddView(view);
            mProgressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);

            view.LoadData(GetString(Resource.String.terms), "text/html; charset=utf-8", "utf-8");
            view.SetWebViewClient(mWebClient);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}