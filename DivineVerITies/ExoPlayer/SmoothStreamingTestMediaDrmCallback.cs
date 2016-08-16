using System.Collections.Generic;
using Android.Media;
using Com.Google.Android.Exoplayer.Drm;
using Com.Google.Android.Exoplayer.Util;
using Java.Lang;
using Java.Util;

namespace DivineVerITies.ExoPlayer
{
    /// <summary>
    /// Demo <see cref="StreamingDrmSessionManager"/> for smooth streaming test content.
    /// </summary>
    class SmoothStreamingTestMediaDrmCallback : Object, IMediaDrmCallback
    {
        private const string PlayreadyTestDefaultUri =
            "http://playready.directtaps.net/pr/svc/rightsmanager.asmx";
        private static readonly IDictionary<string, string> KeyRequestProperties = new Dictionary<string, string>
        {
            {"Content-Type", "text/xml"},
            {"SOAPAction", "http://schemas.microsoft.com/DRM/2007/03/protocols/AcquireLicense"}
        };

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
                url = PlayreadyTestDefaultUri;
            }
            return ExoPlayerUtil.ExecutePost(url, request.GetData(), KeyRequestProperties);
        }

    }
}