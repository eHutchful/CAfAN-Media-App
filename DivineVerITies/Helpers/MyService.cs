using Android.App;
using Android.Content;
using Android.OS;
using System;
using System.Threading;

namespace DivineVerITies.Helpers
{
    [Service]
    [IntentFilter(new[] { Cancel, Pause, Resume })]
    public class MyService : Service
    {
        public static CancellationTokenSource cts= new CancellationTokenSource();
        public const string Cancel = "com.xamarin.action.CANCEL";
        public const string Pause = "com.xamarin.action.PAUSE";
        public const string Resume = "com.xamarin.action.RESUME";
        public override IBinder OnBind(Intent intent)
        {
            return null;
        }
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            switch (intent.Action)
            {
                case Cancel:
                    try
                    {
                        cts.Cancel();
                    }
                    catch (Exception e)
                    {

                    }
                    finally { cancel(); }
                    
                    break;
                case Pause:
                    break;
                case Resume:
                    break;
            }
            return StartCommandResult.Sticky;
        }
        private void cancel()
        {
            NotificationManager notificationManager =
                        GetSystemService(Context.NotificationService) as NotificationManager;

            // Publish the notification:
            const int notificationId = 0;
            notificationManager.Cancel(notificationId);
        }
    }
}