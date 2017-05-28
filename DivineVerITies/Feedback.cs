
using Android.App;
using Android.OS;
using Android.Views;
using Android.Content.PM;
using Android.Support.V7.App;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.Design.Widget;
using Newtonsoft.Json.Linq;
using DivineVerITies.Helpers;
using Android.Content;

namespace DivineVerITies
{
    [Activity(Theme = "@style/Theme.DesignDemo", ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, NoHistory = true)]
    public class Feedback : AppCompatActivity
    {
        private SupportToolbar toolBar;
        private TextInputLayout Email;
        private TextInputLayout Subject;
        private TextInputLayout Body;
        private FloatingActionButton fab;
        private string username;
        private NetworkStatusMonitor networkStatusMonitor;
        ISharedPreferences pref;
        ISharedPreferencesEditor edit;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Feedback);
            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);
            SupportActionBar.Title = "Have Your Say";
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            networkStatusMonitor = new NetworkStatusMonitor();

            Email = FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutEmail);
            Subject = FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutSubject);
            Body = FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutBody);
            fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            edit = pref.Edit();

            username = pref.GetString("Username", string.Empty);

            fab.Click += async (o, e) =>
            {
                View anchor = o as View;

                if (string.IsNullOrWhiteSpace(Email.EditText.Text)
                || string.IsNullOrWhiteSpace(Subject.EditText.Text)
                || string.IsNullOrWhiteSpace(Body.EditText.Text))
                {
                    return;
                }

                JObject jo = new JObject();
                jo.Add("email", Email.EditText.Text);
                jo.Add("subject", Subject.EditText.Text);
                jo.Add("body", Body.EditText.Text);
                jo.Add("username", username);

                try
                {
                    if (networkStatusMonitor.State == NetworkState.ConnectedData || networkStatusMonitor.State == NetworkState.ConnectedWifi)
                    {
                        var t = await AzureService.DefaultService.client.InvokeApiAsync("/api/feedback/sendsupportmail", jo);
                        Email.EditText.Text = string.Empty;
                        Subject.EditText.Text = string.Empty;
                        Body.EditText.Text = string.Empty;
                        Snackbar.Make(anchor, "Thanks. We'll get back to you shortly.", Snackbar.LengthIndefinite).Show();
                    }
                    else if (networkStatusMonitor.State == NetworkState.Disconnected)
                    {
                        CreateAndShowDialog("Your device is currently not connected to the internet. Please checkyour data or WiFi connection and try again.", "Network Connectivity Error");
                    }

                }
                catch (System.Exception ex)
                {
                    Snackbar.Make(anchor, "An error occured! Please try again.", Snackbar.LengthIndefinite)
                    //.SetAction("RETRY", async delegate
                    //{
                    //    if (string.IsNullOrWhiteSpace(Email.EditText.Text)
                    //    || string.IsNullOrWhiteSpace(Subject.EditText.Text)
                    //    || string.IsNullOrWhiteSpace(Body.EditText.Text))
                    //    {
                    //        return;
                    //    }

                    //    JObject jo = new JObject();
                    //    jo.Add("email", Email.EditText.Text);
                    //    jo.Add("subject", Subject.EditText.Text);
                    //    jo.Add("body", Body.EditText.Text);
                    //    jo.Add("username", username);

                    //    await AzureService.DefaultService.client.InvokeApiAsync("/feedback", jo);
                    //    Snackbar.Make(anchor, "Thanks. We'll get back to you shortly.", Snackbar.LengthIndefinite).Show();
                    //})
                    .Show();

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
    }
}