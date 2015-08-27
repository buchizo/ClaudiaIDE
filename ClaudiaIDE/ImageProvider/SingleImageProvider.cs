using System;
using System.IO;
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
            var fileUri = new Uri(setting.BackgroundImageAbsolutePath, UriKind.Absolute);
            var fileInfo = new FileInfo(fileUri.AbsolutePath);
            _bitmap = new BitmapImage();
            if(fileInfo.Exists)
            {
                _bitmap.BeginInit();
                _bitmap.UriSource = fileUri;
                _bitmap.EndInit();
            }
        }

        public BitmapImage GetBitmap(IWpfTextView provider)
        {
            return _bitmap;
        }

        public event EventHandler NewImageAvaliable;
    }
}
