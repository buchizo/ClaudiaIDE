using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;

namespace ClaudiaIDE.Helpers
{
    internal class ImageDownloader
    {
        private static string lastDownloaded = "";
        private static BitmapImage _image;
        private static bool loading;

        public static async Task<BitmapImage> LoadImageAsync(string url, ImageStretch imageStretch, int maxWidth,
            int maxHeight)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (loading) return null;
            if (lastDownloaded.Equals(url)) return _image;
            using (var client = new WebClient())
            {
                lastDownloaded = url;
                try
                {
                    var data = await client.DownloadDataTaskAsync(url);
                    if (data != null)
                    {
                        var memoryStream = new MemoryStream(data);
                        _image = new BitmapImage();
                        _image.BeginInit();
                        _image.StreamSource = memoryStream;
                        _image.StreamSource.Position = 0;
                        _image.EndInit();
                        _image.Freeze();

                        if (imageStretch == ImageStretch.None)
                            if (maxHeight > 0 || maxWidth > 0)
                            {
                                var tmp = new BitmapImage();
                                tmp.BeginInit();
                                tmp.StreamSource = memoryStream;
                                memoryStream.Position = 0;
                                var modified = false;
                                if (maxHeight > 0 && _image.PixelHeight > maxHeight)
                                {
                                    if (_image.PixelHeight > _image.PixelWidth || maxWidth == 0)
                                        tmp.DecodePixelHeight = maxHeight;
                                    else
                                        tmp.DecodePixelWidth = maxWidth;
                                    modified = true;
                                }

                                if (maxWidth > 0 && _image.PixelWidth > maxHeight && !modified)
                                {
                                    if (_image.PixelWidth > _image.PixelHeight || maxHeight == 0)
                                        tmp.DecodePixelWidth = maxWidth;
                                    else
                                        tmp.DecodePixelHeight = maxHeight;
                                }

                                tmp.EndInit();
                                tmp.Freeze();
                                _image = tmp;
                            }

                        loading = false;
                        return _image;
                    }

                    return null;
                }
                catch
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