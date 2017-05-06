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
using Android.Content.PM;
using Android.Support.V7.App;
using Android.Graphics;
using DivineVerITies.Helpers;

namespace DivineVerITies
{
    [Activity(MainLauncher = true, ScreenOrientation = ScreenOrientation.Portrait, Theme = "@style/Theme.Splash")]
    public class Splash : AppCompatActivity
    {
        private bool success;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
        }

        protected async override void OnResume()
        {
            base.OnResume();

            ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            ISharedPreferencesEditor edit = pref.Edit();

            try
            {
                success = await AzureService.DefaultService.AutoAuthenticate();
            }
            catch (Exception e)
            {

                if (e.GetType() == typeof(Java.Net.UnknownHostException))
                {
                    CreateAndShowDialog("Please Ensure That You Are Connected To The Internet And Try Again", "Connection Error");
                }
                else
                {
                    CreateAndShowDialog(e.Message, "Error");
                }
            }

            if (success)
            {
               
                Intent intent = new Intent(this, typeof(MainApp));
                StartActivity(intent);
                Finish();
            }
            else
            {
                Intent intent = new Intent(this, typeof(MainActivity));
                StartActivity(intent);
                Finish();
            }
        }

        private void CreateAndShowDialog(Exception exception, string title)
        {
            CreateAndShowDialog(exception.Message, title);
        }

        private void CreateAndShowDialog(string message, string title)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(this);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.SetPositiveButton("OKAY", delegate { builder.Dispose(); });
            builder.Create().Show();
        }
    }
}