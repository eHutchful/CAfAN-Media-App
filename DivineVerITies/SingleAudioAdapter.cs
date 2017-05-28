using System;

using Android.Views;
using Android.Widget;
using Android.Support.V7.Widget;
using Android.Util;
using Android.Content.Res;
using Android.Content;
using System.Collections.Generic;
using Newtonsoft.Json;
using DivineVerITies.Helpers;
using Com.Bumptech.Glide;
using Com.Bumptech.Glide.Load.Resource.Gif;
using Com.Bumptech.Glide.Load.Resource.Bitmap;

namespace DivineVerITies
{
    class SingleAudioAdapter : RecyclerView.Adapter
    {
        public event EventHandler<SingleAudioAdapterClickEventArgs> ItemClick;
        public event EventHandler<SingleAudioAdapterClickEventArgs> ItemLongClick;
        private readonly TypedValue mTypedValue = new TypedValue();
        private int mBackground;
        public List<AudioList> mAudios;
        Resources mResource;
        private Context mContext;

        public SingleAudioAdapter(Context context, List<AudioList> data, Resources res)
        {
            mContext = context;
            context.Theme.ResolveAttribute(Resource.Attribute.selectableItemBackground, mTypedValue, true);
            mBackground = mTypedValue.ResourceId;
            mResource = res;
            mAudios = data;
        }

        // Create new views (invoked by the layout manager)
        public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
        {

            View itemView = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.SingleAudio, parent, false);
            itemView.SetBackgroundResource(mBackground);

            var vh = new SingleAudioAdapterViewHolder(itemView, OnClick, OnLongClick);
            return vh;
        }

        // Replace the contents of a view (invoked by the layout manager)
        public override void OnBindViewHolder(RecyclerView.ViewHolder viewHolder, int position)
        {
            //var item = items[position];

            // Replace the contents of the view with that element
            var holder = viewHolder as SingleAudioAdapterViewHolder;
            //holder.TextView.Text = items[position];

            holder.mAudioTitle.Text = mAudios[position].Title;
            holder.mAudioAlbum.Text = mAudios[position].SubTitle;

            //Glide.With(mContext)
            //    .Load(Resource.Drawable.audio1)
            //    //.Transform(new GifDrawableTransformation(new CircleTransform(mContext), Glide.Get(mContext).BitmapPool))
            //    .Into(holder.mNowPlaying);

            holder.mOptions.Click += delegate
            {
                Android.Support.V7.Widget.PopupMenu Popup = new Android.Support.V7.Widget.PopupMenu(
                    holder.mOptions.Context, holder.mOptions);

                //Popup.Menu.Add(Menu.None, 0, Menu.None, "items").SetTitle("Play Next").SetShowAsAction(ShowAsAction.Never);
                Popup.Menu.Add(Menu.None, 1, Menu.None, "items").SetTitle("Download").SetShowAsAction(ShowAsAction.Never);
                Popup.Menu.Add(Menu.None, 2, Menu.None, "items").SetTitle("Details").SetShowAsAction(ShowAsAction.Never);

                Popup.MenuItemClick += (o, args) =>
                {
                    switch (args.Item.ItemId)
                    {
                        //case 0:
                        //    break;

                        case 1:
                            //MyService.selectedAudio = mAudios[position];
                            //MyService.contxt = mContext;
                            //var intent = new Intent(mContext, typeof(MyService));
                            //intent.SetAction(MyService.StartD);
                            //mContext.StartService(intent);


                            var checker = new FileChecker(mContext, "audio", mAudios[position]);
                            bool exists = checker.FileExists();
                            if (exists)
                            {
                                checker.CreateAndShowDialog(mAudios[position]);
                            }
                            else
                            {
                                //new DownloadTask(mContext, mAudios[position]).Execute(mAudios[position].Link);
                                var audio = mAudios[position];
                                var dwnIntent = new Intent(mContext, typeof(DownloadService));
                                dwnIntent.PutExtra("url", audio.Link);
                                dwnIntent.PutExtra("type", "audio");
                                dwnIntent.PutExtra("selectedAudio", JsonConvert.SerializeObject(audio));
                                mContext.StartService(dwnIntent);
                            }


                            break;

                        case 2:
                            string serial;
                            var intent = new Intent(mContext, typeof(PodcastDetails));
                            serial = JsonConvert.SerializeObject(mAudios[position]);
                            intent.PutExtra("selectedItem", serial);
                            mContext.StartActivity(intent);
                            break;
                    }
                }; Popup.Show();
            };
        }

        public override int ItemCount => mAudios.Count;

        void OnClick(SingleAudioAdapterClickEventArgs args) => ItemClick?.Invoke(this, args);
        void OnLongClick(SingleAudioAdapterClickEventArgs args) => ItemLongClick?.Invoke(this, args);

    }

    public class SingleAudioAdapterViewHolder : RecyclerView.ViewHolder
    {
        public readonly View mView;
        public readonly ImageView mNowPlaying;
        public readonly ImageView mOptions;
        public readonly TextView mAudioTitle;
        public readonly TextView mAudioAlbum;


        public SingleAudioAdapterViewHolder(View itemView, Action<SingleAudioAdapterClickEventArgs> clickListener,
                            Action<SingleAudioAdapterClickEventArgs> longClickListener) : base(itemView)
        {
            mView = itemView;
            mNowPlaying = itemView.FindViewById<ImageView>(Resource.Id.now_playing);
            mAudioTitle = itemView.FindViewById<TextView>(Resource.Id.txt_AudioTitle);
            mAudioAlbum = itemView.FindViewById<TextView>(Resource.Id.txt_Album);
            mOptions = itemView.FindViewById<ImageButton>(Resource.Id.img_options);
            itemView.Click += (sender, e) => clickListener(new SingleAudioAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
            itemView.LongClick += (sender, e) => longClickListener(new SingleAudioAdapterClickEventArgs { View = itemView, Position = AdapterPosition });
        }
    }

    public class SingleAudioAdapterClickEventArgs : EventArgs
    {
        public View View { get; set; }
        public int Position { get; set; }
    }
}