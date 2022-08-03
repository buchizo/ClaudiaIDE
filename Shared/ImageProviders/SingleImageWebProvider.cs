using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;

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
            ImageDownloader.ResetUrl();
        }

        public override BitmapSource GetBitmap()
        {
            if (Image != null) return Image;
            var image = ImageDownloader.LoadImage(Setting.WebSingleUrl, Setting.ImageStretch, Setting.MaxWidth,
                Setting.MaxHeight);
            _ = image.ContinueWith(OnDownloadComplete, CancellationToken.None,
                TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

            return null;
        }

        private void OnDownloadComplete(Task<BitmapImage> obj)
        {
            if (obj.IsCompleted)
            {
                Image = obj.Result;
                FireImageAvailable();
            }
        }
    }
}