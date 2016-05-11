using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;

namespace ClaudiaIDE.Helpers
{
    static class Utils
    {
        public static BitmapImage EnsureMaxWidthHeight(BitmapImage original, int maxWidth, int maxHeight, Settings.ZoomType zoomType)
        {
            if(maxWidth > 0 && maxHeight > 0
                && original.PixelWidth > maxWidth && original.PixelHeight > maxHeight)
            {
				return ResizeBoth(original, maxWidth, maxHeight, zoomType);
            }
            else if(maxWidth > 0 && original.PixelWidth > maxWidth)
            {
				return ResizeHorizontal(original, maxWidth, maxHeight, zoomType);
            }
            else if(maxHeight > 0 && original.PixelHeight > maxHeight)
            {
				return ResizeVertical(original, maxWidth, maxHeight, zoomType);
            }

            return original;
        }

		private static BitmapImage ResizeVertical(BitmapImage original, int maxWidth, int maxHeight, ZoomType zoomType) {
			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.UriSource = original.UriSource;

			if (zoomType == ZoomType.Zoom) 
			{
				float ratio = (float)maxHeight / (float)original.PixelHeight;
				bitmap.DecodePixelWidth = (int)((float)original.PixelWidth * ratio);
				bitmap.DecodePixelHeight = (int)((float)original.PixelHeight * ratio);
			}
			else if (zoomType == ZoomType.Stretch)
			{
				bitmap.DecodePixelHeight = maxHeight;
			}

			bitmap.EndInit();
			bitmap.Freeze();
			return bitmap;
		}

		private static BitmapImage ResizeHorizontal(BitmapImage original, int maxWidth, int maxHeight, ZoomType zoomType) {
			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.UriSource = original.UriSource;

			if (zoomType == ZoomType.Zoom) 
			{
				float ratio = (float)maxWidth / (float)original.PixelWidth;

				bitmap.DecodePixelWidth = (int)((float)original.PixelWidth * ratio);
				bitmap.DecodePixelHeight = (int)((float)original.PixelHeight * ratio);
			}
			else if (zoomType == ZoomType.Stretch) 
			{
				bitmap.DecodePixelWidth = maxWidth;
			}

			bitmap.EndInit();
			bitmap.Freeze();
			return bitmap;
		}

		private static BitmapImage ResizeBoth(BitmapImage original, int maxWidth, int maxHeight, ZoomType zoomType) {
			BitmapImage bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.UriSource = original.UriSource;

			if (zoomType == ZoomType.Zoom) 
			{
				float hRatio = (float)maxWidth / (float)original.PixelWidth;
				float vRatio = (float)maxHeight / (float)original.PixelHeight;

				float ratio = Math.Min(hRatio, vRatio);
				bitmap.DecodePixelWidth = (int)((float)original.PixelWidth * ratio);
				bitmap.DecodePixelHeight = (int)((float)original.PixelHeight * ratio);
			}
			else if (zoomType == ZoomType.Stretch) 
			{
				bitmap.DecodePixelWidth = maxWidth;
				bitmap.DecodePixelHeight = maxHeight;
			}

			bitmap.EndInit();
			bitmap.Freeze();
			return bitmap;
		}
	}
}
