using System;

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using SupportFragment = Android.Support.V4.App.Fragment;
using Android.Views.InputMethods;
using Android.Content;

namespace DivineVerITies.Fragments
{
    public class Fragment1 : SupportFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View View = inflater.Inflate(Resource.Layout.Fragment1, container, false);
     
            LinearLayout mLinearLayout = View.FindViewById<LinearLayout>(Resource.Id.mainView);
            mLinearLayout.Click += mLinearLayout_Click;

            Button mButtonSignUp = View.FindViewById<Button>(Resource.Id.btnSignUp);
            mButtonSignUp.Click += mButtonSignUp_Click;

            return View;
        }

        private void mLinearLayout_Click(object sender, EventArgs e)
        {
            InputMethodManager inputManager = (InputMethodManager)Activity.GetSystemService(Android.App.Activity.InputMethodService);
            inputManager.HideSoftInputFromWindow(Activity.CurrentFocus.WindowToken, HideSoftInputFlags.None);
        }

        private void mButtonSignUp_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(Activity, typeof(SelectTopics));
            StartActivity(intent);
        }
    }
}