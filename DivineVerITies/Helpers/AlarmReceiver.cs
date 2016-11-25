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
using Android.Support.V4.App;
using Android.Graphics;


namespace DivineVerITies.Helpers
{
    [BroadcastReceiver]
    public class AlarmReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            ContextWrapper cw = new ContextWrapper(context);
            //Toast.MakeText(context, "Received intent!", ToastLength.Short).Show();
            var message = intent.GetStringExtra("message");
            var title = intent.GetStringExtra("title");
            var notificationIntent = new Intent(context, typeof(MainApp));
            var contentIntent = PendingIntent.GetActivity(context, 0, notificationIntent, PendingIntentFlags.CancelCurrent);
            var manager = NotificationManagerCompat.From(context);

            var style = new NotificationCompat.BigTextStyle();
            style.BigText(message);
            //int resourceId;

            var builder = new NotificationCompat.Builder(context)
                .SetContentIntent(contentIntent)
                .SetSmallIcon(Resource.Drawable.Logo_trans72)
                .SetLargeIcon(BitmapFactory.DecodeResource(cw.Resources, Resource.Drawable.Logo_trans192))
                .SetContentTitle(title)
                .SetContentText(message)
                .SetStyle(style)
                .SetDefaults(3)
                .SetPriority(2)
                //.SetVisibility (NotificationVisibility.Public)
                .SetVisibility(3)
                .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis())
                .SetAutoCancel(true);
            var notification = builder.Build();
            manager.Notify(0, notification);

            
        }
    }
}