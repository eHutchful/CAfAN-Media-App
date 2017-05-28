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
using Java.Lang;
using System.Globalization;
using Android.Text;
using Android.Text.Style;
using Android.Graphics;

namespace DivineVerITies.Helpers
{
    public class MediaRecyclerAdapter : RecyclerView.Adapter, IFilterable
    {
        public List<Media> media;
        public List<Media> filterMedia;
        private Context mContext;
        private readonly TypedValue mTypedValue = new TypedValue();
        private int Background;
        private Resources mResource;
        public event EventHandler<int> itemClick;
        private string type;      
        const int Audios = -100;   
        const int Videos = -200;
        public string searchString;
        private bool isFixedSize;

        public MediaRecyclerAdapter(Context context, List<Media> media, Resources res, string type, bool fixedSize)
        {
            mContext = context;
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            Background = mTypedValue.ResourceId;
            this.media = media;
            mResource = res;
            this.type = type;
            isFixedSize = fixedSize;
            Filter = new MediaFilter(this);           
        }
        public override int ItemCount
        {
            get
            {
                return media.Count;
            }
        }

        public Filter Filter
        {
            get; private set;
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (media.Count > 0)
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
                    //.DiskCacheStrategy(DiskCacheStrategy.All)
                    .Into(simpleHolder.AlbumArt);

                string Msg = media[position].Title.ToLower(CultureInfo.CurrentCulture);
                if (!string.IsNullOrEmpty(searchString) && Msg.Contains(searchString))
                {
                    int startPos = Msg.IndexOf(searchString);
                    int endPos = startPos + searchString.Length;

                    ISpannable spanString = new SpannableFactory().NewSpannable(media[position].Title);
                    spanString.SetSpan(new ForegroundColorSpan(Color.ParseColor("#FF4081")), startPos, endPos, SpanTypes.ExclusiveExclusive);

                    simpleHolder.Album.SetText(spanString, TextView.BufferType.Spannable);
                }

                if (isFixedSize)
                {
                    simpleHolder.mOptions.Visibility = ViewStates.Gone;
                }
                else
                {
                    simpleHolder.mOptions.Visibility = ViewStates.Visible;
                }
            }
            else
            {
                var emptyHolder = holder as EmptyMediaHolder;

                if (type == "audio")
                {
                    emptyHolder.Text.Text = "No Audios Added";
                }
                else if (type == "video")
                {
                    emptyHolder.Text.Text = "No Videos Added";
                }
            }
        }

        public override int GetItemViewType(int position)
        {
            if (type == "audio")
            {
                return Audios;
            }
            else if (type == "video")
            {
                return Videos;
            }

            return position;
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view;
            switch (viewType)
            {
               
                case Videos:
                    if (isFixedSize)
                    {
                        view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.album_card2, parent, false);
                        view.SetBackgroundResource(Background);
                        
                        return new SimpleMediaHolder(view, OnClick);
                    }
                    else
                    {
                        view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.album_card, parent, false);
                        view.SetBackgroundResource(Background);
                        
                        return new SimpleMediaHolder(view, OnClick);
                    }
                    
                case Audios:
                    if (isFixedSize)
                    {
                        view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.album_card2, parent, false);
                        view.SetBackgroundResource(Background);

                        return new SimpleMediaHolder(view, OnClick);
                    }
                    else
                    {
                        view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.album_card, parent, false);
                        view.SetBackgroundResource(Background);

                        return new SimpleMediaHolder(view, OnClick);
                    }

                default:
                    view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.EmptyMediaCard, parent, false);
                    view.SetBackgroundResource(Background);
                    
                    return new EmptyMediaHolder(view);
            }
        }

        public class SimpleMediaHolder : RecyclerView.ViewHolder
        {
            public View mainView { get; set; }
            public TextView Title { get; set; }
            public TextView Album { get; set; }
            public ImageView AlbumArt { get; set; }
            public ImageButton mOptions { get; set; }

            public SimpleMediaHolder(View view, Action<int> listener):base(view)
            {
                mainView = view;
                Title = view.FindViewById<TextView>(Resource.Id.title);
                Album = view.FindViewById<TextView>(Resource.Id.count);
                AlbumArt = view.FindViewById<ImageView>(Resource.Id.thumbnail);
                mOptions = view.FindViewById<ImageButton>(Resource.Id.overflow);
                mainView.Click += (sender, e) => listener(AdapterPosition);
            }
        }

        public class EmptyMediaHolder : RecyclerView.ViewHolder
        {
            public View mainView { get; set; }
            public TextView Text { get; set; }

            public EmptyMediaHolder(View view) : base(view)
            {
                mainView = view;
                Text = view.FindViewById<TextView>(Resource.Id.txtEmptyText);
            }
        }

        void OnClick(int position)
        {
            itemClick?.Invoke(this, position);
        }

        public class MediaFilter : Filter
        {
            private readonly MediaRecyclerAdapter _adapter;
            public MediaFilter(MediaRecyclerAdapter adapter)
            {
                _adapter = adapter;
            }

            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                var returnObj = new FilterResults();
                var results = new List<Media>();
                if (_adapter.filterMedia == null)
                    _adapter.filterMedia = _adapter.media;

                if (constraint == null) return returnObj;

                if (_adapter.filterMedia != null && _adapter.filterMedia.Any())
                {
                    try
                    {
                        // Compare constraint to all names lowercased. 
                        // It they are contained they are added to results.
                        results.AddRange(
                            _adapter.filterMedia.Where(
                            audio => 
                            //audio.Title.ToLower().Contains(constraint.ToString()) ||
                            audio.Album.ToLower().Contains(constraint.ToString())));
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
                        _adapter.media = values.ToArray<Java.Lang.Object>()
                            .Select(r => r.ToNetObject<Media>()).ToList();
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