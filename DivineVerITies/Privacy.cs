
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Webkit;
using Android.Support.V7.Widget;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Views;

namespace DivineVerITies
{
    [Activity(Theme = "@style/Theme.DesignDemo")]
    public class Privacy : AppCompatActivity
    {
        private SupportToolbar toolBar;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Privacy);
            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);
            SupportActionBar.Title = "Privacy Policy";
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            WebView view = new WebView(this);
            
            view.VerticalScrollBarEnabled = false;

            (FindViewById<CardView>(Resource.Id.privacyCard)).AddView(view);

            view.LoadData(GetString(Resource.String.policy_info), "text/html; charset=utf-8", "utf-8");
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    Finish();
                    return true;

                default:
                    return base.OnOptionsItemSelected(item);
            }
        }
    }
}