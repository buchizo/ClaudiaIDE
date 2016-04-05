using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ClaudiaIDE.Helpers
{
    static class Utils
    {
        public static BitmapImage EnsureMaxWidthHeight(BitmapImage original, int maxWidth, int maxHeight)
        {
            BitmapImage bitmap = null;

            if(maxWidth > 0 && maxHeight > 0
                && original.PixelWidth > maxWidth && original.PixelHeight > maxHeight)
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = original.UriSource;
                bitmap.DecodePixelWidth = maxWidth;
                bitmap.DecodePixelHeight = maxHeight;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            else if(maxWidth > 0 && original.PixelWidth > maxWidth)
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = original.UriSource;
                bitmap.DecodePixelWidth = maxWidth;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            else if(maxHeight > 0 && original.PixelHeight > maxHeight)
            {
                bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = original.UriSource;
                bitmap.DecodePixelHeight = maxHeight;
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }

            return original;
        }
    }
}
