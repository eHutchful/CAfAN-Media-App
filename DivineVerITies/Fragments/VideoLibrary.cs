
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using System.Collections.Generic;
using Android.Graphics;
using DivineVerITies.Helpers;
using Android.Content;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Runtime;
using System.Linq;

namespace DivineVerITies.Fragments
{
    public class VideoLibrary : SupportFragment
    {
        private ProgressBar mProgresBar;
        private SwipeRefreshLayout swipeRefreshLayout;
        private RecyclerView recyclerView;
        private VideoRecyclerViewAdapter mVideoAdapter;
        public static List<Video> mVideos;
        public List<Video> mlist;
        private View view;
        private Android.Support.V7.Widget.SearchView mSearchView;
        private Android.Support.V7.App.AlertDialog.Builder builder;
        string[] sortitems = { "Title Ascending", "Title Descending", "Date Ascending", "Date Descending" };


        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            view = inflater.Inflate(Resource.Layout.Fragment4, container, false) as View;
            recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerview);
            mProgresBar = view.FindViewById<ProgressBar>(Resource.Id.audio_player_loading);
            mProgresBar.Animate();
            swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            swipeRefreshLayout.SetColorSchemeColors(Color.ParseColor("#067ab4"));
            SetUpVideoRecyclerView(recyclerView);
            return view;
        }

        private void SetUpVideoRecyclerView(RecyclerView recyclerView)
        {

            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));

            recyclerView.SetItemClickListener((rv, position, view) =>
            {

                string serial;
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;
                Intent intent = new Intent(context, typeof(VideoCastDetails));

                if (mVideoAdapter.mVideos == null)
                { serial = JsonConvert.SerializeObject(mVideos[position]); }

                else if (mVideoAdapter.mVideos.Count == mVideos.Count)
                { serial = JsonConvert.SerializeObject(mVideos[position]); }

                else
                { serial = JsonConvert.SerializeObject(mVideoAdapter.mVideos[position]); }

                intent.PutExtra("selectedItem", serial);
                context.StartActivity(intent);
            });
        }

        private async Task getVideosAsync(bool userSwiped)
        {
            try
            {
                mVideos = await (new Initialize()).getVideoList();
                mVideoAdapter = new VideoRecyclerViewAdapter(recyclerView.Context, mVideos, Activity.Resources);
                recyclerView.SetAdapter(mVideoAdapter);

                recyclerView.Visibility = ViewStates.Visible;
                mProgresBar.Visibility = ViewStates.Gone;

                if (userSwiped)
                {
                    swipeRefreshLayout.Refreshing = false;
                }
            }
            catch (Exception e)
            {
                mProgresBar.Visibility = ViewStates.Gone;
                Snackbar.Make(view, "Connection Error", Snackbar.LengthIndefinite)
                 .SetAction("Retry", async v =>
                 {
                     mProgresBar.Visibility = ViewStates.Visible;
                     await getVideosAsync(false);
                 }).Show();
            }
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            HasOptionsMenu = true;
            swipeRefreshLayout.Refresh += swipeRefreshLayout_Refresh;
        }

        private async void swipeRefreshLayout_Refresh(object sender, EventArgs e)
        {
            await getVideosAsync(true);
            
        }

        public async override void OnStart()
        {
            base.OnStart();
            await getVideosAsync(false);
        }
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var item = menu.FindItem(Resource.Id.action_search);
            var searchView = MenuItemCompat.GetActionView(item);
            mSearchView = searchView.JavaCast<Android.Support.V7.Widget.SearchView>();
            mSearchView.QueryTextChange += (s, e) =>
            {
                mVideoAdapter.Filter.InvokeFilter(e.NewText);
                mVideoAdapter.searchString = e.NewText;
            };
            mSearchView.QueryTextSubmit += (s, e) =>
            {
                Toast.MakeText(Activity, "Searched for: " + e.Query, ToastLength.Short).Show();
                e.Handled = true;
            };

            MenuItemCompat.SetOnActionExpandListener(item, new SearchViewExpandListener(mVideoAdapter));
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_sort:
                    SortVideos();
                    return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        private void SortVideos()
        {
            builder = new Android.Support.V7.App.AlertDialog.Builder(Activity);
            builder.SetTitle("Sort Order")
                   .SetSingleChoiceItems(sortitems, -1, SortTypeListClicked)
                   .SetNegativeButton("Cancel", delegate { builder.Dispose(); });
            builder.Create().Show();
        }

        private void SortTypeListClicked(object sender, DialogClickEventArgs e)
        {
            switch (e.Which)
            {
                case 0:
                    mVideos = (from video in mVideos
                               orderby video.Title
                               select video).ToList();

                    mVideoAdapter = new VideoRecyclerViewAdapter(recyclerView.Context, mVideos, Activity.Resources);
                    recyclerView.SetAdapter(mVideoAdapter);
                    builder.Dispose();
                    break;

                case 1:
                    mVideos = (from video in mVideos
                               orderby video.Title descending
                               select video).ToList();

                    mVideoAdapter = new VideoRecyclerViewAdapter(recyclerView.Context, mVideos, Activity.Resources);
                    recyclerView.SetAdapter(mVideoAdapter);
                    builder.Dispose();
                    break;

                case 2:
                    mVideos = (from video in mVideos
                               orderby video.PubDate
                               select video).ToList();

                    mVideoAdapter = new VideoRecyclerViewAdapter(recyclerView.Context, mVideos, Activity.Resources);
                    recyclerView.SetAdapter(mVideoAdapter);
                    builder.Dispose();
                    break;

                case 3:
                    mVideos = (from video in mVideos
                               orderby video.PubDate descending
                               select video).ToList();

                    mVideoAdapter = new VideoRecyclerViewAdapter(recyclerView.Context, mVideos, Activity.Resources);
                    recyclerView.SetAdapter(mVideoAdapter);
                    builder.Dispose();
                    break;
            }
        }
    }
}