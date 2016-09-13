using Android.Content;
using Android.Content.Res;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using Java.Lang;
using System;
using System.Collections.Generic;
using System.Linq;
using Object = Java.Lang.Object;
using DivineVerITies.Helpers;
using DivineVerITies.Fragments;

namespace DivineVerITies
{
    public class VideoRecyclerViewAdapter: RecyclerView.Adapter, IFilterable
    {
        public List<Video> mVideos;
        public List<Video> mFilterVideos;
        private readonly TypedValue mTypedValue = new TypedValue();
        private int mBackground;
        Resources mResource;
        private Context mContext;
        public event EventHandler<int> itemClick;
        public Filter Filter { get; private set; }

        public VideoRecyclerViewAdapter(Context context, List<Video> videos, Resources res)
        {
            mContext = context;
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            mBackground = mTypedValue.ResourceId;
            mVideos= videos;
            mResource = res;

            Filter = new VideoFilter(this);
        }

        public override int ItemCount
        {
            get
            {
                return mVideos.Count;
            }
        }

        //public void Add(Video video)
        //{
        //    mVideos.Add(video);
        //    NotifyDataSetChanged();
        //}

        void OnClick(int position)
        {
            if (itemClick != null)
                itemClick(this, position);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var simpleHolder = holder as SimpleVideoViewHolder;           
            simpleHolder.mAudioTitle.Text = mVideos[position].Title;
            simpleHolder.mAudioSubTitle.Text = mVideos[position].SubTitle;
            simpleHolder.mPubDate.Text = mVideos[position].PubDate;
            simpleHolder.mOptions.Click += (s, e) =>
            {
                Android.Support.V7.Widget.PopupMenu Popup = new Android.Support.V7.Widget.PopupMenu(
                    simpleHolder.mOptions.Context, simpleHolder.mOptions);
                Popup.Inflate(Resource.Menu.menu_album);
                Popup.MenuItemClick += (o, args) =>
                {
                    switch (args.Item.ItemId)
                    {
                        case Resource.Id.action_add_favourite:
                            break;

                        case Resource.Id.action_play_next:
                            break;

                        case Resource.Id.action_Download:
                            MyService.selectedVideo = mVideos[position];
                            MyService.contxt = Fragment4.context;
                            var intent = new Intent(Fragment4.context, typeof(MyService));
                            intent.SetAction(MyService.StartD);
                            Fragment4.context.StartService(intent);
                            break;
                    }
                }; Popup.Show();
            };

            Glide.With(simpleHolder.mAlbumArt.Context)
                .Load("")
                .Placeholder(Resource.Drawable.Logo_trans192)
                .Error(Resource.Drawable.Logo_trans192)                
                .DiskCacheStrategy(DiskCacheStrategy.All)
                .Into(simpleHolder.mAlbumArt);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.audio_cardview, parent, false);
            view.SetBackgroundResource(mBackground);

            SimpleVideoViewHolder simpleHolder = new SimpleVideoViewHolder(view, OnClick);
 
            return simpleHolder;
        }
        public class SimpleVideoViewHolder : RecyclerView.ViewHolder
        {
            public View mMainAudioView { get; set; }
            public TextView mAudioTitle { get; set; }
            public TextView mAudioSubTitle { get; set; }
            public TextView mPubDate { get; set; }
            public ImageView mAlbumArt { get; set; }
            public ImageButton mOptions { get; set; }

            public SimpleVideoViewHolder(View view, Action<int> listener)
                : base(view)
            {
                mMainAudioView = view;
                mAudioTitle = view.FindViewById<TextView>(Resource.Id.txtRow1);
                mAudioSubTitle = view.FindViewById<TextView>(Resource.Id.txtRow2);
                mPubDate = view.FindViewById<TextView>(Resource.Id.txtRow3);
                mAlbumArt = view.FindViewById<ImageView>(Resource.Id.avatar);
                mOptions = view.FindViewById<ImageButton>(Resource.Id.img_options);
                mMainAudioView.Click += (sender, e) => listener(base.Position);
            }
        }
        class VideoFilter : Filter
        {
            private readonly VideoRecyclerViewAdapter _adapter;
            public VideoFilter(VideoRecyclerViewAdapter adapter)
            {
                _adapter = adapter;
            }

            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                var returnObj = new FilterResults();
                var results = new List<Video>();
                if (_adapter.mFilterVideos == null)
                    _adapter.mFilterVideos = _adapter.mVideos;

                if (constraint == null) return returnObj;

                if (_adapter.mFilterVideos != null && _adapter.mFilterVideos.Any())
                {
                    try
                    {
                        // Compare constraint to all names lowercased. 
                        // It they are contained they are added to results.
                        results.AddRange(
                            _adapter.mFilterVideos.Where(
                                video => video.Title.ToLower().Contains(constraint.ToString())));
                    }
                    catch (ArgumentNullException)
                    {
                        
                        return returnObj;
                    }
                }

                // Nasty piece of .NET to Java wrapping, be careful with this!
                returnObj.Values = FromArray(results.Select(r => r.ToJavaObject()).ToArray());
                returnObj.Count = results.Count;

                constraint.Dispose();

                return returnObj;
            }
            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
                using (var values = results.Values)
                    _adapter.mVideos = values.ToArray<Object>()
                        .Select(r => r.ToNetObject<Video>()).ToList();

                _adapter.NotifyDataSetChanged();

                // Don't do this and see GREF counts rising
                constraint.Dispose();
                results.Dispose();
            }
        }

    }
    }