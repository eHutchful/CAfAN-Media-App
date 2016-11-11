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
using Android.Support.Design.Widget;
using Newtonsoft.Json.Linq;
using DivineVerITies.Helpers;

namespace DivineVerITies.Fragments
{
    public class Fragment1 : SupportFragment
    {
        SupportEditText UserName;
        SupportEditText Email;
        SupportEditText Phone;
        SupportEditText Password;
        SupportEditText ConfirmPassword;

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

            UserName = View.FindViewById<SupportEditText>(Resource.Id.txtInputLayoutUserName);
            Email = View.FindViewById<SupportEditText>(Resource.Id.txtInputLayoutEmail);
            Password = View.FindViewById<SupportEditText>(Resource.Id.txtInputLayoutPassword);
            ConfirmPassword = View.FindViewById<SupportEditText>(Resource.Id.txtInputLayoutConfirmPassword);
            Phone = View.FindViewById<SupportEditText>(Resource.Id.txtInputLayoutPhone);

            Button mButtonSignUp = View.FindViewById<Button>(Resource.Id.btnSignUp);
            mButtonSignUp.Click += mButtonSignUp_Click;

            return View;
        }

        private void mLinearLayout_Click(object sender, EventArgs e)
        {
            InputMethodManager inputManager = (InputMethodManager)Activity.GetSystemService(Android.App.Activity.InputMethodService);
            inputManager.HideSoftInputFromWindow(Activity.CurrentFocus.WindowToken, HideSoftInputFlags.None);
        }

        private async void mButtonSignUp_Click(object sender, EventArgs e)
        {
            var anchor = sender as View;
            string txtPassword = Password.EditText.Text;
            string txtUserName = UserName.EditText.Text;
            string txtEmail = Email.EditText.Text;
            string txtCPassword = ConfirmPassword.EditText.Text;
            string txtPhone = Phone.EditText.Text;

            if (txtUserName.Length == 0)
            {
                UserName.Error = "Username is required!";
                return;
            }
            if (txtPassword.Length == 0)
            {
                Password.Error = "Password is required!";
                return;
            }

            if (txtCPassword.Length == 0)
            {
                ConfirmPassword.Error = "Password Confirmation is required!";
                return;
            }
            if (txtPhone.Length == 0)
            {
                Phone.Error = "Phone Number is required!";
                return;
            }

            if (txtPassword != txtCPassword)
            {
                Phone.Error = "Passwords do not match!";
                return;
            }

            try
            {
                var newuser = new JObject();
                newuser.Add("username", txtUserName);
                newuser.Add("password", txtPassword);
                newuser.Add("confirmpassword", txtCPassword);
                newuser.Add("email", txtEmail);
                newuser.Add("phone", txtPhone);

                Snackbar.Make(anchor, "Setting Up Account...Please Wait!", Snackbar.LengthIndefinite).Show();

                await AzureService.DefaultService.client.InvokeApiAsync("/api/accounts/create", newuser);

                Intent intent = new Intent(Activity, typeof(SelectTopics));
                StartActivity(intent);
                Activity.Finish();
            }
            catch (MobileServiceInvalidOperationException exception)
            {
                Toast.MakeText(this.Context, exception.Message, ToastLength.Long).Show();
                //if (string.Equals(exception.Message, "invalid_grant"))
                    CreateAndShowDialog("Error", "Error Creating User. Please Try Again.");                
            }
           
        }

         private void CreateAndShowDialog(Exception exception, String title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        private void CreateAndShowDialog(string message, string title)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this.Context);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }
    }
}