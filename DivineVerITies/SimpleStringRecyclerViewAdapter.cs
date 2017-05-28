using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using DivineVerITies.Helpers;
using System;
using System.Collections.Generic;

namespace DivineVerITies
{
    class SimpleStringRecyclerViewAdapter : RecyclerView.Adapter
    {
        private readonly TypedValue mTypedValue = new TypedValue();
        private int mBackground;
        private List<string> mValues;
        Resources mResource;
        public event EventHandler<int> itemClick;
        private Context mContext;
       
        public SimpleStringRecyclerViewAdapter(Context context, List<string> items, Resources res)
        {
            mContext = context;
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            mBackground = mTypedValue.ResourceId;
            mValues = items;
            mResource = res;
        }

        public override int ItemCount
        {
            get
            {
                return mValues.Count;
            }
        }

        void OnClick(int position)
        {
            itemClick?.Invoke(this, position);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var simpleHolder = holder as SimpleViewHolder;

            Glide.With(simpleHolder.mImageView.Context)
                .Load(Resource.Drawable.ChurchLogo)
                .Transform(new CircleTransform(simpleHolder.mImageView.Context))
                .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                .Error(Resource.Drawable.ChurchLogo_Gray)
                //.DiskCacheStrategy(DiskCacheStrategy.All)
                .Into(simpleHolder.mImageView);

            simpleHolder.mTxtView.Text = mValues[position];

            //simpleHolder.mCbxFavTopic.Checked = ((SelectTopics)mContext).favourites.Contains(mValues[position]) ? true: false;
            simpleHolder.mCbxFavTopic.CheckedChange += (rv, r) => {
                if (r.IsChecked)
                {
                    SelectTopics.favourites.Add(mValues[position]);
                }
                else
                {
                    SelectTopics.favourites.Remove(mValues[position]);
                }
                   
            };
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
    }
}