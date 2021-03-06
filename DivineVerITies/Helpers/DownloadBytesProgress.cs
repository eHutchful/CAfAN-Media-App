﻿
namespace DivineVerITies.Helpers
{
    public class DownloadBytesProgress
    {
        //private string urlToDownload;
        //private int receivedBytes;
        //private int totalBytes;

        public DownloadBytesProgress(string urlToDownload, int receivedBytes, int totalBytes)
        {
            // TODO: Complete member initialization
            FileName = urlToDownload;
            BytesReceived = receivedBytes;
            TotalBytes = totalBytes;
            prevPer = 0;
        }

        public string FileName { get; set; }

        public int BytesReceived { get; set; }

        public int TotalBytes { get; set; }

        public float PercentComplete { get { return (float)BytesReceived / TotalBytes; } }
        

        public int prevPer;

        public bool IsFinished { get { return BytesReceived == TotalBytes; } }
    }
}
