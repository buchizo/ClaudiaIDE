using System;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text.Editor;
using ClaudiaIDE.Settings;

namespace ClaudiaIDE
{
    public interface IImageProvider
    {
        BitmapSource GetBitmap();
        event EventHandler NewImageAvaliable;
        ImageBackgroundType ProviderType { get; }
        void ReloadSettings();
    }

}