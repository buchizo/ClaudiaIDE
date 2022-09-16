using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Interfaces;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Threading;
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
            _timer.Stop();
            ChangeImage();
            if (IsPaused)
                _timer.Pause();
        }

        private void ChangeImage()
        {
            ChangeImageAsync().Forget();
        }

        private async Task ChangeImageAsync()
        {
            if (Setting.ImageBackgroundType != ImageBackgroundType.WebApi) return;
            if (_timer.IsPaused) return;
            using (var client = new WebClient())
            {
                try
                {
                    var endpointResult = await client.DownloadStringTaskAsync(new Uri(Setting.WebApiEndpoint));
                    var reader = new JsonTextReader(new StringReader(endpointResult));

                    while (reader.Read())
                        if (reader.Value != null && reader.TokenType == JsonToken.PropertyName &&
                            reader.Value.ToString().Equals(Setting.WebApiJsonKey))
                        {
                            var imageUrl = reader.ReadAsString();
                            Image = await ImageDownloader.LoadImageAsync(imageUrl, Setting.ImageStretch, Setting.MaxWidth, Setting.MaxHeight);
                            _timer.Restart();
                            if (Image != null) FireImageAvailable();
                            return;
                        }
                }
                catch {}
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

            if ((sender as Setting)?.ImageBackgroundType != ImageBackgroundType.WebApi)
            {
                _timer.Stop();
                return;
            }

            ImageDownloader.ResetUrl();
            ChangeImage();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            ChangeImage();
        }
    }
}