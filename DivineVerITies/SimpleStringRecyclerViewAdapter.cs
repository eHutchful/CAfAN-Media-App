using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using DivineVerITies.Helpers;
using System.Collections.Generic;

namespace DivineVerITies
{
    class SimpleStringRecyclerViewAdapter : RecyclerView.Adapter
    {
        private readonly TypedValue mTypedValue = new TypedValue();
        private int mBackground;
        private List<string> mValues;
        Resources mResource;
        private Dictionary<int, int> mCalculatedSizes;

        public SimpleStringRecyclerViewAdapter(Context context, List<string> items, Resources res)
        {
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            mBackground = mTypedValue.ResourceId;
            mValues = items;
            mResource = res;

            mCalculatedSizes = new Dictionary<int, int>();
        }

        public override int ItemCount
        {
            get
            {
                return mValues.Count;
            }
        }

        public override async void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var simpleHolder = holder as SimpleViewHolder;

            simpleHolder.mBoundString = mValues[position];
            simpleHolder.mTxtView.Text = mValues[position];

            int drawableID = Resource.Drawable.Logo_trans192;
            BitmapFactory.Options options = new BitmapFactory.Options();

            if (mCalculatedSizes.ContainsKey(drawableID))
            {
                options.InSampleSize = mCalculatedSizes[drawableID];
            }

            else
            {
                options.InJustDecodeBounds = true;

                BitmapFactory.DecodeResource(mResource, drawableID, options);

                options.InSampleSize = Topics.CalculateInSampleSize(options, 100, 100);
                options.InJustDecodeBounds = false;

                mCalculatedSizes.Add(drawableID, options.InSampleSize);
            }


            var bitMap = await BitmapFactory.DecodeResourceAsync(mResource, drawableID, options);

            simpleHolder.mImageView.SetImageBitmap(bitMap);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.List_Topic, parent, false);
            view.SetBackgroundResource(mBackground);

            return new SimpleViewHolder(view);
        }
    }

    public class SimpleViewHolder : RecyclerView.ViewHolder
    {
        public string mBoundString;
        public readonly View mView;
        public readonly ImageView mImageView;
        public readonly TextView mTxtView;
        public readonly CheckBox mCbxFavTopic;

        public SimpleViewHolder(View view)
            : base(view)
        {
            mView = view;
            mImageView = view.FindViewById<ImageView>(Resource.Id.avatar);
            mTxtView = view.FindViewById<TextView>(Resource.Id.txtTopic);
            mCbxFavTopic = view.FindViewById<CheckBox>(Resource.Id.cbxFavTopics);
        }

        public override string ToString()
        {
            return base.ToString() + " '" + mTxtView.Text;
        }
    }
}