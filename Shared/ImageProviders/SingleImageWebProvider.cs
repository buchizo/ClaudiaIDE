using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Threading;

namespace ClaudiaIDE.ImageProviders
{
    internal class SingleImageWebProvider : ImageProvider
    {
        private string _currentUrl;

        public SingleImageWebProvider(Setting setting, string solutionfile = null) : base(setting, solutionfile,
            ImageBackgroundType.WebSingle)
        {
        }

        protected override void OnSettingChanged(object sender, EventArgs e)
        {
            if ((sender as Setting)?.ImageBackgroundType != ImageBackgroundType.WebSingle) return;
            ImageDownloader.ResetUrl();
            Image = null;
        }

        public override BitmapSource GetBitmap()
        {
            if (Image != null) return Image;
            LoadImageFromWebAsync().Forget();
            return null;
        }

        private async Task LoadImageFromWebAsync()
        {
            try
            {
                _currentUrl = Setting.WebSingleUrl;
                if (!IsStaticImage()) return;
                Image = await ImageDownloader.LoadImageAsync(Setting.WebSingleUrl, Setting.ImageStretch, Setting.MaxWidth, Setting.MaxHeight, Setting);
                if (Image != null) FireImageAvailable();
            }
            catch { }
        }

        public override bool IsStaticImage()
        {
            if (string.IsNullOrEmpty(_currentUrl)) return true;
            var f = new FileInfo(new Uri(_currentUrl).LocalPath);
            return !Setting.SupportVideoFileExtensions.Any(x => x == f.Extension.ToLower());
        }

        public override string GetCurrentImageUri()
        {
            return _currentUrl;
        }
    }
}