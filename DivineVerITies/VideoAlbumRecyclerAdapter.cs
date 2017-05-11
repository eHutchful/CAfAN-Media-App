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
using Android.Util;
using Android.Content.Res;
using System.Globalization;
using Android.Text;
using Android.Text.Style;
using Android.Graphics;
using DivineVerITies.Helpers;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using Java.Lang;

namespace DivineVerITies
{
    public class VideoAlbumRecyclerViewAdapter : RecyclerView.Adapter, IFilterable
    {
        public List<Video> mVideos;
        public List<Video> mFilterVideos;
        private readonly TypedValue mTypedValue = new TypedValue();
        private int mBackground;
        Resources mResource;
        private Context mContext;
        public event EventHandler<int> itemClick;
        public Filter Filter { get; private set; }
        private List<int> test = new List<int>();
        internal string searchString;
        private bool isFixedSize;

        public VideoAlbumRecyclerViewAdapter(Context context, List<Video> videos, Resources res, bool fixedSize)
        {
            mContext = context;
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            mBackground = mTypedValue.ResourceId;
            mVideos = videos;
            mResource = res;
            isFixedSize = fixedSize;
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
            itemClick?.Invoke(this, position);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var simpleHolder = holder as SimpleVideoViewHolder;
            simpleHolder.mAudioTitle.Text = mVideos[position].Title;
            simpleHolder.mAudioSubtitle.Text = mVideos[position].SubTitle;

            string Msg = mVideos[position].Title.ToLower(CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(searchString) && Msg.Contains(searchString))
            {
                int startPos = Msg.IndexOf(searchString);
                int endPos = startPos + searchString.Length;

                ISpannable spanString = new SpannableFactory().NewSpannable(mVideos[position].Title);
                spanString.SetSpan(new ForegroundColorSpan(Color.ParseColor("#FF4081")), startPos, endPos, SpanTypes.ExclusiveExclusive);

                simpleHolder.mAudioTitle.SetText(spanString, TextView.BufferType.Spannable);
            }

            simpleHolder.mMainAudioView.Click += (sender, er) =>
            {
                if (!simpleHolder.mMainAudioView.Selected)
                    test.Remove(position);
            };
            simpleHolder.mMainAudioView.LongClick += (sender, er) =>
            {
                test.Add(position);
            };
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
                            MyService.contxt = mContext;
                            var intent = new Intent(mContext, typeof(MyService));
                            intent.SetAction(MyService.Startvd);
                            mContext.StartService(intent);
                            break;
                    }
                }; Popup.Show();
            };

            Glide.With(simpleHolder.mAlbumArt.Context)
                .Load(mVideos[position].ImageUrl)
                .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                .Error(Resource.Drawable.ChurchLogo_Gray)
                .DiskCacheStrategy(DiskCacheStrategy.All)
                .Into(simpleHolder.mAlbumArt);
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view;
            if (!isFixedSize)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.album_card, parent, false);
                view.SetBackgroundResource(mBackground);
            }
            else
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.album_card2, parent, false);
                view.SetBackgroundResource(mBackground);
            }

            return new SimpleVideoViewHolder(view, OnClick);
        }
        public class SimpleVideoViewHolder : RecyclerView.ViewHolder
        {
            public View mMainAudioView { get; set; }
            public TextView mAudioTitle { get; set; }
            public TextView mAudioSubtitle { get; set; }
            public ImageView mAlbumArt { get; set; }
            public ImageButton mOptions { get; set; }

            public SimpleVideoViewHolder(View view, Action<int> listener)
                : base(view)
            {
                mMainAudioView = view;
                mAudioTitle = view.FindViewById<TextView>(Resource.Id.title);
                mAudioSubtitle = view.FindViewById<TextView>(Resource.Id.count);
                mAlbumArt = view.FindViewById<ImageView>(Resource.Id.thumbnail);
                mOptions = view.FindViewById<ImageButton>(Resource.Id.overflow);
                mMainAudioView.Click += (sender, e) => listener(AdapterPosition);
            }
        }
        class VideoFilter : Filter
        {
            private readonly VideoAlbumRecyclerViewAdapter _adapter;
            public VideoFilter(VideoAlbumRecyclerViewAdapter adapter)
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
                try
                {
                    using (var values = results.Values)
                        _adapter.mVideos = values.ToArray<Java.Lang.Object>()
                            .Select(r => r.ToNetObject<Video>()).ToList();
                }
                catch (NullReferenceException)
                {

                    Thread.Sleep(500);
                }

                _adapter.NotifyDataSetChanged();

                // Don't do this and see GREF counts rising
                constraint.Dispose();
                results.Dispose();
            }
        }

    }
}