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
using Android.Support.V7.Widget;
using Android.Support.V4.Widget;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Graphics;
using Android.Support.Design.Widget;
using DivineVerITies.Helpers;

namespace DivineVerITies
{
    [Activity(Theme = "@style/Theme.DesignDemo")]
    public class MessageBoard : AppCompatActivity
    {
        private SupportToolbar toolBar;
        private ProgressBar mProgressBar;
        private SwipeRefreshLayout swipeRefreshLayout;
        private RecyclerView recyclerView;
        private AnnouncementRecyclerAdapter mAdapter;
        private List<Announcement> mAnnouncements;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.MessageBoard);

            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);
            SupportActionBar.Title = "Announcements";
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);


            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerview);
            SetUpRecyclerView(recyclerView);
            mProgressBar = FindViewById<ProgressBar>(Resource.Id.message_loading);
            swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            swipeRefreshLayout.SetColorSchemeColors(Color.Purple);

            swipeRefreshLayout.Refresh += delegate
            {
                GetAnnouncements();
            };
        }

        protected override void OnStart()
        {
            base.OnStart();
            GetAnnouncements();
        }

        private async void GetAnnouncements()
        {
            try
            {

                mAnnouncements = await (new Initialize()).getAnnouncements();
                //mAnnouncements.Add(new Announcement { message = "lorem ipsum", timestamp = "today"});
                mAdapter = new AnnouncementRecyclerAdapter(recyclerView.Context, mAnnouncements, Resources);
                recyclerView.SetAdapter(mAdapter);
                recyclerView.Visibility = ViewStates.Visible;
                mProgressBar.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                mProgressBar.Visibility = ViewStates.Gone;
                Snackbar.Make(recyclerView, "Connection Error", Snackbar.LengthIndefinite)
                 .SetAction("Retry", v =>
                 {
                     recyclerView.Visibility = ViewStates.Gone;
                     mProgressBar.Visibility = ViewStates.Visible;
                     GetAnnouncements();
                 }).Show();
            }
        }

        private void SetUpRecyclerView(RecyclerView recyclerView)
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));
            recyclerView.SetItemAnimator(new DefaultItemAnimator());
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