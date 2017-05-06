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
using Android.Support.V7.Widget;
using Android.Graphics.Drawables;
using Android.Content.Res;
using Android.Graphics;
using Android.Support.V4.View;

namespace DivineVerITies.Helpers
{
    public class DividerDecoration : RecyclerView.ItemDecoration
    {

        private static readonly int[] ATTRS = new int[] { Android.Resource.Attribute.ListDivider };
        public static readonly int HORIZONTAL_LIST = LinearLayoutManager.Horizontal;
        public static readonly int VERTICAL_LIST = LinearLayoutManager.Vertical;
        private Drawable mDivider;
        private int mOrientation;


        public DividerDecoration(Context context, int orientation)
        {
            TypedArray typedArray = context.ObtainStyledAttributes(ATTRS);
            mDivider = typedArray.GetDrawable(0);
            typedArray.Recycle();
            mOrientation = orientation;

        }

        public virtual int Orientation
        {
            set
            {
                if (value != HORIZONTAL_LIST && value != VERTICAL_LIST)
                {
                    throw new System.ArgumentException("invalid orientation");
                }
                mOrientation = value;
            }
        }

        public override void OnDraw(Canvas c, RecyclerView parent)
        {
            if (mOrientation == VERTICAL_LIST)
            {
                drawVertical(c, parent);
            }
            else
            {
                drawHorizontal(c, parent);
            }
        }

        public virtual void drawVertical(Canvas c, RecyclerView parent)
        {
            int left = parent.PaddingLeft;
         
            int right = parent.Width - parent.PaddingRight;
            
            int childCount = parent.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                View child = parent.GetChildAt(i);
                RecyclerView v = new RecyclerView(parent.Context);
               
                RecyclerView.LayoutParams @params = (RecyclerView.LayoutParams)child.LayoutParameters;
              
                int top = child.Bottom + @params.BottomMargin;
                
                int bottom = top + mDivider.IntrinsicHeight;
                mDivider.SetBounds(left, top, right, bottom);
                mDivider.Draw(c);
            }
        }

        public virtual void drawHorizontal(Canvas c, RecyclerView parent)
        {
            int top = parent.PaddingTop;
           
            int bottom = parent.Height - parent.PaddingBottom;
            
            int childCount = parent.ChildCount;
            for (int i = 0; i < childCount; i++)
            {
                View child = parent.GetChildAt(i);
                
                RecyclerView.LayoutParams @params = (RecyclerView.LayoutParams)child.LayoutParameters;
               
                int left = child.Right + @params.RightMargin;
                
                int right = left + mDivider.IntrinsicHeight;
                mDivider.SetBounds(left, top, right, bottom);
                mDivider.Draw(c);
            }
        }


        public override void GetItemOffsets(Rect outRect, int itemPosition, RecyclerView parent)
        {
            if (mOrientation == VERTICAL_LIST)
            {
                outRect.Set(0, 0, 0, mDivider.IntrinsicHeight);
            }
            else
            {
                outRect.Set(0, 0, mDivider.IntrinsicWidth, 0);
            }
        }
    }

}