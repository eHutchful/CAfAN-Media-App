using Android.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace DivineVerITies.Helpers
{
    public class Download
    {

        Context mContext;
        public CancellationTokenSource cts = new CancellationTokenSource();
        public  async void CreateDownloadTask(string urlToDownload,
            string fullPath, IProgress<DownloadBytesProgress> progressReporter, Context context)
        {
            
            mContext = context;
                WebClient client = new WebClient();
            
                cts.Token.Register(client.CancelAsync);
                int receivedBytes = 0;
                int totalBytes = 0;
                List<byte> file = new List<byte>();

            try
            {
                
                  
                var stream = await client.OpenReadTaskAsync(urlToDownload);
                
                    byte[] buffer = new byte[40960];
                    totalBytes = Int32.Parse(client.ResponseHeaders[HttpRequestHeader.ContentLength]);                   
                    var filestream = File.Create(fullPath, totalBytes);
                    for (; ; )
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);
                        file.AddRange(buffer);
                        if (bytesRead == 0)
                        {
                            await Task.Yield();
                            break;
                        }
                        filestream.Write(buffer, 0, bytesRead);
                        receivedBytes += bytesRead;
                        if (progressReporter != null)
                        {
                            DownloadBytesProgress args = new DownloadBytesProgress(urlToDownload, receivedBytes, totalBytes);
                            progressReporter.Report(args);
                        }
                    }
                stream.Close();
                filestream.Close();
                    
                }
            
            catch (ObjectDisposedException e)
            {
                CreateAndShowDownloadError("Podcast Download Was Cancelled By User", "Download Cancelled",mContext);
            }

            catch(Exception b)
            {
                CreateAndShowDownloadError(b, "Download Error",mContext);
                
            }
        }
       
        private static void CreateAndShowDownloadError(Exception e, string title,Context mContext)
        {
            CreateAndShowDownloadError(e.Message, title,mContext);
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