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

namespace DivineVerITies.Fragments
{
    public class Fragment3 : SupportFragment
    {
        private ProgressBar mProgresBar;
        private SwipeRefreshLayout swipeRefreshLayout;
        private RecyclerView recyclerView;
        private AudioRecyclerViewAdapter mAudioAdapter;
        public List<AudioList> mAudios;
        public List<AudioList> mlist;
        private View view;
        private Android.Support.V7.Widget.SearchView mSearchView;
        //private XDocument xdoc;
 
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
            
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            base.OnActivityCreated(savedInstanceState);

            getAudiosAsyc();

            HasOptionsMenu = true;

            swipeRefreshLayout.Refresh += swipeRefreshLayout_Refresh;
            //mAudios = await (new Initialize()).getAudioList();
            
        }

        private async void getAudiosAsyc()
        {
            try
            {
                //var response = await httpClient.GetStringAsync(feed);
                //xdoc = XDocument.Parse(response);
                //GetAudios();
                mAudios = await(new Initialize()).getAudioList();
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
                     //var response = await httpClient.GetStringAsync(feed);
                     //xdoc = XDocument.Parse(response);
                     //GetAudios();
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
            mProgresBar = view.FindViewById<ProgressBar>(Resource.Id.audio_player_loading);
            swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            swipeRefreshLayout.SetColorSchemeColors(Color.Indigo, Color.Pink, Color.Blue, Color.Yellow);            
            SetUpAudioRecyclerView(recyclerView);
            return view;
        }

        private void SetUpAudioRecyclerView(RecyclerView recyclerView)
        {
            
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context));
           // mAudioAdapter = new AudioRecyclerViewAdapter(recyclerView.Context, null, Activity.Resources);
            //recyclerView.SetAdapter(mAudioAdapter);
            

            recyclerView.SetItemClickListener((rv, position, view) =>
            {
                int itemposition = rv.GetChildAdapterPosition(view);
                //An item has been clicked
                Context context = view.Context;
                Intent intent = new Intent(context, typeof(PodcastDetails));
                var serial= JsonConvert.SerializeObject(mAudios[position]);
                intent.PutExtra("selectedItem", serial );

                context.StartActivity(intent);
            });
        }

        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            //base.OnCreateOptionsMenu(menu, inflater);
           //inflater.Inflate(Resource.Menu.sample_actions, menu);

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

            mAudioAdapter = new AudioRecyclerViewAdapter(recyclerView.Context, mAudios, Activity.Resources);
            MenuItemCompat.SetOnActionExpandListener(item, new SearchViewExpandListener(mAudioAdapter));

            
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