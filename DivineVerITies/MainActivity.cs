using System;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using Android.Support.V7.App;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using DivineVerITies.Fragments;

namespace DivineVerITies
{
    [Activity(MainLauncher = true, Icon = "@drawable/icon", Theme = "@style/Theme.DesignDemo")]
    public class MainActivity : AppCompatActivity
    {

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            CollapsingToolbarLayout collapsingToolBar = FindViewById<CollapsingToolbarLayout>(Resource.Id.collapsing_toolbar);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolbar);
            SetSupportActionBar(toolBar);

            TabLayout tabs = FindViewById<TabLayout>(Resource.Id.tabs);

            ViewPager viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);

            SetUpViewPager(viewPager);

            tabs.SetupWithViewPager(viewPager);

            Button mButtonSignUp = FindViewById<Button>(Resource.Id.btnSignUp);
            mButtonSignUp.Click += mButtonSignUp_Click;

            Button mButtonSignIn = FindViewById<Button>(Resource.Id.btnSignIn);
            mButtonSignIn.Click += mButtonSignIn_Click;
        }

        void mButtonSignIn_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(RecommendedForYou));
            StartActivity(intent);
            this.Finish();
        }

        void mButtonSignUp_Click(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        private void SetUpViewPager(ViewPager viewPager)
        {
            TabAdapter adapter = new TabAdapter(SupportFragmentManager);
            adapter.AddFragment(new Fragment1(), "CREATE ACCOUNT");
            adapter.AddFragment(new Fragment2(), "SIGN IN");

            viewPager.Adapter = adapter;
        }
    }
}

