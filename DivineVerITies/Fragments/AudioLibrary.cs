using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using DivineVerITies.Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using SupportFragment = Android.Support.V4.App.Fragment;
using Android.Runtime;
using System.Linq;
using Android.Support.V7.App;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;

namespace DivineVerITies.Fragments
{
    public class AudioLibrary : SupportFragment
    {
        private ProgressBar mProgresBar;
        private SwipeRefreshLayout swipeRefreshLayout;
        private RecyclerView recyclerView;
        private AudioAlbumRecyclerViewAdapter mAudioAdapter;
        public static List<AudioList> mAudios;
        public List<AudioList> mlist;
        private View view;
        private Android.Support.V7.Widget.SearchView mSearchView;
        private Android.Support.V7.App.AlertDialog.Builder builder;
        string[] sortitems = { "Title Ascending", "Title Descending", "Date Ascending", "Date Descending" };
        public static TextView chosenView;
        TextView mPlayedText;
        private RecyclerView bottomSheetRecyclerView;
        private ImageView backdrop;
        CoordinatorLayout sheet;
        BottomSheetBehavior bottomSheetBehavior;
        ProgressBar backdropProgress;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here

            sheet = ((AppCompatActivity)Activity).FindViewById<CoordinatorLayout>(Resource.Id.bottom_sheet);
            bottomSheetBehavior = BottomSheetBehavior.From(sheet);

            bottomSheetRecyclerView =((AppCompatActivity)Activity).FindViewById<RecyclerView>(Resource.Id.recyclerview2);
            bottomSheetRecyclerView.SetLayoutManager(new LinearLayoutManager(Activity));

            backdrop = ((AppCompatActivity)Activity).FindViewById<ImageView>(Resource.Id.backdrop);
            backdropProgress = ((AppCompatActivity)Activity).FindViewById<ProgressBar>(Resource.Id.image_loading);
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
                       
            HasOptionsMenu = true;
            swipeRefreshLayout.Refresh += swipeRefreshLayout_Refresh;
        }

        public override void OnStart()
        {
            base.OnStart();
            getAudiosAsyc();
        }

        

        private async void getAudiosAsyc()
        {
            try
            {
                mAudios = await (new Initialize()).getAudioList();
                mAudioAdapter = new AudioAlbumRecyclerViewAdapter(recyclerView.Context, mAudios, Activity.Resources, false);
                recyclerView.SetAdapter(mAudioAdapter);
                
                recyclerView.Visibility = ViewStates.Visible;
                mProgresBar.Visibility = ViewStates.Gone;

                bottomSheetRecyclerView.SetAdapter(new AudioRecyclerViewAdapter(Activity, mAudios, Activity.Resources));
                bottomSheetRecyclerView.Visibility = ViewStates.Visible;
            }
            catch (Exception e)
            {
                mProgresBar.Visibility = ViewStates.Gone;
                Snackbar.Make(view, "Connection Error", Snackbar.LengthIndefinite)
                 .SetAction("Retry", v =>
                 {
                     recyclerView.Visibility = ViewStates.Gone;
                     mProgresBar.Visibility = ViewStates.Visible;
                     getAudiosAsyc();
                 }).Show();
            }
        }

        private void swipeRefreshLayout_Refresh(object sender, EventArgs e)
        {
            getAudiosAsyc();
            swipeRefreshLayout.Refreshing = false;
        }


        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);
            view = inflater.Inflate(Resource.Layout.Fragment3, container, false) as View;
            recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerview);
            //recyclerView.HasFixedSize = true;
            //var layoutManager = new LinearLayoutManager(Activity);

            //var onScrollListener = new XamarinRecyclerViewOnScrollListener(layoutManager);
            //onScrollListener.LoadMoreEvent += (object sender, EventArgs e) =>
            //{
            //    mAudios.Add(mAudios[0]);
            //    recyclerView.GetAdapter().NotifyItemInserted(mAudios.Count - 1);
            //    mAudios.RemoveAt(0);
            //    recyclerView.GetAdapter().NotifyItemRemoved(0);
            //};
            //onScrollListener.LoadMoreReverseEvent += (object sender, EventArgs e) =>
            //{
            //    mAudios.Insert(0, mAudios[mAudios.Count - 1]);                
            //    recyclerView.GetAdapter().NotifyItemInserted(0);
            //    mAudios.RemoveAt(mAudios.Count - 1);
            //    recyclerView.GetAdapter().NotifyItemRemoved(mAudios.Count - 1);
            //};
            //recyclerView.AddOnScrollListener(onScrollListener);

            //recyclerView.SetLayoutManager(layoutManager);
            mProgresBar = view.FindViewById<ProgressBar>(Resource.Id.audio_player_loading);
            mProgresBar.Animate();
            swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            swipeRefreshLayout.SetColorSchemeColors(Color.Indigo, Color.Pink, Color.Blue, Color.Yellow);
            mPlayedText = view.FindViewById<TextView>(Resource.Id.txtPlayed);
            SetUpAudioRecyclerView(recyclerView);
            return view;
        }

        private int dpToPx(int dp)
        {
            int pixels = (int)((dp) * Resources.DisplayMetrics.Density + 0.5f);
            return pixels;
        }

        private void SetUpBottomSheet()
        {            
            bottomSheetBehavior.State = BottomSheetBehavior.StateExpanded;
            try
            {
                var audio = (AudioList)MediaPlayerService.selectedAudio;
                Glide.With(Activity)
                .Load(audio.ImageUrl)
                .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                .Error(Resource.Drawable.ChurchLogo_Gray)
                .SkipMemoryCache(true)
                .DiskCacheStrategy(DiskCacheStrategy.All)
                .Into(backdrop);
            }catch(InvalidCastException inv)
            {
                var audio = (Media)MediaPlayerService.selectedAudio;
                Glide.With(Activity)
                .Load(audio.AlbumArt)
                .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                .Error(Resource.Drawable.ChurchLogo_Gray)
                .SkipMemoryCache(true)
                .DiskCacheStrategy(DiskCacheStrategy.All)
                .Into(backdrop);
            }
            backdropProgress.Visibility = ViewStates.Gone;
        }

        private void SetUpAudioRecyclerView(RecyclerView recyclerView)
        {
            //recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));

            RecyclerView.LayoutManager layoutManager = new GridLayoutManager(Activity, 2);
            recyclerView.SetLayoutManager(layoutManager);
            recyclerView.AddItemDecoration(new GridSpacingItemDecoration(2, dpToPx(10), true));

            recyclerView.SetItemClickListener( (rv, position, view) =>
            {
                var text = view.FindViewById<TextView>(Resource.Id.txtPlayed);
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;
                Intent intent = new Intent(context, typeof(PodcastDetails));
                
                if (mAudioAdapter.mAudios==null)
                {
                    MediaPlayerService.selectedAudio = mAudios[position];
                    SetUpBottomSheet();
                    MainApp.visibility = ViewStates.Visible;
                    
                }                
                else if (mAudioAdapter.mAudios.Count == mAudios.Count)
                {
                    MediaPlayerService.selectedAudio = mAudios[position];
                    SetUpBottomSheet();
                    MainApp.visibility = ViewStates.Visible;
                }                
                else
                {
                    MediaPlayerService.selectedAudio = mAudios[position];
                    SetUpBottomSheet();
                    MainApp.visibility = ViewStates.Visible;
                }
                chosenView = text;
                //text.Visibility = ViewStates.Visible;
            });
        }
        
        public void SortAudios()
        { 
            builder = new Android.Support.V7.App.AlertDialog.Builder(Context);
            builder.SetTitle("Sort Order")
                   .SetSingleChoiceItems(sortitems, -1, SortTypeListClicked)
                   .SetNegativeButton("Cancel", delegate { builder.Dispose(); });
            builder.Create().Show();
        }

        private void SortTypeListClicked(object sender, DialogClickEventArgs args)
        {
            switch (args.Which)
            {
                case 0:
                    mAudios = (from audio in mAudios
                                    orderby audio.Title
                                    select audio).ToList();
                    break;

                case 1:
                    mAudios = (from audio in mAudios
                                    orderby audio.Title descending
                                    select audio).ToList();
                    break;

                case 2:
                    mAudios = (from audio in mAudios
                                    orderby audio.PubDate
                                    select audio).ToList();
                    break;

                case 3:
                    mAudios = (from audio in mAudios
                                    orderby audio.PubDate descending
                                    select audio).ToList();
                    break;
            }

            mAudioAdapter = new AudioAlbumRecyclerViewAdapter(recyclerView.Context, mAudios, Activity.Resources, false);
            recyclerView.SetAdapter(mAudioAdapter);
            builder.Dispose();
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
            };

            mSearchView.QueryTextSubmit += (s, e) =>
            {
                // Handle enter/search button on keyboard here
                Toast.MakeText(Activity, "Searched for: " + e.Query, ToastLength.Short).Show();
                e.Handled = true;
            };

            MenuItemCompat.SetOnActionExpandListener(item, new SearchViewExpandListener(mAudioAdapter));
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
             switch (item.ItemId)
            {
                 case Resource.Id.action_sort:
                    SortAudios();
                    return true;
            }

             return base.OnOptionsItemSelected(item);
        }
    }

    public class SearchViewExpandListener : Java.Lang.Object, MenuItemCompat.IOnActionExpandListener
    {
        private readonly IFilterable _adapter;

        public SearchViewExpandListener(IFilterable adapter)
        {
            _adapter = adapter;
        }

        public bool OnMenuItemActionCollapse(IMenuItem item)
        {
            _adapter.Filter.InvokeFilter("");
            return true;
        }

        public bool OnMenuItemActionExpand(IMenuItem item)
        {
            return true;
        }
    }

}