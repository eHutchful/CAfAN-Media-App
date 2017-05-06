using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V7.App;
using Android.Text;
using Android.Content.PM;
using Android.Support.Design.Widget;
using DivineVerITies.Helpers;
using Newtonsoft.Json.Linq;
using Java.Lang;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;

namespace DivineVerITies
{
    [Activity(Theme = "@style/Theme.DesignDemo", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, NoHistory = true)]
    public class ChangePassword : AppCompatActivity, ITextWatcher
    {
        private SupportToolbar toolBar;
        private TextInputLayout oldPassword;
        private TextInputLayout newPassword;
        private TextInputLayout confrimPassword;
        private FloatingActionButton fab;
        private ValidationHelper validityChecker;
        private string username;
        private NetworkStatusMonitor networkStatusMonitor;
        ISharedPreferences pref;
        ISharedPreferencesEditor edit;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.ChangePassword);
            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);
            SupportActionBar.Title = "Change Password";
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            oldPassword = FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutOldPassword);
            newPassword = FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutNewPassword);
            confrimPassword = FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutConfirmPassword);
            fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            validityChecker = new ValidationHelper(this);
            networkStatusMonitor = new NetworkStatusMonitor();

            newPassword.EditText.AddTextChangedListener(this);
            confrimPassword.EditText.AddTextChangedListener(this);


            pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            edit = pref.Edit();
            
            username = pref.GetString("Username", string.Empty);

            fab.Click += async (o, e) =>
            {
                View anchor = o as View;

                if (string.IsNullOrWhiteSpace(oldPassword.EditText.Text)
                || string.IsNullOrWhiteSpace(newPassword.EditText.Text)
                || string.IsNullOrWhiteSpace(confrimPassword.EditText.Text))
                {
                    return;
                }

                var success1 = validityChecker.validatePassword(newPassword, newPassword.EditText.Text, username);
                var success2 = newPassword.EditText.Text == confrimPassword.EditText.Text;

                if (success1 && success2)
                {
                    if (networkStatusMonitor.State == NetworkState.ConnectedData || networkStatusMonitor.State == NetworkState.ConnectedWifi)
                    {
                        var password = confrimPassword.EditText.Text.Trim();
                        try
                        {
                            JObject model = new JObject();
                            model.Add("oldpassword", oldPassword.EditText.Text);
                            model.Add("newpassword", newPassword.EditText.Text);
                            model.Add("confirmpassword", confrimPassword.EditText.Text);

                            Toast.MakeText(this, "Please wait...", ToastLength.Long).Show();
                            await AzureService.DefaultService.client.InvokeApiAsync("/api/accounts/changepassword", model);
                            edit.PutString("Password", password);
                            Snackbar.Make(anchor, "Password change successful.", Snackbar.LengthIndefinite).Show();
                        }
                        catch (System.Exception ex)
                        {

                            CreateAndShowDialog(ex.Message, "Error");
                        }
                    }
                    else if (networkStatusMonitor.State == NetworkState.Disconnected)
                    {
                        CreateAndShowDialog("Your device is currently not connected to the internet. Please checkyour data or WiFi connection and try again.", "Network Connectivity Error");
                    }
                }
            };

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

        private void CreateAndShowDialog(System.Exception exception, string title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        private void CreateAndShowDialog(string message, string title)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.SetPositiveButton("Okay", delegate { builder.Dispose(); });
            builder.Create().Show();
        }

        public void AfterTextChanged(IEditable s)
        {
            if (newPassword.HasFocus)
            {
                validityChecker.validatePassword(newPassword, newPassword.EditText.Text, username);
            }
            else if (confrimPassword.HasFocus)
            {
                if (newPassword.EditText.Text != confrimPassword.EditText.Text)
                {
                    confrimPassword.Error = "Passwords do not match.";
                }
                else
                {
                    confrimPassword.ErrorEnabled = false;
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