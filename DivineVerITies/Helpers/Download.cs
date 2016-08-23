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
using System.Threading;
using System.Net;
using System.Threading.Tasks;
using System.IO;

namespace DivineVerITies.Helpers
{
    public class Download
    {
        static Context mContext;
        public static async Task<int> CreateDownloadTask(string urlToDownload,
            string fullPath, IProgress<DownloadBytesProgress> progressReporter, Context context)
        {
            mContext = context;
                WebClient client = new WebClient();
                MyService.cts.Token.Register(client.CancelAsync);
                int receivedBytes = 0;
                int totalBytes = 0;
                List<byte> file = new List<byte>();

            try
            {
                using (var stream = await client.OpenReadTaskAsync(urlToDownload))
                {
                    byte[] buffer = new byte[4096];
                    totalBytes = Int32.Parse(client.ResponseHeaders[HttpRequestHeader.ContentLength]);
                    if (File.Exists(fullPath))
                    {
                        var builder = new Android.Support.V7.App.AlertDialog.Builder(context);
                        builder.SetTitle("File already exists")
                            .SetMessage("The podcast you are trying to download already exists on your device.\nWould you like to overwrite it?")
                            .SetPositiveButton("Yes", delegate { builder.Dispose(); }).SetNegativeButton("No", delegate { client.CancelAsync();
                                //put notification changing code here
                                builder.Dispose(); });
                        builder.Create().Show();
                    }
                    var filestream = File.Create(fullPath, totalBytes);
                    for (; ; )
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
                            DownloadBytesProgress args = new DownloadBytesProgress(urlToDownload, receivedBytes, totalBytes);
                            progressReporter.Report(args);
                        }
                    }
                    filestream.Write(file.ToArray(), 0, (file.ToArray()).Length);
                    filestream.Close();
                }
            }
            catch (ObjectDisposedException e)
            {
                CreateAndShowDownloadError("Podcast Download Was Cancelled By User", "Download Cancelled");
            }

            catch(Exception b)
            {
                CreateAndShowDownloadError(b, "Download Error");
            }
            
            return receivedBytes;
           
        }

        private static void CreateAndShowDownloadError(Exception e, string title)
        {
            CreateAndShowDownloadError(e.Message, title);
        }

        private static void CreateAndShowDownloadError(string message, string title)
        {
            Android.Support.V7.App.AlertDialog.Builder builder = new Android.Support.V7.App.AlertDialog.Builder(mContext);

            builder.SetMessage(message);
            builder.SetTitle(title);
            builder.Create().Show();
        }
    }
}