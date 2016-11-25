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
using System.Threading.Tasks;
using System.Threading;
using System.Net.Http;
using ModernHttpClient;
using System.IO;

namespace DivineVerITies.Helpers
{
    public class VideoDownloader
    {
        Context mContext;
        public CancellationTokenSource cts = new CancellationTokenSource();
        private HttpClient client = new HttpClient(new NativeMessageHandler());
        public async Task DownloadFileAsync(string url,
            string fullPath, IProgress<DownloadBytesProgress> progressReporter, Context context)
        {
            mContext = context;
            ///Takes url, completion option(to read the response headers, and a cancellation token
            ///to register the cancellation method.
            var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cts.Token);
            ///If the requests fails raise the exception
            ///to be changed
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(string.Format("The request returned with HTTP status code {0}", response.StatusCode));
            }

            ///Check content size from the response header and then store it in total variable
            var total = response.Content.Headers.ContentLength.HasValue ? response.Content.Headers.ContentLength.Value : -1L;
            List<byte> file = new List<byte>();
            int receivedBytes = 0;
            try
            {
                var stream = await response.Content.ReadAsStreamAsync();
                byte[] buffer = new byte[4096];
                var filestream = File.Create(fullPath, Convert.ToInt32(total));
                for (;;)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    file.AddRange(buffer);
                    if (bytesRead == 0)
                    {
                        await Task.Yield();
                        break;
                    }
                    receivedBytes += bytesRead;
                    if (progressReporter != null)
                    {
                        DownloadBytesProgress args = new DownloadBytesProgress(url, receivedBytes, Convert.ToInt32(total));
                        progressReporter.Report(args);
                    }
                }
                filestream.Write(file.ToArray(), 0, (file.ToArray()).Length);
                filestream.Close();
                stream.Close();
            }
            catch (ObjectDisposedException e)
            {
                CreateAndShowDownloadError("Podcast Download Was Cancelled By User", "Download Cancelled", mContext);
            }

            catch (Exception b)
            {
                CreateAndShowDownloadError(b, "Download Error", mContext);

            }


        }
        private static void CreateAndShowDownloadError(Exception e, string title, Context mContext)
        {
            CreateAndShowDownloadError(e.Message, title, mContext);
        }

        private static void CreateAndShowDownloadError(string message, string title, Context mContext)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(mContext);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }
    }
}