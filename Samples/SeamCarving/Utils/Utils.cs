using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SeamCarving
{
    public static class Utils
    {
        public static Bitmap ConvertToBitmap(byte[][] markupData, Color emptyAreaColor, Color markedAreaColor)
        {
            int width = markupData[0].Length;
            int height = markupData.Length;
            var b = new Bitmap(width, height, PixelFormat.Format8bppIndexed);

            ColorPalette ncp = b.Palette;
            ncp.Entries[0] = emptyAreaColor;
            //for (int i = 1; i < 256; i++)
            ncp.Entries[1] = markedAreaColor;
            b.Palette = ncp;

            var boundsRect = new Rectangle(0, 0, width, height);
            BitmapData bmpData = b.LockBits(boundsRect, ImageLockMode.WriteOnly, b.PixelFormat);

            IntPtr ptr = bmpData.Scan0;

            byte[] extra = new byte[bmpData.Stride - width];
            for (int i = 0; i < markupData.Length; i++)
            {
                Marshal.Copy(markupData[i], 0, ptr, width);
                ptr += width;
                Marshal.Copy(extra, 0, ptr, extra.Length);
                ptr += extra.Length;
            }

            b.UnlockBits(bmpData);
            return b;
        }
    }
}