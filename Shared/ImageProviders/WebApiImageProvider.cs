using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
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
        private static HttpClient _client = new HttpClient();
        private PausableTimer _timer;
        private string _currentUrl;

        public WebApiImageProvider(Setting setting, string solutionConfigFile = null) : base(setting,
            solutionConfigFile, ImageBackgroundType.WebApi)
        {
            OnSettingChanged(setting, null);
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

            try
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(Setting.WebApiDownloadInterval);
                var request = new HttpRequestMessage(HttpMethod.Get, new Uri(Setting.WebApiEndpoint));
                request.Headers.Add("ContentType", "application/json; charset=utf-8");
                var httpres = await _client.SendAsync(request, cts.Token);
                if (!httpres.IsSuccessStatusCode) return;
                var response = await httpres.Content.ReadAsStringAsync();

                var reader = new JsonTextReader(new StringReader(response));

                while (reader.Read())
                    if (reader.Value != null && reader.TokenType == JsonToken.PropertyName &&
                        reader.Value.ToString().Equals(Setting.WebApiJsonKey))
                    {
                        var imageUrl = reader.ReadAsString();
                        _currentUrl = imageUrl;
                        if (!IsStaticImage())
                        {
                            FireImageAvailable();
                            return;
                        }
                        Image = await ImageDownloader.LoadImageAsync(imageUrl, Setting.ImageStretch, Setting.MaxWidth, Setting.MaxHeight, Setting);
                        _timer.Restart();
                        if (Image != null) FireImageAvailable();
                        return;
                    }
            }
            catch {}
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