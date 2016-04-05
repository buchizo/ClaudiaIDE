using System;
using System.IO;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;
using ClaudiaIDE.Helpers;

namespace ClaudiaIDE.ImageProvider
{
    public class SingleImageProvider : IImageProvider
    {
        private BitmapImage _bitmap;
        private Setting _setting;

        public SingleImageProvider(Setting setting)
        {
            _setting = setting;
            LoadImage();
        }

        public BitmapImage GetBitmap(IWpfTextView provider)
        {
            return _bitmap;
        }

        public void ReloadSettings()
        {
            LoadImage();
        }

        private void LoadImage()
        {
            var fileUri = new Uri(_setting.BackgroundImageAbsolutePath, UriKind.RelativeOrAbsolute);
            var fileInfo = new FileInfo(_setting.BackgroundImageAbsolutePath);
            _bitmap = new BitmapImage();
            if (fileInfo.Exists)
            {
                _bitmap.BeginInit();
                _bitmap.CacheOption = BitmapCacheOption.OnLoad;
                _bitmap.UriSource = fileUri;
                _bitmap.EndInit();
                _bitmap.Freeze();
            }

            _bitmap = Utils.EnsureMaxWidthHeight(_bitmap, _setting.MaxWidth, _setting.MaxHeight);
        }

        public event EventHandler NewImageAvaliable;

        public ImageBackgroundType ProviderType
        {
            get
            {
                return ImageBackgroundType.Single;
            }
        }
    }
}
