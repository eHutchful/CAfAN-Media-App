using Android.Content;
using Android.Content.Res;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using DivineVerITies.ExoPlayer.Player;
using System;
using System.Collections.Generic;


namespace DivineVerITies
{
    class AudioRecyclerViewAdapter : RecyclerView.Adapter
    {
        //bool isRecyclerVewVisible = false;
        List<AudioList> mAudios;
        private readonly TypedValue mTypedValue = new TypedValue();
        private int mBackground;
        Resources mResource;
        private Context mContext;
        public event EventHandler<int> itemClick;

        public AudioRecyclerViewAdapter(Context context, List<AudioList> audios, Resources res)
        {
            mContext = context;
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            mBackground = mTypedValue.ResourceId;
            mAudios = audios;
            mResource = res;
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
    }
}