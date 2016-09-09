using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Views;
using DivineVerITies.ExoPlayer;
using DivineVerITies.Fragments;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;

namespace DivineVerITies
{
    [Activity(Theme = "@style/Theme.DesignDemo")]
    public class MainApp : AppCompatActivity
    {
        private DrawerLayout mDrawerLayout;
        private TabLayout tabs;
        private int[] tabIcons = {
            Resource.Drawable.ic_library_music,
            Resource.Drawable.ic_video_library,
            Resource.Drawable.ic_subscriptions
            //Resource.Drawable.ic_explore
                                 };


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.MainApp);

            SupportToolbar toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);
            SupportActionBar.Title = "DivineVerITies";
            SupportActionBar.SetHomeAsUpIndicator(Resource.Drawable.ic_menu);
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            mDrawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);


            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            if (navigationView != null)
            {
                SetUpDrawerContent(navigationView);
            }

            tabs = FindViewById<TabLayout>(Resource.Id.tabs);

            ViewPager viewPager = FindViewById<ViewPager>(Resource.Id.viewpager);

            SetUpViewPager(viewPager);

            tabs.SetupWithViewPager(viewPager);

            setupTabIcons();

            FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            fab.Click += (o, e) =>
            {
                View anchor = o as View;

                if (tabs.SelectedTabPosition == 0)
                {
                    Snackbar.Make(anchor, "No Connection", Snackbar.LengthLong)
                       .SetAction("Retry", v =>
                       {
                           //Do something here
                           //Intent intent = new Intent(fab.Context, typeof(BottomSheetActivity));
                           //StartActivity(intent);
                       })
                       .Show();
                }

                if (tabs.SelectedTabPosition == 1)
                {
                    Snackbar.Make(anchor, "No Connection", Snackbar.LengthLong)
                        .SetAction("Retry", v =>
                        {
                            //Do something here
                            //Intent intent = new Intent(fab.Context, typeof(BottomSheetActivity));
                            //StartActivity(intent);
                        })
                        .Show();
                }

                if (tabs.SelectedTabPosition == 2)
                {
                    Android.Content.Intent intent = new Android.Content.Intent(this, typeof(SampleChooserActivity));
                    StartActivity(intent);
                }
            };
        }

        private void setupTabIcons()
        {
            tabs.GetTabAt(0).SetIcon(tabIcons[0]);
            tabs.GetTabAt(1).SetIcon(tabIcons[1]);
            tabs.GetTabAt(2).SetIcon(tabIcons[2]);
            //tabs.GetTabAt(3).SetIcon(tabIcons[3]);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    mDrawerLayout.OpenDrawer((int)GravityFlags.Left);
                    return true;

                //case Resource.Id.action_sort:
                //    return true;

                case Resource.Id.action_appSettings:
                    return true;

                case Resource.Id.action_signOut:
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.sample_actions, menu);
            return true;
        }

        private void SetUpDrawerContent(NavigationView navigationView)
        {
            navigationView.NavigationItemSelected += (object sender, NavigationView.NavigationItemSelectedEventArgs e) =>
            {
                e.MenuItem.SetChecked(true);

                mDrawerLayout.CloseDrawers();

                switch (e.MenuItem.ItemId)
                {
                    case Resource.Id.nav_recommended:
                        Intent intent = new Intent(this, typeof(SelectTopics));
                        StartActivity(intent);
                        break;
                }
            };
        }

        private void SetUpViewPager(ViewPager viewPager)
        {
            TabAdapter adapter = new TabAdapter(SupportFragmentManager);
            adapter.AddFragment(new Fragment3(), "AUDIOS");
            adapter.AddFragment(new Fragment4(), "VIDEOS");
            adapter.AddFragment(new Fragment5(), "SUBSCRIPTIONS");
            //adapter.AddFragment(new Fragment6(), "EXPLORE");

            viewPager.Adapter = adapter;
        }
    }
}