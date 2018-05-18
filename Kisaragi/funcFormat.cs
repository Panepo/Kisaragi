using System;
using System.IO;
using System.Windows;
using System.Drawing;
using System.Windows.Media.Imaging;
using Emgu.CV;
using System.Runtime.InteropServices;

namespace funcFormat
{
    class formatTrans
    {
        static public Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage)
        {
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                Bitmap bitmap = new Bitmap(outStream);

                return new Bitmap(bitmap);
            }
        }

        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);
        static public BitmapSource ToBitmapSource(IImage image)
        {
            using (Bitmap source = image.Bitmap)
            {
                IntPtr ptr = source.GetHbitmap();

                BitmapSource bs = System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                    ptr,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                DeleteObject(ptr);
                return bs;
            }
        }
    }
}
