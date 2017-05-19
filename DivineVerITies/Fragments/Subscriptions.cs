
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using DivineVerITies.Helpers;
using SupportFragment = Android.Support.V4.App.Fragment;
using System;
using Android.Support.Design.Widget;
using System.Collections.Generic;
using Android.Support.V4.View;
using Android.Runtime;
using Com.Sothree.Slidinguppanel;
using Android.Support.V4.Media.Session;
using Newtonsoft.Json;

namespace DivineVerITies.Fragments
{
    public class Subscriptions : SupportFragment
    {
        private RecyclerView audioRecyclerView;
        private RecyclerView videoRecyclerView;
        private ProgressBar audioProgressBar;
        private ProgressBar videoProgressBar;
        private TextView audioHeading;
        private TextView videoHeading;
        private AudioRecyclerViewAdapter mAudioAdapter;
        private VideoAlbumRecyclerViewAdapter mVideoAdapter;
        private List<AudioList> mAudios;
        private List<Video> mVideos;
        View view;
        private Android.Support.V7.Widget.SearchView mSearchView;
        private CardView emptyAudioCard;
        private CardView emptyVideoCard;
        private TextView emptyAudioText;
        private TextView emptyVideoText;

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
            emptyAudioCard = view.FindViewById<CardView>(Resource.Id.emptyAudioCard);
            emptyVideoCard = view.FindViewById<CardView>(Resource.Id.emptyVideoCard);
            emptyAudioText = view.FindViewById<TextView>(Resource.Id.txtEmptyAudio);
            emptyVideoText = view.FindViewById<TextView>(Resource.Id.txtEmptyVideo);
            audioHeading = view.FindViewById<TextView>(Resource.Id.audio_heading);
            audioHeading.Text = "Recomended Audios";
            videoHeading = view.FindViewById<TextView>(Resource.Id.video_heading);
            videoHeading.Text = "Recomended Videos";
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
            GetFavouriteAudioList();
            GetFavouriteVideoList();
        }

        private async void GetFavouriteVideoList()
        {
            try
            {
                mVideos = await(new Initialize()).getVideoList();
                if (mVideos.Count > 0)
                {
                    var favourties = Initialize.getFavouritesList(mVideos, Activity);
                    mVideoAdapter = new VideoAlbumRecyclerViewAdapter(Activity, favourties, Activity.Resources, true);
                    videoRecyclerView.SetAdapter(mVideoAdapter);

                    videoRecyclerView.Visibility = ViewStates.Visible;
                    videoProgressBar.Visibility = ViewStates.Gone;
                }
                else
                {
                    emptyVideoText.Text = "No Videos Have Been Subscribed To Yet.";
                    emptyVideoCard.Visibility = ViewStates.Visible;
                    videoProgressBar.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                videoProgressBar.Visibility = ViewStates.Gone;
                Snackbar.Make(view, "Connection Error", Snackbar.LengthIndefinite)
                 .SetAction("Retry", v =>
                 {
                     videoRecyclerView.Visibility = ViewStates.Gone;
                     videoProgressBar.Visibility = ViewStates.Visible;
                     GetFavouriteVideoList();
                 }).Show();
            }
        }

        private async void GetFavouriteAudioList()
        {
            try
            {

                mAudios = await (new Initialize()).getAudioList();
                if (mAudios.Count > 0)
                {
                    var favourties = Initialize.getFavouritesList(mAudios, Activity);
                    mAudioAdapter = new AudioRecyclerViewAdapter(Activity, favourties, Activity.Resources);
                    audioRecyclerView.SetAdapter(mAudioAdapter);

                    audioRecyclerView.Visibility = ViewStates.Visible;
                    audioProgressBar.Visibility = ViewStates.Gone;
                }
                else
                {
                    emptyAudioText.Text = "No Audios Have Been Subscribed To Yet.";
                    emptyAudioCard.Visibility = ViewStates.Visible;
                    audioProgressBar.Visibility = ViewStates.Gone;
                }
            }
            catch (Exception e)
            {
                audioProgressBar.Visibility = ViewStates.Gone;
                Snackbar.Make(view, "Connection Error", Snackbar.LengthIndefinite)
                 .SetAction("Retry", v =>
                 {
                     audioRecyclerView.Visibility = ViewStates.Gone;
                     audioProgressBar.Visibility = ViewStates.Visible;
                     GetFavouriteAudioList();
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
                Intent intent = new Intent(context, typeof(VideoCastDetails));
                string serial = "";
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

        private void SetUpAudioRecyclerView(RecyclerView audioRecyclerView)
        {
            RecyclerView.LayoutManager aLayoutManager = new LinearLayoutManager(Activity, LinearLayoutManager.Horizontal, false);
            audioRecyclerView.SetLayoutManager(aLayoutManager);
            //audioRecyclerView.AddItemDecoration(new GridSpacingItemDecoration(2, dpToPx(10), true));
            audioRecyclerView.SetItemAnimator(new DefaultItemAnimator());
            //SnapHelper snapHelper = new LinearSnapHelper();
            //snapHelper.AttachToRecyclerView(audioRecyclerView);

            audioRecyclerView.SetItemClickListener(async (rv, position, view) =>
            {
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;
                MediaPlayerService.selectedAudio = ((SingleAudioAdapter)rv.GetAdapter()).mAudios[position];
                MainApp.visibility = ViewStates.Visible;
                if (MainApp.mLayout.GetPanelState() != SlidingUpPanelLayout.PanelState.Expanded)
                {
                    MainApp.mLayout.SetPanelState(SlidingUpPanelLayout.PanelState.Expanded);
                }
                var activity = ((MainApp)Activity);
                if (activity.binder.GetMediaPlayerService().mediaPlayer != null && activity.binder.GetMediaPlayerService().MediaPlayerState == PlaybackStateCompat.StatePlaying)
                {
                    await activity.binder.GetMediaPlayerService().Stop();

                    await activity.binder.GetMediaPlayerService().Play();
                }
                else
                {

                    await activity.binder.GetMediaPlayerService().Play();
                }

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
}