using System.Windows.Media;
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
            else
            {
                return original;
            }
        }

        public static BitmapSource ConvertToDpi96(BitmapImage source)
        {
            var dpi = 96;
            var width = source.PixelWidth;
            var height = source.PixelHeight;

            var stride = width * 4;
            var pixelData = new byte[stride * height];
            source.CopyPixels(pixelData, stride, 0);

            return BitmapSource.Create(width, height, dpi, dpi, PixelFormats.Bgra32, null, pixelData, stride);
        }
    }
}
