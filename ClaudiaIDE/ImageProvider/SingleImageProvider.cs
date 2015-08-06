using System;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;

namespace ClaudiaIDE.ImageProvider
{
    class SingleImageProvider : IImageProvider
    {
        private readonly BitmapImage _bitmap;

        public SingleImageProvider(Setting setting)
        {
            _bitmap = new BitmapImage();
            _bitmap.BeginInit();
            _bitmap.UriSource = new Uri(setting.BackgroundImageAbsolutePath, UriKind.Absolute);
            _bitmap.EndInit();
        }

        public BitmapImage GetBitmap(IWpfTextView provider)
        {
            return _bitmap;
        }

        public event EventHandler NewImageAvaliable;
    }
}
