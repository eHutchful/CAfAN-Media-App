
using System;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using DivineVerITies.Helpers;
using Android.Content;
using System.Collections.Generic;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Runtime;

namespace DivineVerITies.Fragments
{
    public class Fragment5 : SupportFragment
    {
        private RecyclerView audioRecyclerView;
        private RecyclerView videoRecyclerView;
        private ProgressBar audioProgressBar;
        private ProgressBar videoProgressBar;
        private TextView audioHeading;
        private TextView videoHeading;
        private AudioAlbumRecyclerViewAdapter mAudioAdapter;
        private VideoAlbumRecyclerViewAdapter mVideoAdapter;
        private List<AudioList> mAudios;
        private List<Video> mVideos;
        View view;
        private Android.Support.V7.Widget.SearchView mSearchView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            view = inflater.Inflate(Resource.Layout.OfflinePodcast, container, false) as View;
            audioProgressBar = view.FindViewById<ProgressBar>(Resource.Id.audio_loading);
            videoProgressBar = view.FindViewById<ProgressBar>(Resource.Id.video_loading);
            audioHeading = view.FindViewById<TextView>(Resource.Id.audio_heading);
            audioHeading.Text = "Favourite Audios";
            videoHeading = view.FindViewById<TextView>(Resource.Id.video_heading);
            videoHeading.Text = "Favourite Videos";
            audioRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.audio_recyclerview);
            videoRecyclerView = view.FindViewById<RecyclerView>(Resource.Id.video_recyclerview);
            SetUpAudioRecyclerView(audioRecyclerView);
            SetUpVideoRecyclerView(videoRecyclerView);
            return view;
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            HasOptionsMenu = true;
        }

        public override void OnStart()
        {
            base.OnStart();
            GetOfflineAudioList();
            GetOfflineVideoList();
        }

        private async void GetOfflineVideoList()
        {
            try
            {
                mVideos = await(new Initialize()).getVideoList();
                mVideoAdapter = new VideoAlbumRecyclerViewAdapter(Activity, mVideos, Activity.Resources, true);
                videoRecyclerView.SetAdapter(mVideoAdapter);

                videoRecyclerView.Visibility = ViewStates.Visible;
                videoProgressBar.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                videoProgressBar.Visibility = ViewStates.Gone;
                Snackbar.Make(view, "Connection Error", Snackbar.LengthIndefinite)
                 .SetAction("Retry", v =>
                 {
                     videoRecyclerView.Visibility = ViewStates.Gone;
                     videoProgressBar.Visibility = ViewStates.Visible;
                     GetOfflineVideoList();
                 }).Show();
            }
        }

        private async void GetOfflineAudioList()
        {
            try
            {
                mAudios = await(new Initialize()).getAudioList();
                mAudioAdapter = new AudioAlbumRecyclerViewAdapter(Activity, mAudios, Activity.Resources, true);
                audioRecyclerView.SetAdapter(mAudioAdapter);

                audioRecyclerView.Visibility = ViewStates.Visible;
                audioProgressBar.Visibility = ViewStates.Gone;
            }
            catch (Exception e)
            {
                audioProgressBar.Visibility = ViewStates.Gone;
                Snackbar.Make(view, "Connection Error", Snackbar.LengthIndefinite)
                 .SetAction("Retry", v =>
                 {
                     audioRecyclerView.Visibility = ViewStates.Gone;
                     audioProgressBar.Visibility = ViewStates.Visible;
                     GetOfflineAudioList();
                 }).Show();
            }
        }

        private void SetUpVideoRecyclerView(RecyclerView videoRecyclerView)
        {
            RecyclerView.LayoutManager vLayoutManager = new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
            videoRecyclerView.SetLayoutManager(vLayoutManager);
            //videoRecyclerView.AddItemDecoration(new GridSpacingItemDecoration(2, dpToPx(10), true));
            videoRecyclerView.SetItemAnimator(new DefaultItemAnimator());
            //SnapHelper snapHelper = new LinearSnapHelper();
            //snapHelper.AttachToRecyclerView(videoRecyclerView);

            videoRecyclerView.SetItemClickListener((rv, position, view) =>
            {
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;

            });
        }

        private void SetUpAudioRecyclerView(RecyclerView audioRecyclerView)
        {
            RecyclerView.LayoutManager aLayoutManager = new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
            audioRecyclerView.SetLayoutManager(aLayoutManager);
            //audioRecyclerView.AddItemDecoration(new GridSpacingItemDecoration(2, dpToPx(10), true));
            audioRecyclerView.SetItemAnimator(new DefaultItemAnimator());
            //SnapHelper snapHelper = new LinearSnapHelper();
            //snapHelper.AttachToRecyclerView(audioRecyclerView);

            audioRecyclerView.SetItemClickListener((rv, position, view) =>
            {
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;
                
            });
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var item = menu.FindItem(Resource.Id.action_search);
            var searchView = MenuItemCompat.GetActionView(item);
            mSearchView = searchView.JavaCast<Android.Support.V7.Widget.SearchView>();

            mSearchView.QueryTextChange += (s, e) =>
            {
                mAudioAdapter.Filter.InvokeFilter(e.NewText);
                mAudioAdapter.searchString = e.NewText;

                mVideoAdapter.Filter.InvokeFilter(e.NewText);
                mVideoAdapter.searchString = e.NewText;
            };

            mSearchView.QueryTextSubmit += (s, e) =>
            {
                // Handle enter/search button on keyboard here
                Toast.MakeText(Activity, "Searched for: " + e.Query, ToastLength.Short).Show();
                e.Handled = true;
            };

            MenuItemCompat.SetOnActionExpandListener(item, new SearchViewExpandListener2(mAudioAdapter, mVideoAdapter));
        }

        private int dpToPx(int dp)
        {
            int pixels = (int)((dp) * Resources.DisplayMetrics.Density + 0.5f);
            return pixels;
        }
    }

    public class SearchViewExpandListener2 : Java.Lang.Object, MenuItemCompat.IOnActionExpandListener
    {
        private readonly IFilterable _adapter1;
        private readonly IFilterable _adapter2;

        public SearchViewExpandListener2(IFilterable adapter1, IFilterable adapter2)
        {
            _adapter1 = adapter1;
            _adapter2 = adapter2;
        }

        public bool OnMenuItemActionCollapse(IMenuItem item)
        {
            _adapter1.Filter.InvokeFilter("");
            _adapter2.Filter.InvokeFilter("");
            return true;
        }

        public bool OnMenuItemActionExpand(IMenuItem item)
        {
            return true;
        }
    }
}