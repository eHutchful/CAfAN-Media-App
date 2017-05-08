using Android.Content;
using Android.OS;
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using DivineVerITies.Helpers;
using System;
using System.Collections.Generic;
using SupportFragment = Android.Support.V4.App.Fragment;

namespace DivineVerITies.Fragments
{
    public class Fragment6 : SupportFragment
    {
        private RecyclerView recyclerView;
        private AudioAlbumRecyclerAdapter mAudioAdapter;
        public static List<AudioList> mAudios=new List<AudioList>();
        //private View view;
        //private Android.Support.V7.Widget.SearchView mSearchView;
        //private Android.Support.V7.App.AlertDialog.Builder builder;
        public static Context context;

        //private SwipeRefreshLayout swipeRefreshLayout;
        private RecyclerView mRecyclerView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            context = this.Context;
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            
            mRecyclerView = inflater.Inflate(Resource.Layout.Fragment6, container, false) as RecyclerView;
            recyclerView = mRecyclerView.FindViewById<RecyclerView>(Resource.Id.recyclerview1);
            recyclerView.HasFixedSize = true;
            SetUpRecyclerView(recyclerView);
            return mRecyclerView;
        }

        private void SetUpRecyclerView(RecyclerView mRecyclerView)
        {
            recyclerView.SetLayoutManager(new LinearLayoutManager(recyclerView.Context)); 
            mAudioAdapter = new AudioAlbumRecyclerAdapter(recyclerView.Context, mAudios, Activity.Resources);
            recyclerView.SetAdapter(mAudioAdapter);
            recyclerView.SetItemClickListener((rv, position, view) =>
            {
                
                int itemposition = rv.GetChildAdapterPosition(view);
                Context context = view.Context;
                Intent intent = new Intent(context, typeof(PodcastDetails));
                if (mAudioAdapter.mAudios == null)
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

        
    }
}