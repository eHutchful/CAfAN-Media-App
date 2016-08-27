using Android.Content;
using Android.Content.Res;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Engine;
using System.Collections.Generic;


namespace DivineVerITies
{
    class AudioRecyclerViewAdapter : RecyclerView.Adapter
    {
        bool isRecyclerVewVisible = false;
        List<AudioList> mAudios;
        private readonly TypedValue mTypedValue = new TypedValue();
        private int mBackground;
        Resources mResource;
        private Context mContext;

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

        public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
        {
            //if (isRecyclerVewVisible)
            //{
                var simpleHolder = holder as SimpleAudioViewHolder;
                //int indexPosition = (mAudios.Count - 1) - position;

                simpleHolder.mAudioTitle.Text = mAudios[position].Title;
                simpleHolder.mAudioSubTitle.Text = mAudios[position].SubTitle;
                simpleHolder.mPubDate.Text = mAudios[position].PubDate;

                Glide.With(mContext)
                    .Load(mAudios[position].ImageUrl)
                    .Placeholder(Resource.Drawable.Logo_trans192)
                    .Error(Resource.Drawable.Logo_trans192)
                    //.SkipMemoryCache(true)
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

            return new SimpleAudioViewHolder(view);
        }

        public class SimpleAudioViewHolder : RecyclerView.ViewHolder
        {
            public View mMainAudioView { get; set; }
            public TextView mAudioTitle { get; set; }
            public TextView mAudioSubTitle { get; set; }
            public TextView mPubDate { get; set; }
            public ImageView mAlbumArt { get; set; }
            public ImageButton mOptions { get; set; }

            public SimpleAudioViewHolder(View view)
                : base(view)
            {
                mMainAudioView = view;
                mAudioTitle = view.FindViewById<TextView>(Resource.Id.txtRow1);
                mAudioSubTitle = view.FindViewById<TextView>(Resource.Id.txtRow2);
                mPubDate = view.FindViewById<TextView>(Resource.Id.txtRow3);
                mAlbumArt = view.FindViewById<ImageView>(Resource.Id.avatar);
                mOptions = view.FindViewById<ImageButton>(Resource.Id.img_options);
            }
        }
    }
}