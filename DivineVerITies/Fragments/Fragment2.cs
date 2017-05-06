using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using System;
using SupportFragment = Android.Support.V4.App.Fragment;
using SupportEditText = Android.Support.Design.Widget.TextInputLayout;
using Microsoft.WindowsAzure.MobileServices;
using DivineVerITies.Helpers;
using Android.Support.Design.Widget;

namespace DivineVerITies.Fragments
{
    public class Fragment2 : SupportFragment
    {
        SupportEditText mtxtEmail;
        SupportEditText mtxtPassword;
        TextView mtxtFgtPassword;
        CheckBox mCbxRemmeberMe;
        public static Context SignInContext;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SignInContext = this.Context;
            // Create your fragment here
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View View = inflater.Inflate(Resource.Layout.Fragment2, container, false);

            LinearLayout mLinearLayout = View.FindViewById<LinearLayout>(Resource.Id.mainView);
            mLinearLayout.Click += mLinearLayout_Click;

            mtxtEmail = View.FindViewById<SupportEditText>(Resource.Id.txtInputLayoutEmail);

            mtxtPassword = View.FindViewById<SupportEditText>(Resource.Id.txtInputLayoutPassword);

            mtxtFgtPassword = View.FindViewById<TextView>(Resource.Id.txtFgtPassword);
            mtxtFgtPassword.Click += mtxtFgtPassword_Click;
            mCbxRemmeberMe = View.FindViewById<CheckBox>(Resource.Id.chkBoxRemmemberMe);
            mCbxRemmeberMe.Click += mCbxRemmeberMe_Click;

            Button mButtonSignIn = View.FindViewById<Button>(Resource.Id.btnSignIn);
            mButtonSignIn.Click += mButtonSignIn_Click;

            ImageView mFacebook = View.FindViewById<ImageView>(Resource.Id.imgFacebook);
            mFacebook.Click += mFacebookSignIn_Click;

            ImageView mTwitterSignIn = View.FindViewById<ImageView>(Resource.Id.imgTwitter);
            mTwitterSignIn.Click += mTwitterSignIn_Click;

            ImageView mGoogleSignIn = View.FindViewById<ImageView>(Resource.Id.imgGoogle);
            mGoogleSignIn.Click += mGoogleSignIn_Click;

            ImageView mMicrosoftSignIn = View.FindViewById<ImageView>(Resource.Id.imgMicrosoft);
            mMicrosoftSignIn.Click += mMicrosoftSignIn_Click;

            return View;  
        }

        void mtxtFgtPassword_Click(object sender, EventArgs e)
        {
            
        }

        void mCbxRemmeberMe_Click(object sender, EventArgs e)
        {
            ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            ISharedPreferencesEditor edit = pref.Edit();

            if (mCbxRemmeberMe.Checked)
            {
                edit.PutBoolean("RememberMe", true);
            }
            else
            {
                edit.PutBoolean("RememberMe", false);
            }

            edit.Apply();
        }

        async void mMicrosoftSignIn_Click(object sender, EventArgs e)
        {
            if (await AzureService.DefaultService.MicrosoftAuthenticate())
            {
                Intent intent = new Intent(Activity, typeof(SelectTopics));
                StartActivity(intent);
                Activity.Finish();
            }
        }

        async void mGoogleSignIn_Click(object sender, EventArgs e)
        {
            if (await AzureService.DefaultService.GoogleAuthenticate())
            {
                Intent intent = new Intent(Activity, typeof(SelectTopics));
                StartActivity(intent);
                Activity.Finish();
            }
        }

        async void mTwitterSignIn_Click(object sender, EventArgs e)
        {
            if (await AzureService.DefaultService.TwitterAuthenticate())
            {
                Intent intent = new Intent(Activity, typeof(SelectTopics));
                StartActivity(intent);
                Activity.Finish();
            }
        }

        async void mFacebookSignIn_Click(object sender, EventArgs e)
        {
            if (await AzureService.DefaultService.FacebookAuthenticate())
            {
                Intent intent = new Intent(Activity, typeof(SelectTopics));
                StartActivity(intent);
                Activity.Finish();
            }
        }

        public override void OnActivityCreated(Bundle savedInstanceState)
        {
            if (!string.IsNullOrWhiteSpace(mtxtPassword.EditText.Text) && mtxtPassword.EditText.Text.Length < 6)
            { mtxtPassword.Error = "Password must be at least 6 characters!"; }
            if (mtxtEmail.EditText.Text.Length != 0 && mtxtPassword.EditText.Text == mtxtEmail.EditText.Text)
            { mtxtPassword.Error = "Password must be different from Username"; }

            base.OnActivityCreated(savedInstanceState);
        }

        private void mLinearLayout_Click(object sender, EventArgs e)
        {
            InputMethodManager inputManager = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
            inputManager.HideSoftInputFromWindow(Activity.CurrentFocus.WindowToken, HideSoftInputFlags.None);
        }

        private async void mButtonSignIn_Click(object sender, EventArgs e)
        {
            var anchor = sender as View;
            string txtPassword = mtxtPassword.EditText.Text;
            string txtUserName = mtxtEmail.EditText.Text;

            if (mtxtEmail.EditText.Text.Length == 0)
            { 
                mtxtEmail.Error = "Username or Email is required!";
                return;
            }
            if (mtxtPassword.EditText.Text.Length == 0)
            {
                mtxtPassword.Error = "Password is required!";
                return;
            }
            if (new NetworkStatusMonitor().State == NetworkState.Disconnected)
            {
                CreateAndShowDialog("Your device is currently not connected to the internet. Please checkyour data or WiFi connection and try again.", "Network Connectivity Error");
                return;
            }
            
            try
            {
                Snackbar.Make(anchor, "Authenticating...Please Wait!", Snackbar.LengthIndefinite).Show();
                await AzureService.DefaultService.Authenticate(txtUserName, txtPassword);

                Intent intent = new Intent(Activity, typeof(SelectTopics));
                StartActivity(intent);
                Activity.Finish();
            }
            catch (MobileServiceInvalidOperationException exception)
            {
                
                if (string.Equals(exception.Message, "invalid_grant"))
                    CreateAndShowDialog("Wrong Username or Password", "Invalid Credentials");

                mtxtEmail.EditText.Text = "";
                mtxtPassword.EditText.Text = "";
                mtxtPassword.Error = "Wrong Username or Password. Please try again.";
            }
        }
        private void CreateAndShowDialog(Exception exception, string title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        private void CreateAndShowDialog(string message, string title)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(Activity);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.SetPositiveButton("Okay", delegate { builder.Dispose(); });
            builder.Create().Show();
        }
    }
}