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

        public override void OnLoadResource(WebView view, string url)
        {
            base.OnLoadResource(view, url);
        }

        public override void OnPageCommitVisible(WebView view, string url)
        {
            base.OnPageCommitVisible(view, url);
        }

        public override void OnReceivedClientCertRequest(WebView view, ClientCertRequest request)
        {
            base.OnReceivedClientCertRequest(view, request);
        }

        public override void OnUnhandledKeyEvent(WebView view, KeyEvent e)
        {
            base.OnUnhandledKeyEvent(view, e);
        }

        public override bool ShouldOverrideUrlLoading(WebView view, string url)
        {
            return base.ShouldOverrideUrlLoading(view, url);
        }

        public override WebResourceResponse ShouldInterceptRequest(WebView view, string url)
        {
            return base.ShouldInterceptRequest(view, url);
        }
        public override bool ShouldOverrideUrlLoading(WebView view, IWebResourceRequest request)
        {
            
            if (request.Url.ToString().Equals("privacy"))
            {
                Intent intent = new Intent(mContext, typeof(Privacy));
                mContext.StartActivity(intent);
                return true;
            }
            else if (request.Url.ToString().Equals("terms"))
            {
                Intent intent = new Intent(mContext, typeof(Terms));
                mContext.StartActivity(intent);
                return true;
            }
            else if (request.Url.ToString().Equals("feedback"))
            {
                Intent intent = new Intent(mContext, typeof(Feedback));
                mContext.StartActivity(intent);
                return true;
            }
            else
            {
                view.LoadUrl(request.Url.ToString());
                return true;
            }
        }

        public override WebResourceResponse ShouldInterceptRequest(WebView view, IWebResourceRequest request)
        {
            return base.ShouldInterceptRequest(view, request);
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