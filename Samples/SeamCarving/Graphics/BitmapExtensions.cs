using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace SeamCarving
{
    public static class BitmapExtensions
    {
        public static Bitmap Resize(this Bitmap bitmap, int width, int height, InterpolationMode interpolation)
        {
            var resized = new Bitmap(width, height);

            using (var g = Graphics.FromImage(resized))
            {
                g.InterpolationMode = interpolation;
                g.DrawImage(bitmap, new Rectangle(0, 0, width, height), new Rectangle(0, 0, bitmap.Width, bitmap.Height), GraphicsUnit.Pixel);
            }

            return resized;
        }

        /// <summary>
        /// Copys one of the ARGB channels from source bitmap to one of the ARGB channels of dest bitmap.
        /// </summary>
        /// <param name="channelSource">Bitmap with a source ARGB channel.</param>
        /// <param name="dest">Destination bitmap.</param>
        /// <param name="sourceChannel">Number of a source channel to copy. 3 - Alpha.</param>
        /// <param name="destChannel">Number of a dest channel. 3 - Alpha.</param>
        public static void CopyChannel(this Bitmap dest, int destChannel, Bitmap channelSource, int sourceChannel)
        {
            if (channelSource.Size != dest.Size)
                throw new ArgumentException();
            Rectangle r = new Rectangle(Point.Empty, channelSource.Size);
            BitmapData bdSrc = channelSource.LockBits(r, ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData bdDst = dest.LockBits(r, ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            unsafe
            {
                byte* bpSrc = (byte*)bdSrc.Scan0.ToPointer();
                byte* bpDst = (byte*)bdDst.Scan0.ToPointer();
                bpSrc += (int)sourceChannel;
                bpDst += (int)destChannel;
                for (int i = r.Height * r.Width; i > 0; i--)
                {
                    *bpDst = *bpSrc;
                    bpSrc += 4;
                    bpDst += 4;
                }
            }
            channelSource.UnlockBits(bdSrc);
            dest.UnlockBits(bdDst);
        }
    }
}
