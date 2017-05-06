using System;
using Android.Support.V7.Widget;

namespace DivineVerITies.Helpers
{
    public class RecyclerViewSwipeListener : RecyclerView.OnFlingListener
    {
        private static int SWIPE_VELOCITY_THRESHOLD = 2000;

        bool mIsScrollingVertically;
        ISwipeListener parent = null;

        // change swipe listener depending on whether we are scanning items horizontally or vertically
        RecyclerViewSwipeListener(bool vertical, ISwipeListener parent)
        {
            mIsScrollingVertically = vertical;
            this.parent = parent;
        }
        public override bool OnFling(int velocityX, int velocityY)
        {
            if (mIsScrollingVertically && Math.Abs(velocityY) > SWIPE_VELOCITY_THRESHOLD)
            {
                if (velocityY < 0)
                {
                    parent.OnSwipeDown();
                }
                else
                {
                    parent.OnSwipeUp();
                }
                return true;
            }
            else if (!mIsScrollingVertically && Math.Abs(velocityX) > SWIPE_VELOCITY_THRESHOLD)
            {
                if (velocityX < 0)
                {
                    parent.OnSwipeLeft();
                }
                else
                {
                    parent.OnSwipeRight();
                }
                return true;
            }
            return false;
        }

        public interface ISwipeListener
        {
            void OnSwipeDown();
            void OnSwipeUp();
            void OnSwipeRight();
            void OnSwipeLeft();
        }
    }
}