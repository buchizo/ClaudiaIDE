
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ClaudiaIDE.Helpers
{
    static class Utils
    {
        public static BitmapImage EnsureMaxWidthHeight(BitmapImage original, int maxWidth, int maxHeight)
        {
            BitmapImage bitmap = null;

            if (maxWidth > 0 && maxHeight > 0
                && original.PixelWidth > maxWidth && original.PixelHeight > maxHeight)
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.None;
                bitmap.UriSource = original.UriSource;
                bitmap.DecodePixelWidth = maxWidth;
                bitmap.DecodePixelHeight = maxHeight;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            else if (maxWidth > 0 && original.PixelWidth > maxWidth)
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.None;
                bitmap.UriSource = original.UriSource;
                bitmap.DecodePixelWidth = maxWidth;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            else if (maxHeight > 0 && original.PixelHeight > maxHeight)
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.CreateOptions = BitmapCreateOptions.None;
                bitmap.UriSource = original.UriSource;
                bitmap.DecodePixelHeight = maxHeight;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            else
            {
                return original;
            }
        }

        public static BitmapSource ConvertToDpi96(BitmapSource source)
        {
            var dpi = 96;
            var width = source.PixelWidth;
            var height = source.PixelHeight;

            var stride = width * 4;
            var pixelData = new byte[stride * height];
            source.CopyPixels(pixelData, stride, 0);

            return BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixelData, stride);
        }

        static byte[] pixelByteArray;
        public static BitmapSource SoftenEdges(BitmapSource original, int softedgex, int softedgey)
        {
            if (softedgex <= 0 && softedgey <= 0)
                return original;

            try
            {
                //System.Windows.Forms.MessageBox.Show(string.Format("SoftenEdges,soft={0},bpp={1}", softedge, original.Format.BitsPerPixel));

                //32bit assumption
                if (original.Format.BitsPerPixel != 32)
                {
                    return original;
                }

                //limit softedge range by half image size
                softedgex = Math.Min(softedgex, (int)(original.Width / 2));
                softedgey = Math.Min(softedgey, (int)(original.Height / 2));

                int height = original.PixelHeight;
                int width = original.PixelWidth;
                int bytesPerPixel = (original.Format.BitsPerPixel + 7) / 8;
                int nStride = width * bytesPerPixel;


                pixelByteArray = new byte[height * nStride];
                original.CopyPixels(pixelByteArray, nStride, 0);

                //alpha and color
                for (int y = 0; y < height; y++)
                {
                    float fAlphaByY = 1f;
                    int nDistToEdgeY = Math.Min(y, height - 1 - y);
                    fAlphaByY = (float)nDistToEdgeY / softedgey;
                    fAlphaByY = Math.Min(fAlphaByY, 1.0f);

                    for (int x = 0; x < width; x++)
                    {
                        float fAlphaByX = 1f;
                        int nDistToEdgeX = Math.Min(x, width - 1 - x);
                        fAlphaByX = (float)nDistToEdgeX / softedgex;
                        fAlphaByX = Math.Min(fAlphaByX, 1.0f);

                        for (int iPix = 0; iPix < 4; iPix++)
                        {
                            int alpha_offset_in_array = bytesPerPixel * (x + y * width) + iPix;
                            int alphaOld = (int)pixelByteArray[alpha_offset_in_array];
                            int alphaNew = (int)Math.Floor(alphaOld * fAlphaByX * fAlphaByY);
                            pixelByteArray[alpha_offset_in_array] = (byte)alphaNew;
                        }
                    }
                }

                WriteableBitmap newbm = new WriteableBitmap(width, height, original.DpiX, original.DpiY, PixelFormats.Pbgra32, null);
                newbm.WritePixels(new System.Windows.Int32Rect(0, 0, width, height), pixelByteArray, nStride, 0);

                return newbm;
            }
            catch (System.Exception exp)
            {
                System.Windows.Forms.MessageBox.Show(exp.ToString() + exp.StackTrace);
            }

            return original;
        }

    }
}
