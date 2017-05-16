using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using DivineVerITies.Helpers;
using Java.Lang;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace DivineVerITies
{
    public class AudioAlbumRecyclerViewAdapter : RecyclerView.Adapter, IFilterable
    {
        public List<Album> mAudios;
        public List<Album> mFilterAudios;
        private readonly TypedValue mTypedValue = new TypedValue();
        private int mBackground;
        Resources mResource;
        private Context mContext;
        public event EventHandler<int> itemClick;
        bool isFixedSize;
        public string searchString;

        public AudioAlbumRecyclerViewAdapter(Context context, List<Album> audios, Resources res, bool fixedSize)
        {
            mContext = context;
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            mBackground = mTypedValue.ResourceId;
            mAudios = audios;
            mResource = res;
            isFixedSize = fixedSize;
            Filter = new AudioFilter(this);
        }

        public override int ItemCount
        {
            get
            {
                return mAudios.Count;
            }
        }

        public Filter Filter
        {
            get; private set;
        }

        public void Add(Album audio)
        {
            mAudios.Add(audio);
            NotifyDataSetChanged();
        }

        void OnClick(int position)
        {
            itemClick?.Invoke(this, position);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            var simpleHolder = holder as SimpleAudioViewHolder;
            //int indexPosition = (mAudios.Count - 1) - position;

            simpleHolder.mAudioTitle.Text = mAudios[position].Title;
            simpleHolder.mAudioSubtitle.Text = mAudios[position].Count.ToString()+" Podcasts";

            Glide.With(mContext)
                .Load(mAudios[position].ImageUrl)
                .Placeholder(Resource.Drawable.Logo_trans192)
                .Error(Resource.Drawable.Logo_trans192)
                .SkipMemoryCache(true)
                //.Thumbnail(1)
                .DiskCacheStrategy(DiskCacheStrategy.All)
                .Into(simpleHolder.mAlbumArt);

            string Msg = mAudios[position].Title.ToLower(CultureInfo.CurrentCulture);
            if (!string.IsNullOrEmpty(searchString) && Msg.Contains(searchString))
            {
                int startPos = Msg.IndexOf(searchString);
                int endPos = startPos + searchString.Length;

                ISpannable spanString = new SpannableFactory().NewSpannable(mAudios[position].Title);
                spanString.SetSpan(new ForegroundColorSpan(Color.ParseColor("#FF4081")), startPos, endPos, SpanTypes.ExclusiveExclusive);

                simpleHolder.mAudioTitle.SetText(spanString, TextView.BufferType.Spannable);
            }

            simpleHolder.mOptions.Click += (s, e) =>
            {
                Android.Support.V7.Widget.PopupMenu Popup = new Android.Support.V7.Widget.PopupMenu(simpleHolder.mOptions.Context, simpleHolder.mOptions);
                Popup.Inflate(Resource.Menu.audio_menu_album);
                Popup.MenuItemClick += (o, args) =>
                {
                    switch (args.Item.ItemId)
                    {
                        //case Resource.Id.action_add_favourite:
                        //    break;

                        //case Resource.Id.action_play_next:
                        //    if(MediaPlayerService.playlist.Count !=0)
                        //        MediaPlayerService.playlist.AddRange(mAudios[position].members);
                        //    else
                        //    {
                        //        MediaPlayerService.playlist.AddRange(mAudios[position].members);
                        //        MainApp.visibility = ViewStates.Visible;
                        //    }
                        //    break;

                        case Resource.Id.action_Download:
                            if (MyService.typeQueue.Count == 0)
                            {
                                foreach(var single in mAudios[position].members)
                                {
                                    MyService.typeQueue.Enqueue("audio");
                                    MyService.audioQueue.Enqueue(single);
                                }
                                
                                MyService.contxt = mContext;
                                var intenta = new Intent(mContext, typeof(MyService));
                                intenta.SetAction(MyService.StartD);
                                mContext.StartService(intenta);
                            }
                            else
                            {
                                foreach (var single in mAudios[position].members)
                                {
                                    MyService.typeQueue.Enqueue("audio");
                                    MyService.audioQueue.Enqueue(single);
                                }

                            }
                            break;

                        //case Resource.Id.action_details:
                        //    //string serial;
                        //    //intent = new Intent(mContext, typeof(PodcastDetails));
                        //    //serial = JsonConvert.SerializeObject(mAudios[position]);
                        //    //intent.PutExtra("selectedItem", serial);
                        //    //mContext.StartActivity(intent);
                        //    break;
                    }
                }; Popup.Show();
            };
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

            return new SimpleAudioViewHolder(view, OnClick);
        }

        public class SimpleAudioViewHolder : RecyclerView.ViewHolder
        {
            public View mMainAudioView { get; set; }
            public TextView mAudioTitle { get; set; }
            public TextView mAudioSubtitle { get; set; }
            public ImageView mAlbumArt { get; set; }
            public ImageButton mOptions { get; set; }

            public SimpleAudioViewHolder(View view, Action<int> listener)
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

        public class AudioFilter : Filter
        {
            private readonly AudioAlbumRecyclerViewAdapter _adapter;
            public AudioFilter(AudioAlbumRecyclerViewAdapter adapter)
            {
                _adapter = adapter;
            }

            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                var returnObj = new FilterResults();
                var results = new List<Album>();
                if (_adapter.mFilterAudios == null)
                    _adapter.mFilterAudios = _adapter.mAudios;

                if (constraint == null) return returnObj;

                if (_adapter.mFilterAudios != null && _adapter.mFilterAudios.Any())
                {
                    try
                    {
                        // Compare constraint to all names lowercased. 
                        // It they are contained they are added to results.
                        results.AddRange(
                            _adapter.mFilterAudios.Where(
                            audio => audio.Title.ToLower().Contains(constraint.ToString())));
                            //|| audio.SubTitle.ToLower().Contains(constraint.ToString())));
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
                        _adapter.mAudios = values.ToArray<Java.Lang.Object>()
                            .Select(r => r.ToNetObject<Album>()).ToList();
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