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
using System.IO;

namespace DivineVerITies.Helpers
{
    [BroadcastReceiver(Enabled=true)]
    [IntentFilter(new[] {cancel})]
    public class CancelReceiver : BroadcastReceiver
    {
        public const string cancel = "com.xamarin.Action.CANCELLED";
        public override void OnReceive(Context context, Intent intent)
        {
            
            try
            {
                MyService.cancellations[intent.GetStringExtra("filename")].Cancel();
                //MyService.cancellations.Remove(intent.GetStringExtra("filename"));
                File.Delete(intent.GetStringExtra("filename"));
            }
            catch (Exception e)
            {

            }
            finally 
            { 
                MyService.cancel(MyService.notificationIds[intent.GetStringExtra("filename")]);
                //MyService.notificationIds.Remove(intent.GetStringExtra("filename"));
            }
     
        }
    }
}