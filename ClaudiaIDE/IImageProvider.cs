using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text.Editor;

namespace ClaudiaIDE
{
    public interface IImageProvider
    {
        BitmapImage GetBitmap(IWpfTextView provider);
        event EventHandler NewImageAvaliable;
    }
}