using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Google.Android.Exoplayer;
using Com.Google.Android.Exoplayer.Util;
using Java.Lang;
using AndroidResource = Android.Resource;

namespace DivineVerITies.ExoPlayer
{
    /// <summary>
    /// An activity for selecting from a number of samples.
    /// </summary>
    [Activity(
        //Name = "DivineVerITies.ExoPlayer.SampleChooserActivity",
        ConfigurationChanges = ConfigChanges.KeyboardHidden,
        Label = "@string/ApplicationName"
        )]
    // ReSharper disable once UnusedMember.Global
    class SampleChooserActivity : Activity
    {
        private const string Tag = "SampleChooserActivity";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.sample_chooser_activity);

            var sampleList = FindViewById<ListView>(Resource.Id.sample_list);
            var sampleAdapter = new SampleAdapter(this);

            sampleAdapter.Add(new Header("YouTube DASH"));
            // ReSharper disable CoVariantArrayConversion
            sampleAdapter.AddAll(Samples.YoutubeDashMp4);
            sampleAdapter.Add(new Header("Widevine GTS DASH"));
            sampleAdapter.AddAll(Samples.WidevineGts);
            sampleAdapter.Add(new Header("SmoothStreaming"));
            sampleAdapter.AddAll(Samples.Smoothstreaming);
            sampleAdapter.Add(new Header("HLS"));
            sampleAdapter.AddAll(Samples.Hls);
            sampleAdapter.Add(new Header("Misc"));
            sampleAdapter.AddAll(Samples.Misc);

            // Add WebM samples if the device has a VP9 decoder.
            try
            {
                if (MediaCodecUtil.GetDecoderInfo(MimeTypes.VideoVp9, false) != null)
                {
                    sampleAdapter.Add(new Header("YouTube WebM DASH (Experimental)"));
                    sampleAdapter.AddAll(Samples.YoutubeDashWebm);
                }
            }
            catch (MediaCodecUtil.DecoderQueryException e)
            {
                Log.Error(Tag, "Failed to query vp9 decoder", e);
            }
            // ReSharper restore CoVariantArrayConversion

            sampleList.Adapter = sampleAdapter;
            sampleList.ItemClick += (sender, args) =>
            {
                var item = sampleAdapter.GetItem(args.Position);
                var sample = item as Samples.Sample;
                if (sample != null)
                {
                    OnSampleSelected(sample);
                }
            };
        }

        private void OnSampleSelected(Samples.Sample sample)
        {
            var mpdIntent = new Intent(this, typeof(PlayerActivity))
                .SetData(Uri.Parse(sample.Uri))
                .PutExtra(PlayerActivity.ContentIdExtra, sample.ContentId)
                .PutExtra(PlayerActivity.ContentTypeExtra, sample.Type);
            StartActivity(mpdIntent);
        }

        private class SampleAdapter : ArrayAdapter<Object>
        {

            public SampleAdapter(Context context)
                : base(context, 0)
            {

            }

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var view = convertView;
                if (view == null)
                {
                    var layoutId = GetItemViewType(position) == 1
                        ? AndroidResource.Layout.SimpleListItem1
                        : Resource.Layout.sample_chooser_inline_header;
                    view = LayoutInflater.From(Context).Inflate(layoutId, null, false);
                }
                var item = GetItem(position);
                string name = null;
                var sample = item as Samples.Sample;
                var header = item as Header;
                if (sample != null)
                {
                    name = sample.Name;
                }
                else if (header != null)
                {
                    name = header.Name;
                }
                ((TextView)view).Text = name;
                return view;
            }

            public override int GetItemViewType(int position)
            {
                return (GetItem(position) is Samples.Sample) ? 1 : 0;
            }

            public override int ViewTypeCount
            {
                get { return 2; }
            }
        }

        private class Header : Object
        {
            public readonly string Name;

            public Header(string name)
            {
                Name = name;
            }
        }
    }
}