using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Interfaces;
using ClaudiaIDE.Settings;
using Newtonsoft.Json;

namespace ClaudiaIDE.ImageProviders
{
    internal class WebApiImageProvider : ImageProvider, IPausable, ISkipable
    {
        private PausableTimer _timer;

        public WebApiImageProvider(Setting setting, string solutionConfigFile = null) : base(setting,
            solutionConfigFile, ImageBackgroundType.WebApi)
        {
            OnSettingChanged(null, null);
        }

        public bool IsPaused => _timer.IsPaused;

        public void Pause()
        {
            if (!IsPaused) _timer.Pause();
        }

        public void Resume()
        {
            if (IsPaused) _timer.Resume();
        }

        public void Skip()
        {
            var paused = IsPaused;
            _timer.Stop();
            ChangeImage();
            if (IsPaused)
                _timer.Pause();
        }

        private void ChangeImage()
        {
            if (Setting.ImageBackgroundType != ImageBackgroundType.WebApi) return;
            if (_timer.IsPaused) return;
            using (var client = new WebClient())
            {
                try
                {
                    var endpointResult = client.DownloadString(Setting.WebApiEndpoint);
                    var reader = new JsonTextReader(new StringReader(endpointResult));

                    while (reader.Read())
                        if (reader.Value != null && reader.TokenType == JsonToken.PropertyName &&
                            reader.Value.ToString().Equals(Setting.WebApiJsonKey))
                        {
                            var imageUrl = reader.ReadAsString();
                            var task = ImageDownloader.LoadImage(imageUrl, Setting.ImageStretch, Setting.MaxWidth,
                                Setting.MaxHeight);
                            _ = task.ContinueWith(OnDownloadComplete, CancellationToken.None,
                                TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
                        }
                }
                catch (Exception ex)
                {
                }
            }
        }

        private void OnDownloadComplete(Task<BitmapImage> obj)
        {
            if (obj.IsCompleted)
            {
                Image = obj.Result;
                _timer.Restart();
                FireImageAvailable();
            }
        }

        protected override void OnSettingChanged(object sender, EventArgs e)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Elapsed -= OnTimerElapsed;
                _timer.Dispose();
            }

            _timer = new PausableTimer(Setting.WebApiDownloadInterval.TotalMilliseconds);
            _timer.Elapsed += OnTimerElapsed;

            ImageDownloader.ResetUrl();
            ChangeImage();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            ChangeImage();
        }
    }
}