
using Android.App;
using Android.OS;
using Android.Views;
using Android.Content.PM;
using Android.Support.V7.App;
using SupportActionBar = Android.Support.V7.App.ActionBar;
using SupportToolbar = Android.Support.V7.Widget.Toolbar;
using Android.Support.Design.Widget;

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

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.Feedback);
            toolBar = FindViewById<SupportToolbar>(Resource.Id.toolBar);
            SetSupportActionBar(toolBar);
            SupportActionBar.Title = "Have Your Say";
            SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            Email = FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutEmail);
            Subject = FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutSubject);
            Body = FindViewById<TextInputLayout>(Resource.Id.txtInputLayoutBody);
            fab = FindViewById<FloatingActionButton>(Resource.Id.fab);

            fab.Click += (o, e) =>
            {
                if (string.IsNullOrWhiteSpace(Email.EditText.Text)
                || string.IsNullOrWhiteSpace(Subject.EditText.Text)
                || string.IsNullOrWhiteSpace(Body.EditText.Text))
                {
                    return;
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