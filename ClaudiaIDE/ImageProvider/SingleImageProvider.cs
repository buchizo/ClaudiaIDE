using System;
using System.IO;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;

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
            var fileUri = new Uri(_setting.BackgroundImageAbsolutePath, UriKind.Absolute);
            var fileInfo = new FileInfo(fileUri.AbsolutePath);
            _bitmap = new BitmapImage();
            if (fileInfo.Exists)
            {
                _bitmap.BeginInit();
                _bitmap.UriSource = fileUri;
                _bitmap.EndInit();
            }
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
