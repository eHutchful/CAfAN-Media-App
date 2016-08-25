using Android.Graphics;
using System.Collections.Generic;

namespace DivineVerITies.Helpers
{
    class Topics
    {
        public static List<string> TopicStrings
        {
            get
            {
                return new List<string>()
                {
                    "Faith", "Overcoming Temptations", "The Trinity", "Grace", "The Doctrine of Life", "The Spirit Life", "Apologetics",
                    "Money", "Love", "Purpose", "Relationships and Marriage", "The Gospel"
                };
            }
        }

        public static int CalculateInSampleSize(BitmapFactory.Options options, int reqWidth, int reqHeight)
        {
            // Raw height and width of image
            int height = options.OutHeight;
            int width = options.OutWidth;
            int inSampleSize = 1;

            if (height > reqHeight || width > reqWidth)
            {

                // Calculate ratios of height and width to requested height and
                // width
                int heightRatio = height / reqHeight;
                int widthRatio = width / reqWidth;

                // Choose the smallest ratio as inSampleSize value, this will
                // guarantee
                // a final image with both dimensions larger than or equal to the
                // requested height and width.
                inSampleSize = heightRatio < widthRatio ? heightRatio : widthRatio;
            }

            return inSampleSize;
        }
    }
}