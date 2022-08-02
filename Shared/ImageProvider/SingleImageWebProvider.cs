using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace ClaudiaIDE.ImageProvider
{
    internal class SingleImageWebProvider : IImageProvider
    {
        Setting _setting;

        BitmapImage _image;

        public SingleImageWebProvider(Setting setting, string solutionfile = null)
        {
            _setting = setting;
            _setting.OnChanged.AddEventHandler(ReloadSettings);
        }

        private void ReloadSettings(object sender, EventArgs e)
        {
            ImageDownloader.ResetUrl();
        }

        public ImageBackgroundType ProviderType => ImageBackgroundType.WebSingle;

        public string SolutionConfigFile {get; private set;}

        public event EventHandler NewImageAvaliable;

        public BitmapSource GetBitmap()
        {
            if(_image != null) return _image;
            var image = ImageDownloader.LoadImage(_setting.WebSingleUrl, _setting.ImageStretch, _setting.MaxWidth, _setting.MaxHeight);
            image.ContinueWith(OnDownloadComplete, TaskContinuationOptions.OnlyOnRanToCompletion);
         
            return null;
        }

        private void OnDownloadComplete(Task<BitmapImage> obj)
        {
            if(obj.IsCompleted)
            {
                _image = obj.Result;
                NewImageAvaliable.Invoke(this, EventArgs.Empty);
            }
        }
    }
}
