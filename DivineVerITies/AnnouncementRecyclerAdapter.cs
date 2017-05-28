using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using System.Collections.Generic;
using Android.Util;
using Android.Content.Res;
using Android.Content;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;

namespace DivineVerITies
{
    public class AnnouncementRecyclerAdapter : RecyclerView.Adapter
    {
        public event EventHandler<AnnouncementRecyclerAdapterClickEventArgs> ItemClick;
        public event EventHandler<AnnouncementRecyclerAdapterClickEventArgs> ItemLongClick;
        List<Announcement> mAnnouncements;
        private readonly TypedValue mTypedValue = new TypedValue();
        private int mBackground;
        Resources mResource;
        private Context mContext;

        public AnnouncementRecyclerAdapter(Context context, List<Announcement> announcements, Resources res)
        {
            mContext = context;
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            mBackground = mTypedValue.ResourceId;
            mAnnouncements = announcements;
            mResource = res;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            //Setup your layout here
            View itemView = null;
            itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.AnnouncementCard, parent, false);
            itemView.SetBackgroundResource(mBackground);

            var vh = new AnnouncementRecyclerAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            var item = mAnnouncements[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as AnnouncementRecyclerAdapterViewHolder;
            holder.Message.Text = item.message;
            holder.TimeStamp.Text = item.timestamp;

            Glide.With(mContext)
                .Load(item.imageURL)
                .Placeholder(Resource.Drawable.ChurchLogo)
                .Error(Resource.Drawable.ChurchLogo)
                .SkipMemoryCache(true)
                //.DiskCacheStrategy(DiskCacheStrategy.All)
                .Into(holder.Image);
        }

        public override int ItemCount => mAnnouncements.Count;

        void OnClick(AnnouncementRecyclerAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(AnnouncementRecyclerAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class AnnouncementRecyclerAdapterViewHolder : RecyclerView.ViewHolder
    {
        public View mView { get; set; }
        public TextView Message { get; set; }
        public ImageView Image { get; set; }
        public TextView TimeStamp { get; set; }


        public AnnouncementRecyclerAdapterViewHolder(View itemView, Action<AnnouncementRecyclerAdapterClickEventArgs> clickListener,
                            Action<AnnouncementRecyclerAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            mView = itemView;
            Message = itemView.FindViewById<TextView>(Resource.Id.message);
            TimeStamp = itemView.FindViewById<TextView>(Resource.Id.date);
            Image = itemView.FindViewById<ImageView>(Resource.Id.thumbnail);
            itemView.Click += (sender, e) => clickListener(new AnnouncementRecyclerAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new AnnouncementRecyclerAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class AnnouncementRecyclerAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}