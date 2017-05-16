using System;

using Android.App;
using Android.Content;
using Android.Widget;
using Gcm.Client;
using Microsoft.WindowsAzure.MobileServices;
using Newtonsoft.Json.Linq;
using Android.Support.V4.App;

[assembly: Permission(Name = "com.divineverities.mobile.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.divineverities.mobile.permission.C2D_MESSAGE")]
[assembly: UsesPermission(Name = "com.google.android.c2dm.permission.RECEIVE")]

//GET_ACCOUNTS is only needed for android versions 4.0.3 and below
[assembly: UsesPermission(Name = "android.permission.GET_ACCOUNTS")]
[assembly: UsesPermission(Name = "android.permission.INTERNET")]
[assembly: UsesPermission(Name = "android.permission.WAKE_LOCK")]

namespace DivineVerITies.Helpers
{
    [BroadcastReceiver(Permission = Constants.PERMISSION_GCM_INTENTS)]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_MESSAGE },
     Categories = new string[] { "com.divineverities.mobile" })]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_REGISTRATION_CALLBACK },
     Categories = new string[] { "com.divineverities.mobile" })]
    [IntentFilter(new string[] { Constants.INTENT_FROM_GCM_LIBRARY_RETRY },
 Categories = new string[] { "com.divineverities.mobile" })]
    public class PushBroadcastReceiver : GcmBroadcastReceiverBase<PushHandlerService>
    {
        // Set the Google app ID.
        public static string[] senderIDs = new string[] { "180119202282" };

    }

    // The ServiceAttribute must be applied to the class.
    [Service]
    public class PushHandlerService : GcmServiceBase
    {
        public static string RegistrationID { get; private set; }

        public PushHandlerService() : base(PushBroadcastReceiver.senderIDs) { }

        protected override void OnMessage(Context context, Intent intent)
        {
            string message = string.Empty;
            string type = string.Empty;
            string tag = string.Empty;

            // Extract the push notification message from the intent.
            if (intent.Extras.ContainsKey("message") && intent.Extras.Get("url").ToString()=="")
            {
                message = intent.Extras.Get("message").ToString();
                type = intent.Extras.Get("type").ToString();
                tag = intent.Extras.Get("category").ToString();

                var title = $"New {type} added:";

                // Create a notification manager to send the notification.
                var notificationManager =
                    GetSystemService(NotificationService) as NotificationManager;

                // Create a new intent to show the notification in the UI.
                Intent secondIntent = new Intent(this, typeof(MainApp));

                if (type == "audio")
                {
                    secondIntent.PutExtra("SELECTED_TAB_EXTRA_KEY", 0);
                }
                else if (type == "video")
                {
                    secondIntent.PutExtra("SELECTED_TAB_EXTRA_KEY", 1);
                }
                PendingIntent contentIntent =
                    PendingIntent.GetActivity(context, 0, secondIntent, 0);

                // Create the notification using the builder.
                var builder = new NotificationCompat.Builder(context);
                builder.SetAutoCancel(true);
                builder.SetContentTitle(title);
                builder.SetContentText(message);
                builder.SetSmallIcon(Resource.Drawable.ChurchLogo);
                builder.SetContentIntent(contentIntent);
                builder.SetPriority(NotificationCompat.PriorityMax);
                builder.SetDefaults(NotificationCompat.DefaultAll);
                if ((int)Android.OS.Build.VERSION.SdkInt >= 21)
                {
                    builder.SetCategory(NotificationCompat.CategoryMessage);
                    builder.SetVisibility(NotificationCompat.VisibilitySecret);
                }
                var notification = builder.Build();

                // Display the notification in the Notifications Area.
                notificationManager.Notify(type, 1, notification);

            }

            else if (intent.Extras.ContainsKey("message") && intent.Extras.Get("url").ToString() != string.Empty)
            {
                message = intent.Extras.Get("message").ToString();
                type = intent.Extras.Get("type").ToString();
                tag = intent.Extras.Get("url").ToString();

                var title = $"New {type} added:";

                // Create a notification manager to send the notification.
                var notificationManager =
                    GetSystemService(NotificationService) as NotificationManager;

                // Create a new intent to show the notification in the UI.
                Intent secondIntent = new Intent(this, typeof(MainApp));

                secondIntent.PutExtra("TAG", tag);
                    
                PendingIntent contentIntent =
                    PendingIntent.GetActivity(context, 0, secondIntent, 0);

                // Create the notification using the builder.
                var builder = new NotificationCompat.Builder(context);
                builder.SetAutoCancel(true);
                builder.SetContentTitle(title);
                builder.SetContentText(message);
                builder.SetSmallIcon(Resource.Drawable.ChurchLogo);
                builder.SetContentIntent(contentIntent);
                builder.SetPriority(NotificationCompat.PriorityMax);
                builder.SetDefaults(NotificationCompat.DefaultAll);
                if ((int)Android.OS.Build.VERSION.SdkInt >= 21)
                {
                    builder.SetCategory(NotificationCompat.CategoryMessage);
                    builder.SetVisibility(NotificationCompat.VisibilitySecret);
                }
                var notification = builder.Build();

                // Display the notification in the Notifications Area.
                notificationManager.Notify(type, 2, notification);

            }
        }

        protected override void OnError(Context context, string errorId)
        {
            System.Diagnostics.Debug.WriteLine(
                string.Format("Error occurred in the notification: {0}.", errorId));
            Toast.MakeText(
                context, 
                string.Format("Error occurred in the notification: {0}.", errorId), 
                ToastLength.Long)
                .Show();
        }

        protected override void OnRegistered(Context context, string registrationId)
        {
            System.Diagnostics.Debug.WriteLine("The device has been registered with GCM.", "Success!");

            // Get the MobileServiceClient from the current activity instance.
            MobileServiceClient client = AzureService.DefaultService.client;
            var push = client.GetPush();

            // Define a message body for GCM.
            const string templateBodyGCM = "{\"data\":{\"message\":\"$(messageParam)\",\"type\":\"$(mediaType)\",\"tag\":\"$(category)\", \"url\":\"$(url)\"}}";

            // Define the template registration as JSON.
            JObject templates = new JObject();
            templates["genericMessage"] = new JObject
             {
               {"body", templateBodyGCM }
             };

            try
            {
                // Make sure we run the registration on the same thread as the activity, 
                // to avoid threading errors.
                MainApp.CurrentActivity.RunOnUiThread(

                    // Register the template with Notification Hubs.
                    async () => await push.RegisterAsync(registrationId, templates));

                System.Diagnostics.Debug.WriteLine(
                    string.Format("Push Installation Id", push.InstallationId.ToString()));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(
                    string.Format("Error with Azure push registration: {0}", ex.Message));
            }
        }

        protected override void OnUnRegistered(Context context, string registrationId)
        {
            Toast.MakeText(
               context,
               string.Format("Client unregistered from notification hub: {0}.", registrationId),
               ToastLength.Long)
               .Show();
        }
    }
}