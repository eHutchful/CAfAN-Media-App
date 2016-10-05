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

namespace DivineVerITies.Fragments
{
    public class Fragment3 : SupportFragment
    {
        private ProgressBar mProgresBar;
        private SwipeRefreshLayout swipeRefreshLayout;
        private RecyclerView recyclerView;
        private AudioRecyclerViewAdapter mAudioAdapter;
        public static List<AudioList> mAudios;
        public List<AudioList> mlist;
        private View view;
        private Android.Support.V7.Widget.SearchView mSearchView;
        private Android.Support.V7.App.AlertDialog.Builder builder;
        String[] sortitems = { "Title Ascending", "Title Descending", "Date Ascending", "Date Descending" };
        public static Context context;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            context = this.Context;
            // Create your fragment here
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);
            
            getAudiosAsyc();
            HasOptionsMenu = true;
            swipeRefreshLayout.Refresh += swipeRefreshLayout_Refresh;
        }

        private async void getAudiosAsyc()
        {
            try
            {
                mAudios = await (new Initialize()).getAudioList();
                mAudioAdapter = new AudioRecyclerViewAdapter(recyclerView.Context, mAudios, Activity.Resources);
                recyclerView.SetAdapter(mAudioAdapter);
                
                recyclerView.Visibility = ViewStates.Visible;
                mProgresBar.Visibility = ViewStates.Gone;
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
            recyclerView.HasFixedSize = true;
            var layoutManager = new LinearLayoutManager(Activity);
            var onScrollListener = new XamarinRecyclerViewOnScrollListener(layoutManager);
            onScrollListener.LoadMoreEvent += (object sender, EventArgs e) =>
            {
                mAudios.Add(mAudios[0]);
                recyclerView.GetAdapter().NotifyItemInserted(mAudios.Count - 1);
                mAudios.RemoveAt(0);
                recyclerView.GetAdapter().NotifyItemRemoved(0);
            };
            onScrollListener.LoadMoreReverseEvent += (object sender, EventArgs e) =>
            {
                mAudios.Insert(0, mAudios[mAudios.Count - 1]);                
                recyclerView.GetAdapter().NotifyItemInserted(0);
                mAudios.RemoveAt(mAudios.Count - 1);
                recyclerView.GetAdapter().NotifyItemRemoved(mAudios.Count - 1);
            };
            recyclerView.AddOnScrollListener(onScrollListener);

            recyclerView.SetLayoutManager(layoutManager);
            mProgresBar = view.FindViewById<ProgressBar>(Resource.Id.audio_player_loading);
            mProgresBar.Animate();
            swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            swipeRefreshLayout.SetColorSchemeColors(Color.Indigo, Color.Pink, Color.Blue, Color.Yellow);
            SetUpAudioRecyclerView(recyclerView);
            return view;
        }

        private void SetUpAudioRecyclerView(RecyclerView recyclerView)
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));

            recyclerView.SetItemClickListener( (rv, position, view) =>
            {
               
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;
                Intent intent = new Intent(context, typeof(PodcastDetails));
                if(mAudioAdapter.mAudios==null)
                {
                    MediaPlayerService.selectedAudio = mAudios[position];
                    MainApp.visibility = ViewStates.Visible;
                    
                }                
                else if (mAudioAdapter.mAudios.Count == mAudios.Count)
                {
                    MediaPlayerService.selectedAudio = mAudios[position];
                    MainApp.visibility = ViewStates.Visible;
                }                
                else
                {
                    MediaPlayerService.selectedAudio = mAudios[position];
                    MainApp.visibility = ViewStates.Visible;
                }
                
            });
        }
        
        public void SortAudios()
        {
            
            base.OnAttach(Activity);
            builder = new Android.Support.V7.App.AlertDialog.Builder(Activity);
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
                                    select audio).ToList<AudioList>();

                    mAudioAdapter = new AudioRecyclerViewAdapter(recyclerView.Context, mAudios, Activity.Resources);
                    recyclerView.SetAdapter(mAudioAdapter);
                    builder.Dispose();
                    break;

                case 1:
                    mAudios = (from audio in mAudios
                                    orderby audio.Title descending
                                    select audio).ToList<AudioList>();

                    mAudioAdapter = new AudioRecyclerViewAdapter(recyclerView.Context, mAudios, Activity.Resources);
                    recyclerView.SetAdapter(mAudioAdapter);
                    builder.Dispose();
                    break;

                case 2:
                    mAudios = (from audio in mAudios
                                    orderby audio.PubDate
                                    select audio).ToList<AudioList>();

                    mAudioAdapter = new AudioRecyclerViewAdapter(recyclerView.Context, mAudios, Activity.Resources);
                    recyclerView.SetAdapter(mAudioAdapter);
                    builder.Dispose();
                    break;

                case 3:
                    mAudios = (from audio in mAudios
                                    orderby audio.PubDate descending
                                    select audio).ToList<AudioList>();

                    mAudioAdapter = new AudioRecyclerViewAdapter(recyclerView.Context, mAudios, Activity.Resources);
                    recyclerView.SetAdapter(mAudioAdapter);
                    builder.Dispose();
                    break;
            }
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            var item = menu.FindItem(Resource.Id.action_search);
            var searchView = MenuItemCompat.GetActionView(item);
            mSearchView = searchView.JavaCast<Android.Support.V7.Widget.SearchView>();

            mSearchView.QueryTextChange += (s, e) => mAudioAdapter.Filter.InvokeFilter(e.NewText);

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