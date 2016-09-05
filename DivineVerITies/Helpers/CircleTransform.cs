using Android.Graphics;
using Com.Bumptech.Glide.Load.Engine.Bitmap_recycle;
using Com.Bumptech.Glide.Load.Resource.Bitmap;
using System;

namespace DivineVerITies.Helpers
{
    public class CircleTransform : BitmapTransformation
    {
        public CircleTransform(Android.Content.Context context)
            : base(context)
        {
        }

        public override string Id
        {
            get
            {
                return "CircleTransform";
            }
        }

        protected override Bitmap Transform(IBitmapPool bitmapPool, Bitmap source, int outWidth, int outHeight)
        {
            int size = Math.Min(source.Width, source.Height);

            int width = (source.Width - size) / 2;
            int height = (source.Height - size) / 2;

            Bitmap squaredBitmap = Bitmap.CreateBitmap(source, width, height, size, size);
            //if (squaredBitmap != source)
            //{
            //    source.Recycle();
            //}

            Bitmap bitmap = Bitmap.CreateBitmap(size, size, Bitmap.Config.Argb8888);

            Canvas canvas = new Canvas(bitmap);
            Paint paint = new Paint();
            BitmapShader shader = new BitmapShader(squaredBitmap, BitmapShader.TileMode.Clamp,
                    BitmapShader.TileMode.Clamp);
            paint.SetShader(shader);
            paint.AntiAlias = true;

            float r = size / 2f;
            canvas.DrawCircle(r, r, r, paint);

            //squaredBitmap.Recycle();

            return BitmapResource.Obtain(bitmap, bitmapPool).Get();
        }
    }
}