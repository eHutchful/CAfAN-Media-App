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
using Android.Text;
using Java.Lang;

namespace DivineVerITies.Fragments
{
    public class Fragment1 : SupportFragment, ITextWatcher
    {
        SupportEditText UserName;
        SupportEditText Email;
        SupportEditText Phone;
        SupportEditText Password;
        SupportEditText ConfirmPassword;
        private ValidationHelper validityChecker;

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            validityChecker = new ValidationHelper(Activity);
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
            Password.EditText.AddTextChangedListener(this);
            ConfirmPassword = View.FindViewById<SupportEditText>(Resource.Id.txtInputLayoutConfirmPassword);
            ConfirmPassword.EditText.AddTextChangedListener(this);
            Phone = View.FindViewById<SupportEditText>(Resource.Id.txtInputLayoutPhone);

            Button mButtonSignUp = View.FindViewById<Button>(Resource.Id.btnSignUp);
            mButtonSignUp.Click += mButtonSignUp_Click;

            return View;
        }

        private void mLinearLayout_Click(object sender, EventArgs e)
        {
            InputMethodManager inputManager = (InputMethodManager)Activity.GetSystemService(Context.InputMethodService);
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

            if (!validityChecker.isEditTextFilled(UserName, "Username is required!")
                || !validityChecker.isEditTextFilled(Password, "Password is required!")
                || !validityChecker.isEditTextFilled(Email, "Email address is required!")
                || !validityChecker.isEditTextFilled(Phone, "Phone Number is required!")
                || !validityChecker.isEditTextFilled(ConfirmPassword, "Password Confirmation is required!"))
            {
                return;
            }

            if (!validityChecker.isEditTextEmail(Email, "Email address is invalid!"))
            {
                return;
            }

            if (!validityChecker.validatePassword(Password, txtPassword, txtUserName))
            {
                return;
            }

            if (txtPassword != txtCPassword)
            {
                ConfirmPassword.Error = "Passwords do not match!";
                return;
            }
            if (new NetworkStatusMonitor().State == NetworkState.Disconnected)
            {
                CreateAndShowDialog("Your device is currently not connected to the internet. Please check your data or WiFi connection and try again.", "Network Connectivity Error");
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
            catch(System.Exception ex)
            {
                CreateAndShowDialog(ex.Message, "Error");
            }
           
        }

         private void CreateAndShowDialog(System.Exception exception, string title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        private void CreateAndShowDialog(string message, string title)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(Activity);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.SetPositiveButton("OKAY", delegate { builder.Dispose(); });
            builder.Create().Show();
        }

        public void AfterTextChanged(IEditable s)
        {
            if (Password.HasFocus)
            {
                validityChecker.validatePassword(Password, Password.EditText.Text, UserName.EditText.Text);
            }
            else if (ConfirmPassword.HasFocus)
            {
                if (Password.EditText.Text != ConfirmPassword.EditText.Text)
                {
                    ConfirmPassword.Error = "Passwords do not match.";
                }
                else
                {
                    ConfirmPassword.ErrorEnabled = false;
                }
            }
        }

        public void BeforeTextChanged(ICharSequence s, int start, int count, int after)
        {
            
        }

        public void OnTextChanged(ICharSequence s, int start, int before, int count)
        {
            
        }
    }
}