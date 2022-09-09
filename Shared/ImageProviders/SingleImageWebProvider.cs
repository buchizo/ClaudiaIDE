using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Threading;

namespace ClaudiaIDE.ImageProviders
{
    internal class SingleImageWebProvider : ImageProvider
    {
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
                Image = await ImageDownloader.LoadImageAsync(Setting.WebSingleUrl, Setting.ImageStretch, Setting.MaxWidth, Setting.MaxHeight);
                FireImageAvailable();
            }
            catch { }
        }
    }
}