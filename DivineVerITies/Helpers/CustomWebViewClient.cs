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
using Android.Webkit;

namespace DivineVerITies.Helpers
{
    public class CustomWebViewClient : WebViewClient
    {
        private Context mContext;
        public delegate void ToggleProgressBar(int state);
        public ToggleProgressBar mOnProgressChanged;

        public CustomWebViewClient(Context context)
        {
            mContext = context;
        }

        public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
        {
            if (request.Url.Equals(new Uri("Privacy")))
            {
                Intent intent = new Intent(mContext, typeof(Privacy));
                mContext.StartActivity(intent);
                return true;
            }
            
            else
            {
                view.LoadUrl(request.Url.ToString());
                return true;
            }
        }

        public override void OnPageStarted(WebView view, string url, Android.Graphics.Bitmap favicon)
        {
            if (mOnProgressChanged != null)
            {
                mOnProgressChanged.Invoke(1);
            }
            base.OnPageStarted(view, url, favicon);
        }

        public override void OnPageFinished(WebView view, string url)
        {
            if (mOnProgressChanged != null)
            {
                mOnProgressChanged.Invoke(0);
            }

            base.OnPageFinished(view, url);
        }
    }
}