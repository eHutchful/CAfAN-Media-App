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
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using Android.Content.Res;
using Android.Util;

namespace DivineVerITies.Helpers
{
    class MediaRecyclerAdapter : RecyclerView.Adapter
    {
        public List<Media> media;
        private Context mContext;
        private readonly TypedValue mTypedValue = new TypedValue();
        private int Background;
        private Resources mResource;
        public event EventHandler<int> itemClick;
        public MediaRecyclerAdapter(Context context, List<Media> media, Resources res)
        {
            mContext = context;
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            Background = mTypedValue.ResourceId;
            this.media = media;
            mResource = res;
                        
        }
        public override int ItemCount
        {
            get
            {
                return media.Count;
            }
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var simpleHolder = holder as SimpleMediaHolder;
            simpleHolder.Title.Text = media[position].Title;
            simpleHolder.Album.Text = media[position].Album;

            Glide.With(mContext)
                .Load(media[position].AlbumArt)
                .Placeholder(Resource.Drawable.Logo_trans192)
                .Error(Resource.Drawable.Logo_trans192)
                .SkipMemoryCache(true)
                //.Thumbnail(1)
                .DiskCacheStrategy(DiskCacheStrategy.All)
                .Into(simpleHolder.AlbumArt);

           

        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.album_card, parent, false);
            view.SetBackgroundResource(Background);
            SimpleMediaHolder simpleHolder = new SimpleMediaHolder(view, OnClick);
            return simpleHolder;
        }

        public class SimpleMediaHolder : RecyclerView.ViewHolder
        {
            public View mainView { get; set; }
            public TextView Title { get; set; }
            public TextView Album { get; set; }
            public ImageView AlbumArt { get; set; }
            

            public SimpleMediaHolder(View view, Action<int> listener):base(view)
            {
                mainView = view;
                Title = view.FindViewById<TextView>(Resource.Id.title);
                Album = view.FindViewById<TextView>(Resource.Id.count);
                AlbumArt = view.FindViewById<ImageView>(Resource.Id.thumbnail);
                
                mainView.Click += (sender, e) => listener(AdapterPosition);
            }
        }

        void OnClick(int position)
        {
            itemClick?.Invoke(this, position);
        }
    }
}