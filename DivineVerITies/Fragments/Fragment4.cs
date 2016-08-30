
using Android.OS;
using Android.Support.V4.Widget;
using Android.Support.V7.Widget;
using Android.Views;
using SupportFragment = Android.Support.V4.App.Fragment;

namespace DivineVerITies.Fragments
{
    public class Fragment4 : SupportFragment
    {
        //private SwipeRefreshLayout swipeRefreshLayout;
        private RecyclerView recyclerView;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            // Use this to return your custom view for this Fragment
            // return inflater.Inflate(Resource.Layout.YourFragment, container, false);

            recyclerView = inflater.Inflate(Resource.Layout.Fragment4, container, false) as RecyclerView;
            return recyclerView;
        }
    }
}