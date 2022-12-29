using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MessageBox = System.Windows.Forms.MessageBox;

namespace ClaudiaIDE.Helpers
{
    internal static class Utils
    {
        private static byte[] pixelByteArray;

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

            if (maxWidth > 0 && original.PixelWidth > maxWidth)
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

            if (maxHeight > 0 && original.PixelHeight > maxHeight)
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

            return original;
        }

        public static BitmapSource ConvertToDpi96(BitmapSource source)
        {
            var dpi = 96;
            var width = source.PixelWidth;
            var height = source.PixelHeight;

            var stride = width * 4;
            var pixelData = new byte[stride * height];
            source.CopyPixels(pixelData, stride, 0);

            var ret = BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixelData, stride);
            ret.Freeze();
            return ret;
        }

        public static BitmapSource SoftenEdges(BitmapSource original, int softedgex, int softedgey)
        {
            if (softedgex <= 0 && softedgey <= 0) return original;

            try
            {
                //32bit assumption
                if (original.Format.BitsPerPixel != 32) return original;

                //limit softedge range by half image size
                softedgex = Math.Min(softedgex, (int)(original.Width / 2));
                softedgey = Math.Min(softedgey, (int)(original.Height / 2));

                var height = original.PixelHeight;
                var width = original.PixelWidth;
                var bytesPerPixel = (original.Format.BitsPerPixel + 7) / 8;
                var nStride = width * bytesPerPixel;

                pixelByteArray = new byte[height * nStride];
                original.CopyPixels(pixelByteArray, nStride, 0);

                //alpha and color
                for (var y = 0; y < height; y++)
                {
                    var fAlphaByY = 1f;
                    var nDistToEdgeY = Math.Min(y, height - 1 - y);
                    fAlphaByY = (float)nDistToEdgeY / softedgey;
                    fAlphaByY = Math.Min(fAlphaByY, 1.0f);

                    for (var x = 0; x < width; x++)
                    {
                        var fAlphaByX = 1f;
                        var nDistToEdgeX = Math.Min(x, width - 1 - x);
                        fAlphaByX = (float)nDistToEdgeX / softedgex;
                        fAlphaByX = Math.Min(fAlphaByX, 1.0f);

                        for (var iPix = 0; iPix < 4; iPix++)
                        {
                            var alpha_offset_in_array = bytesPerPixel * (x + y * width) + iPix;
                            int alphaOld = pixelByteArray[alpha_offset_in_array];
                            var alphaNew = (int)Math.Floor(alphaOld * fAlphaByX * fAlphaByY);
                            pixelByteArray[alpha_offset_in_array] = (byte)alphaNew;
                        }
                    }
                }

                var bs = BitmapSource.Create(width,
                                        height,
                                        original.DpiX,
                                        original.DpiY,
                                        PixelFormats.Pbgra32,
                                        original.Palette,
                                        pixelByteArray,
                                        nStride);
                bs.Freeze();
                return bs;
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp + exp.StackTrace);
            }

            return original;
        }
    }
}