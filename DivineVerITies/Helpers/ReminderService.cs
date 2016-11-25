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

namespace DivineVerITies.Helpers
{
    public class ReminderService : IReminderService
    {
        public void Remind(Android.Content.Context context,DateTime dateTime, string title, string message)
        {
            Intent alarmIntent = new Intent(context, typeof(AlarmReceiver));
            alarmIntent.PutExtra("message", message);
            alarmIntent.PutExtra("title", title);
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(context, 0,alarmIntent, PendingIntentFlags.UpdateCurrent);
            AlarmManager alarmManager = (AlarmManager)context.GetSystemService(Android.Content.Context.AlarmService);
            //alarmManager.Set(AlarmType.ElapsedRealtime, SystemClock.ElapsedRealtime() + 5 * 1000, pendingIntent);
            
            var tomorrowMorningTime = dateTime;
            var timeDifference = tomorrowMorningTime-DateTime.Now;
            var millisecondsInOneDay = 86400000;
            alarmManager.SetInexactRepeating(AlarmType.ElapsedRealtimeWakeup, SystemClock.ElapsedRealtime() + (long)timeDifference.TotalMilliseconds, (long)millisecondsInOneDay, pendingIntent);
        }
    }

    public interface IReminderService
    {
        void Remind(Android.Content.Context context,DateTime dateTIme, string title, string message);
    }
}