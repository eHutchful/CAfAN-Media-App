
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
    public class Fragment4 : SupportFragment
    {
        private ProgressBar mProgresBar;
        private SwipeRefreshLayout swipeRefreshLayout;
        private RecyclerView recyclerView;
        private VideoRecyclerViewAdapter mVideoAdapter;
        public List<Video> mVideos;
        public List<Video> mlist;
        private View view;
        private Android.Support.V7.Widget.SearchView mSearchView;
        private Android.Support.V7.App.AlertDialog.Builder builder;
        String[] sortitems = { "Title Ascending", "Title Descending", "Date Ascending", "Date Descending" };

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
            swipeRefreshLayout.SetColorSchemeColors(Color.Indigo, Color.Pink, Color.Blue, Color.Yellow);
            SetUpVideoRecyclerView(recyclerView);
            return view;
        }
        private void SetUpVideoRecyclerView(RecyclerView recyclerView)
        {

            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));

            recyclerView.SetItemClickListener((rv, position, view) =>
            {
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;
                Intent intent = new Intent(context, typeof(VideoCastDetails));
                var serial = JsonConvert.SerializeObject(mVideos[position]);
                intent.PutExtra("selectedItem", serial);
                context.StartActivity(intent);
            });
        }
        private async Task getVideosAsync()
        {
            try
            {
                mVideos = await (new Initialize()).getVideoList();
                mVideoAdapter = new VideoRecyclerViewAdapter(recyclerView.Context, mVideos, Activity.Resources);
                recyclerView.SetAdapter(mVideoAdapter);

                recyclerView.Visibility = ViewStates.Visible;
                mProgresBar.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                mProgresBar.Visibility = ViewStates.Gone;
                Snackbar.Make(view, "Connection Error", Snackbar.LengthIndefinite)
                 .SetAction("Retry", async v =>
                 {
                     mProgresBar.Visibility = ViewStates.Visible;
                     await getVideosAsync();
                 }).Show();
            }
        }
        public override async void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            await getVideosAsync();

            HasOptionsMenu = true;

            swipeRefreshLayout.Refresh += swipeRefreshLayout_Refresh;


        }
        private async void swipeRefreshLayout_Refresh(object sender, EventArgs e)
        {
            await getVideosAsync();

            swipeRefreshLayout.Refreshing = false;
        }
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var item = menu.FindItem(Resource.Id.action_search);
            var searchView = MenuItemCompat.GetActionView(item);
            mSearchView = searchView.JavaCast<Android.Support.V7.Widget.SearchView>();
            mSearchView.QueryTextChange += (s, e) => mVideoAdapter.Filter.InvokeFilter(e.NewText);
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
            base.OnAttach(Activity);
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
                               select video).ToList<Video>();

                    mVideoAdapter = new VideoRecyclerViewAdapter(recyclerView.Context, mVideos, Activity.Resources);
                    recyclerView.SetAdapter(mVideoAdapter);
                    builder.Dispose();
                    break;

                case 1:
                    mVideos = (from video in mVideos
                               orderby video.Title descending
                               select video).ToList<Video>();

                    mVideoAdapter = new VideoRecyclerViewAdapter(recyclerView.Context, mVideos, Activity.Resources);
                    recyclerView.SetAdapter(mVideoAdapter);
                    builder.Dispose();
                    break;

                case 2:
                    mVideos = (from video in mVideos
                               orderby video.PubDate
                               select video).ToList<Video>();

                    mVideoAdapter = new VideoRecyclerViewAdapter(recyclerView.Context, mVideos, Activity.Resources);
                    recyclerView.SetAdapter(mVideoAdapter);
                    builder.Dispose();
                    break;

                case 3:
                    mVideos = (from video in mVideos
                               orderby video.PubDate descending
                               select video).ToList<Video>();

                    mVideoAdapter = new VideoRecyclerViewAdapter(recyclerView.Context, mVideos, Activity.Resources);
                    recyclerView.SetAdapter(mVideoAdapter);
                    builder.Dispose();
                    break;
            }
        }
    }
}