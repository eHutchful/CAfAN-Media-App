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


namespace DivineVerITies
{
    public class AudioRecyclerViewAdapter : RecyclerView.Adapter, IFilterable
    {
        //bool isRecyclerVewVisible = false;
        public List<AudioList> mAudios;
        public List<AudioList> mFilterAudios;
        private readonly TypedValue mTypedValue = new TypedValue();
        private int mBackground;
        Resources mResource;
        private Context mContext;
        public event EventHandler<int> itemClick;

        public AudioRecyclerViewAdapter(Context context, List<AudioList> audios, Resources res)
        {
           // mFilterAudios = audios;
            mContext = context;
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            mBackground = mTypedValue.ResourceId;
            mAudios = audios;
            mResource = res;

            Filter = new AudioFilter(this);
        }

        public override int ItemCount
        {
            get
            {
                return mAudios.Count;
            }
        }

        public void Add(AudioList audio)
        {
            mAudios.Add(audio);
            NotifyDataSetChanged();
        }

        void OnClick(int position)
        {
            if (itemClick != null)
                itemClick(this, position);
        }

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            //if (isRecyclerVewVisible)
            //{
                var simpleHolder = holder as SimpleAudioViewHolder;
                //int indexPosition = (mAudios.Count - 1) - position;

                simpleHolder.mAudioTitle.Text = mAudios[position].Title;
                simpleHolder.mAudioSubTitle.Text = mAudios[position].SubTitle;
                simpleHolder.mPubDate.Text = mAudios[position].PubDate;

                Glide.With(simpleHolder.mAlbumArt.Context)
                    .Load(mAudios[position].ImageUrl)
                    .Placeholder(Resource.Drawable.Logo_trans192)
                    .Error(Resource.Drawable.Logo_trans192)
                    //.SkipMemoryCache(true)
                    //.Thumbnail(1)
                    .DiskCacheStrategy(DiskCacheStrategy.All)
                    .Into(simpleHolder.mAlbumArt);


                //Picasso.With(mContext)
                //       .Load(mAudios[position].ImageUrl)
                //       .Placeholder(Resource.Drawable.Logo_trans192)
                //       .Error(Resource.Drawable.Logo_trans192)
                //       //.Resize(60, 60)
                //       //.CenterCrop()
                //       .Into(simpleHolder.mAlbumArt);

                //var imageBitmap = (new Initialize()).GetImageBitmapFromUrl(mAudios[position].ImageUrl);
                //simpleHolder.mAlbumArt.SetImageBitmap(imageBitmap); 
            //}
            //else
            //{
            //    isRecyclerVewVisible = true;
            //}

        }

        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {
            View view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.audio_cardview, parent, false);
            view.SetBackgroundResource(mBackground);

            SimpleAudioViewHolder simpleHolder = new SimpleAudioViewHolder(view, OnClick);
            simpleHolder.mOptions.Click += (s, e) =>
            {
                Android.Support.V7.Widget.PopupMenu Popup = new Android.Support.V7.Widget.PopupMenu(simpleHolder.mOptions.Context, simpleHolder.mOptions);
                Popup.Inflate(Resource.Menu.menu_album);
                Popup.MenuItemClick +=  (o, args) =>
                {
                    switch (args.Item.ItemId)
                    {
                        case Resource.Id.action_add_favourite:
                            break;

                        case Resource.Id.action_play_next:
                            break;

                        case Resource.Id.action_Download:
                          
                            break;
                    }
                }; Popup.Show();
            };

            return new SimpleAudioViewHolder(view, OnClick);
        }

        public class SimpleAudioViewHolder : RecyclerView.ViewHolder
        {
            public View mMainAudioView { get; set; }
            public TextView mAudioTitle { get; set; }
            public TextView mAudioSubTitle { get; set; }
            public TextView mPubDate { get; set; }
            public ImageView mAlbumArt { get; set; }
            public ImageButton mOptions { get; set; }

            public SimpleAudioViewHolder(View view, Action<int> listener)
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

        public Filter Filter { get; private set; }

        //public void NotifyDataSetChanged()
        //{
        //    // If you are using cool stuff like sections
        //    // remember to update the indices here!
        //    base.NotifyDataSetChanged();
        //}
    }

    class AudioFilter : Filter
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

            if (_adapter.mFilterAudios != null && _adapter.mFilterAudios.Any())
            {
                // Compare constraint to all names lowercased. 
                // It they are contained they are added to results.
                results.AddRange(
                    _adapter.mFilterAudios.Where(
                        audio => audio.Title.ToLower().Contains(constraint.ToString())));
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
                _adapter.mAudios = values.ToArray<Object>()
                    .Select(r => r.ToNetObject<AudioList>()).ToList();

            _adapter.NotifyDataSetChanged();

            // Don't do this and see GREF counts rising
            constraint.Dispose();
            results.Dispose();
        }
    }
}