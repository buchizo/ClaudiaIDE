using System;
using System.IO;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;

namespace ClaudiaIDE.ImageProviders
{
    public class SingleImageProvider : ImageProvider
    {
        private BitmapImage _bitmap;

        public SingleImageProvider(Setting setting, string solutionfile = null) : base(setting, solutionfile,
            ImageBackgroundType.Single)
        {
            LoadImage();
        }

        protected override void OnSettingChanged(object sender, EventArgs e)
        {
            LoadImage();
            FireImageAvailable();
        }


        public override BitmapSource GetBitmap()
        {
            BitmapSource ret_bitmap = _bitmap;
            if (Setting.ImageStretch == ImageStretch.None &&
                (_bitmap.Width != _bitmap.PixelWidth || _bitmap.Height != _bitmap.PixelHeight)
               )
                ret_bitmap = Utils.ConvertToDpi96(_bitmap);

            if (Setting.SoftEdgeX > 0 || Setting.SoftEdgeY > 0)
                ret_bitmap = Utils.SoftenEdges(ret_bitmap, Setting.SoftEdgeX, Setting.SoftEdgeY);

            return ret_bitmap;
        }

        private void LoadImage()
        {
            var fileUri = new Uri(Setting.BackgroundImageAbsolutePath, UriKind.RelativeOrAbsolute);
            var fileInfo = new FileInfo(Setting.BackgroundImageAbsolutePath);

            if (fileInfo.Exists)
            {
                _bitmap = new BitmapImage();
                _bitmap.BeginInit();
                _bitmap.CacheOption = BitmapCacheOption.OnLoad;
                _bitmap.CreateOptions = BitmapCreateOptions.None;
                _bitmap.UriSource = fileUri;
                _bitmap.EndInit();
                _bitmap.Freeze();

                if (Setting.ImageStretch == ImageStretch.None)
                    _bitmap = Utils.EnsureMaxWidthHeight(_bitmap, Setting.MaxWidth, Setting.MaxHeight);
            }
            else
            {
                _bitmap = null;
            }
        }
    }
}