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
    public class Test:Java.Lang.Object, Android.Views.ViewTreeObserver.IOnGlobalLayoutListener
    {
        private Action setListener;
        public Test(Action setListener)
        {
            this.setListener += setListener;
        }
        public void OnGlobalLayout()
        {
            this.setListener();
        }
    }
}