using System;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;

namespace ClaudiaIDE.ImageProviders
{
    public class SingleImageProvider : ImageProvider
    {
        private BitmapImage _bitmap;
        private BitmapSource _bitmapSource;

        public SingleImageProvider(Setting setting, string solutionfile = null) : base(setting, solutionfile,
            ImageBackgroundType.Single)
        {
            LoadImage();
        }

        protected override void OnSettingChanged(object sender, EventArgs e)
        {
            if ((sender as Setting)?.ImageBackgroundType != ImageBackgroundType.Single) return;
            LoadImage();
            FireImageAvailable();
        }


        public override BitmapSource GetBitmap()
        {
            if (_bitmap == null && _bitmapSource != null) return _bitmapSource;
            if (_bitmap != null && _bitmapSource == null) return _bitmap;
            return _bitmap;
        }

        public override bool IsStaticImage()
        {
            if (string.IsNullOrEmpty(Setting.BackgroundImageAbsolutePath)) return true;
            var fileInfo = new FileInfo(Setting.BackgroundImageAbsolutePath);
            return !Setting.SupportVideoFileExtensions.Any(x => x == fileInfo.Extension.ToLower());
        }

        private void LoadImage()
        {
            try
            {
                var fileUri = new Uri(Setting.BackgroundImageAbsolutePath, UriKind.RelativeOrAbsolute);
                var fileInfo = new FileInfo(Setting.BackgroundImageAbsolutePath);
                if (Setting.SupportVideoFileExtensions.Any(x => x == fileInfo.Extension.ToLower())) return;
                if (fileInfo.Exists)
                {
                    _bitmapSource = null;
                    _bitmap = new BitmapImage();
                    _bitmap.BeginInit();
                    _bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    _bitmap.CreateOptions = BitmapCreateOptions.None;
                    _bitmap.UriSource = fileUri;
                    _bitmap.EndInit();
                    _bitmap.Freeze();

                    if (Setting.ImageStretch == ImageStretch.None)
                        _bitmap = Utils.EnsureMaxWidthHeight(_bitmap, Setting.MaxWidth, Setting.MaxHeight);

                    BitmapSource ret_bitmap = null;
                    if (Setting.ImageStretch == ImageStretch.None &&
                        (_bitmap.Width != _bitmap.PixelWidth || _bitmap.Height != _bitmap.PixelHeight)
                       )
                        ret_bitmap = Utils.ConvertToDpi96(_bitmap);

                    if (Setting.BlurRadius > 0)
                        ret_bitmap = Utils.Blur(ret_bitmap ?? _bitmap, Setting.BlurRadius);

                    if (Setting.SoftEdgeX > 0 || Setting.SoftEdgeY > 0)
                        ret_bitmap = Utils.SoftenEdges(ret_bitmap ?? _bitmap, Setting.SoftEdgeX, Setting.SoftEdgeY);

                    if (ret_bitmap != null)
                    {
                        _bitmapSource = ret_bitmap;
                        _bitmap = null;
                    }
                }
                else
                {
                    _bitmap = null;
                    _bitmapSource = null;
                }
            }
            catch { }
        }

        public override string GetCurrentImageUri()
        {
            return Setting.BackgroundImageAbsolutePath;
        }
    }
}