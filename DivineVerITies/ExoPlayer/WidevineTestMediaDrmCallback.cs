using Android.Media;
using Com.Google.Android.Exoplayer.Drm;
using Com.Google.Android.Exoplayer.Util;
using Java.Lang;
using Java.Util;

namespace DivineVerITies.ExoPlayer
{
    /// <summary>
    /// A <see cref="IMediaDrmCallback"/> for Widevine test content.
    /// </summary>
    class WidevineTestMediaDrmCallback : Object, IMediaDrmCallback
    {
        private static readonly string WidevineGtsDefaultBaseUri =
            "http://wv-staging-proxy.appspot.com/proxy?provider=YouTube&video_id=";

        private readonly string _defaultUri;

        public WidevineTestMediaDrmCallback(string videoId)
        {
            _defaultUri = WidevineGtsDefaultBaseUri + videoId;
        }

        public byte[] ExecuteProvisionRequest(UUID uuid, MediaDrm.ProvisionRequest request)
        {
            var url = request.DefaultUrl + "&signedRequest=" + System.Text.Encoding.ASCII.GetString(request.GetData());
            return ExoPlayerUtil.ExecutePost(url, null, null);
        }

        public byte[] ExecuteKeyRequest(UUID uuid, MediaDrm.KeyRequest request)
        {
            var url = request.DefaultUrl;
            if (string.IsNullOrEmpty(url))
            {
                url = _defaultUri;
            }
            return ExoPlayerUtil.ExecutePost(url, request.GetData(), null);
        }
    }
}