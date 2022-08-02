using ClaudiaIDE.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ClaudiaIDE.Helpers
{
    internal class ImageDownloader
    {
        private static string lastDownloaded = "";
        private static BitmapImage _image;

        public static async Task<BitmapImage> LoadImage(string url, ImageStretch imageStretch, int maxWidth, int maxHeight)
        {
            if(string.IsNullOrEmpty(url)) return null;
            if(lastDownloaded.Equals(url)) return _image;
            using(var client = new WebClient())
            {
                lastDownloaded = url;
                try
                {
                    var data =await client.DownloadDataTaskAsync(url);
                    if(data != null)
                    {
                        _image = new BitmapImage();
                        _image.BeginInit();
                        _image.StreamSource =new MemoryStream(data);
                        _image.EndInit();
                        _image.Freeze();

                        if(imageStretch == ImageStretch.None)
                        {
                            _image = Utils.EnsureMaxWidthHeight(_image, maxWidth, maxHeight);
                        }
                        return _image;
                    }
                    return null;
                }
                catch(Exception ex)
                {
                    return null;
                }
            }
        }

        internal static void ResetUrl()
        {
            lastDownloaded = "";
        }
    }
}
