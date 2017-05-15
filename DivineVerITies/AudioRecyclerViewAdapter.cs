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
using Newtonsoft.Json;
using System.Globalization;
using Android.Text;
using Android.Text.Style;
using Android.Graphics;
using Android.App;
using Android.Support.V7.App;
using Android.Support.Design.Widget;

namespace DivineVerITies
{
    public class AudioRecyclerViewAdapter : RecyclerView.Adapter, IFilterable
    {
        public List<AudioList> mAudios;
        public List<AudioList> mFilterAudios;
        private readonly TypedValue mTypedValue = new TypedValue();
        private int mBackground;
        Resources mResource;
        public event EventHandler<int> itemClick;
        private Context mContext;
        public string searchString;
        private TabLayout selectedTab;

        public AudioRecyclerViewAdapter(Context context, List<AudioList> audios, Resources res)
        {
            mContext = context;
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            mBackground = mTypedValue.ResourceId;
            mAudios = audios;
            mResource = res;
            
            Filter = new AudioFilter(this);
            selectedTab = ((AppCompatActivity)mContext).FindViewById<TabLayout>(Resource.Id.tabs);
        }

        public override int ItemCount
        {
            get
            {
                return mAudios.Count;
            }
        }

        //public void Add(AudioList audio)
        //{
        //    mAudios.Add(audio);
        //    NotifyDataSetChanged();
        //}

        void OnClick(int position)
        {
            itemClick?.Invoke(this, position);
        }
        
        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            if (holder.GetType() == typeof(SimpleAudioViewHolder))
            {
                var simpleHolder = holder as SimpleAudioViewHolder;
                //int indexPosition = (mAudios.Count - 1) - position;

                simpleHolder.mAudioTitle.Text = mAudios[position].Title;
                simpleHolder.mAudioSubTitle.Text = mAudios[position].SubTitle;
                simpleHolder.mPubDate.Text = mAudios[position].PubDate;
                simpleHolder.mPlayed.Text = "Played";

                simpleHolder.mOptions.Click += (sender, argss) =>
                {

                    Android.Support.V7.Widget.PopupMenu Popup = new Android.Support.V7.Widget.PopupMenu(
                        simpleHolder.mOptions.Context, simpleHolder.mOptions);
                    Popup.Inflate(Resource.Menu.audio_menu_album);

                    Popup.MenuItemClick += (o, args) =>
                    {
                        switch (args.Item.ItemId)
                        {
                            case Resource.Id.action_add_favourite:
                                break;

                            //case Resource.Id.action_play_next:
                            //    Fragment6.mAudios.Add(mAudios[position]);
                            //    break;

                            case Resource.Id.action_Download:
                                if(MyService.typeQueue.Count == 0)
                                {
                                    MyService.typeQueue.Enqueue("audio");
                                    MyService.audioQueue.Enqueue(mAudios[position]);
                                    MyService.contxt = mContext;
                                    var intenta = new Intent(mContext, typeof(MyService));
                                    intenta.SetAction(MyService.StartD);
                                    mContext.StartService(intenta);
                                }
                                else
                                {
                                    MyService.typeQueue.Enqueue("audio");
                                    MyService.audioQueue.Enqueue(mAudios[position]);
                                }
                                
                                break;

                            case Resource.Id.action_details:
                                string serial;
                                var intent = new Intent(mContext, typeof(PodcastDetails));
                                serial = JsonConvert.SerializeObject(mAudios[position]);
                                intent.PutExtra("selectedItem", serial);
                                mContext.StartActivity(intent);
                                break;

                        }
                    }; Popup.Show();
                };

                Glide.With(simpleHolder.mAlbumArt.Context)
                    .Load(mAudios[position].ImageUrl)
                    .Transform(new CircleTransform(simpleHolder.mAlbumArt.Context))
                    .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                    .Error(Resource.Drawable.ChurchLogo_Gray)
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
            }
            else
            {
                var simpleHolder = holder as SimpleAudioViewHolder2;
                //int indexPosition = (mAudios.Count - 1) - position;

                simpleHolder.mAudioTitle.Text = mAudios[position].Title;
                simpleHolder.mAudioSubtitle.Text = mAudios[position].SubTitle;

                Glide.With(simpleHolder.mAlbumArt.Context)
                    .Load(mAudios[position].ImageUrl)
                    .Placeholder(Resource.Drawable.ChurchLogo_Gray)
                    .Error(Resource.Drawable.ChurchLogo_Gray)
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
            }
            
        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view;

            if (viewType == -100)
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.album_card2, parent, false);
                view.SetBackgroundResource(mBackground);

                return new SimpleAudioViewHolder2(view, OnClick);
            }
            else
            {
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.audio_cardview, parent, false);
                view.SetBackgroundResource(mBackground);

                return new SimpleAudioViewHolder(view, OnClick);
            }
        }

        public override int GetItemViewType(int position)
        {
            
            if (selectedTab.SelectedTabPosition == 2 || selectedTab.SelectedTabPosition == 3)
            {
                return -100;
            }
            else
            {
                return base.GetItemViewType(position);
            }
            
        }

        public class SimpleAudioViewHolder : RecyclerView.ViewHolder
        {
            public View mMainAudioView { get; set; }
            public TextView mAudioTitle { get; set; }
            public TextView mAudioSubTitle { get; set; }
            public TextView mPubDate { get; set; }
            public ImageView mAlbumArt { get; set; }
            public ImageButton mOptions { get; set; }
            public TextView mPlayed {get; set;}

            public SimpleAudioViewHolder(View view, Action<int> listener)
                : base(view)
            {
                mMainAudioView = view;
                mAudioTitle = view.FindViewById<TextView>(Resource.Id.txtRow1);
                mAudioSubTitle = view.FindViewById<TextView>(Resource.Id.txtRow2);
                mPubDate = view.FindViewById<TextView>(Resource.Id.txtRow3);
                mAlbumArt = view.FindViewById<ImageView>(Resource.Id.avatar);
                mOptions = view.FindViewById<ImageButton>(Resource.Id.img_options);
                mPlayed = view.FindViewById<TextView>(Resource.Id.txtPlayed);
                mMainAudioView.Click += (sender, e) => listener(AdapterPosition);
            }
        }

        public class SimpleAudioViewHolder2 : RecyclerView.ViewHolder
        {
            public View mMainAudioView { get; set; }
            public TextView mAudioTitle { get; set; }
            public TextView mAudioSubtitle { get; set; }
            public ImageView mAlbumArt { get; set; }
            public ImageButton mOptions { get; set; }

            public SimpleAudioViewHolder2(View view, Action<int> listener)
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

        public Filter Filter { get; private set; }
    }

    public class AudioFilter : Filter
    {
        private readonly AudioRecyclerViewAdapter _adapter;
        public AudioFilter(AudioRecyclerViewAdapter adapter)
        {
            _adapter = adapter;
        }

        protected override FilterResults PerformFiltering(ICharSequence constraint)
        {
            var returnObj = new FilterResults();
            var results = new List<AudioList>();
            if (_adapter.mFilterAudios == null)
                _adapter.mFilterAudios = _adapter.mAudios;

            if (constraint == null) return returnObj;

            if(_adapter.mFilterAudios != null && _adapter.mFilterAudios.Any())
            {
                try
                {
                    // Compare constraint to all names lowercased. 
                    // It they are contained they are added to results.
                    results.AddRange(
                        _adapter.mFilterAudios.Where(
                        audio => audio.Title.ToLower().Contains(constraint.ToString())
                        || audio.SubTitle.ToLower().Contains(constraint.ToString())));
                }

                catch(ArgumentNullException)
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
                _adapter.mAudios = values.ToArray<Object>()
                    .Select(r => r.ToNetObject<AudioList>()).ToList();
            }
            catch (System.NullReferenceException)
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