using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;

namespace ClaudiaIDE.Helpers
{
    internal class ImageDownloader
    {
        private static HttpClient _client = new HttpClient();
        private static string lastDownloaded = "";
        private static BitmapImage _image;
        private static bool loading;

        public static async Task<BitmapSource> LoadImageAsync(string url, ImageStretch imageStretch, int maxWidth, int maxHeight, Setting setting)
        {
            if (string.IsNullOrEmpty(url)) return null;
            if (loading) return null;

            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(setting.WebApiDownloadInterval);
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(url));
                request.Headers.Add("ContentType", "application/json; charset=utf-8");
                var httpres = await _client.SendAsync(request, cts.Token);
                if (!httpres.IsSuccessStatusCode) return null;

                using (var memoryStream = await httpres.Content.ReadAsStreamAsync())
                {
                    _image = new BitmapImage();
                    _image.BeginInit();
                    _image.StreamSource = memoryStream;
                    _image.StreamSource.Position = 0;
                    _image.EndInit();
                    _image.Freeze();

                    if (imageStretch == ImageStretch.None)
                    {
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
                    }

                    BitmapSource ret_bitmap = null;
                    if ((_image.Width != _image.PixelWidth || _image.Height != _image.PixelHeight))
                        ret_bitmap = Utils.ConvertToDpi96(_image);

                    if (setting.SoftEdgeX > 0 || setting.SoftEdgeY > 0)
                        ret_bitmap = Utils.SoftenEdges(_image, setting.SoftEdgeX, setting.SoftEdgeY);

                    loading = false;
                    return ret_bitmap != null ? ret_bitmap : _image;
                }
            }
            catch
            {
                return null;
            }
        }

        internal static void ResetUrl()
        {
            lastDownloaded = "";
        }
    }
}