using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Graphics;
using Android.Support.Design.Widget;

namespace DivineVerITies.Fragments
{
    public class Fragment6 : SupportFragment
    {
        private SwipeRefreshLayout swipeRefreshLayout;
        private RecyclerView mRecyclerView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            mRecyclerView = inflater.Inflate(Resource.Layout.Fragment6, container, false) as RecyclerView;

            //swipeRefreshLayout = mRecyclerView.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_layout);
            //swipeRefreshLayout.SetColorSchemeColors(Color.Indigo, Color.Pink, Color.Blue, Color.Yellow);
            //swipeRefreshLayout.Refresh += swipeRefreshLayout_Refresh;

            return mRecyclerView;
        }

        private void SetUpRecyclerView(RecyclerView mRecyclerView)
        {
            throw new NotImplementedException();
        }

        void swipeRefreshLayout_Refresh(object sender, EventArgs e)
        {
            View anchor = sender as View;
            Snackbar.Make(anchor, "No Connection", Snackbar.LengthLong)
                        .SetAction("Retry", v =>
                        {
                            //Do something here
                            //Intent intent = new Intent(fab.Context, typeof(BottomSheetActivity));
                            //StartActivity(intent);
                        })
                        .Show();
        }
    }
}