using System;
using System.Windows.Controls;
using Microsoft.VisualStudio.Text.Editor;

namespace ClaudiaIDE
{
    public interface IImageProvider
    {
        Image GetImage(IWpfTextView provider);
        event EventHandler NewImageAvaliable;
    }
}