
using System;
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using DivineVerITies.Helpers;
using Android.Content;

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

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            View view = inflater.Inflate(Resource.Layout.OfflinePodcast, container, false) as View;
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

        private void GetOfflineVideoList()
        {
            
        }

        private void GetOfflineAudioList()
        {
            
        }

        private void SetUpVideoRecyclerView(RecyclerView videoRecyclerView)
        {
            RecyclerView.LayoutManager vLayoutManager = new GridLayoutManager(Activity, 2, LinearLayoutManager.Horizontal, false);
            videoRecyclerView.SetLayoutManager(vLayoutManager);
            videoRecyclerView.AddItemDecoration(new GridSpacingItemDecoration(2, dpToPx(10), true));
            videoRecyclerView.SetItemAnimator(new DefaultItemAnimator());
            SnapHelper snapHelper = new LinearSnapHelper();
            snapHelper.AttachToRecyclerView(videoRecyclerView);

            videoRecyclerView.SetItemClickListener((rv, position, view) =>
            {
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;

            });
        }

        private void SetUpAudioRecyclerView(RecyclerView audioRecyclerView)
        {
            RecyclerView.LayoutManager aLayoutManager = new GridLayoutManager(Activity, 2, LinearLayoutManager.Horizontal, false);
            audioRecyclerView.SetLayoutManager(aLayoutManager);
            audioRecyclerView.AddItemDecoration(new GridSpacingItemDecoration(2, dpToPx(10), true));
            audioRecyclerView.SetItemAnimator(new DefaultItemAnimator());
            SnapHelper snapHelper = new LinearSnapHelper();
            snapHelper.AttachToRecyclerView(audioRecyclerView);

            audioRecyclerView.SetItemClickListener((rv, position, view) =>
            {
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;
                
            });
        }

        private int dpToPx(int dp)
        {
            int pixels = (int)((dp) * Resources.DisplayMetrics.Density + 0.5f);
            return pixels;
        }
    }
}