using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using SupportFragment = Android.Support.V4.App.Fragment;

namespace DivineVerITies.Fragments
{
    public class Fragment2 : SupportFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
           
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View View = inflater.Inflate(Resource.Layout.Fragment2, container, false);

            LinearLayout mLinearLayout = View.FindViewById<LinearLayout>(Resource.Id.mainView);
            mLinearLayout.Click += mLinearLayout_Click;

            Button mButtonSignIn = View.FindViewById<Button>(Resource.Id.btnSignIn);
            mButtonSignIn.Click += mButtonSignIn_Click;

            return View;  
        }

        private void mLinearLayout_Click(object sender, EventArgs e)
        {
            InputMethodManager inputManager = (InputMethodManager)Activity.GetSystemService(Android.App.Activity.InputMethodService);
            inputManager.HideSoftInputFromWindow(Activity.CurrentFocus.WindowToken, HideSoftInputFlags.None);
        }

        private void mButtonSignIn_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Activity, typeof(SelectTopics));
            StartActivity(intent);
        }
    }
}