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
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using ModernHttpClient;

namespace DivineVerITies.Helpers
{
    public class CustomMessageHandler : NativeMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ISharedPreferences pref = Application.Context.GetSharedPreferences("UserInfo", FileCreationMode.Private);
            ISharedPreferencesEditor edit = pref.Edit();

            if (string.IsNullOrWhiteSpace(pref.GetString("Token", string.Empty)))
            {
                //throw new InvalidOperationException("User is not currently logged in");
                request.Headers.Add("Authorization", "bearer " + AzureService.tempToken);
                return base.SendAsync(request, cancellationToken);
            }

            //request.Headers.Add("X-ZUMO-AUTH", pref.GetString("Token", String.Empty));
            request.Headers.Add("Authorization", "bearer " + pref.GetString("Token", string.Empty));
            //request.Headers.Add("ZUMO-API-VERSION", "2.0.0");

            return base.SendAsync(request, cancellationToken);
        }
    }
}