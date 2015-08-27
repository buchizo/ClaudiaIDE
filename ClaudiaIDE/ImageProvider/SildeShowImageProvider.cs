using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;

namespace ClaudiaIDE
{
    public class SildeShowImageProvider : IImageProvider
    {
        private readonly BitmapImage _emptyBitmap = new BitmapImage();
        private readonly static Timer Timer;
        private readonly TimeSpan _updateInterval;
        private DateTime _lastUpdateTime;
        private readonly List<BitmapImage> _bitmaps = new List<BitmapImage>();
        private readonly string[] _extensions;
        private int _currentImageIndex = 0;

        static SildeShowImageProvider()
        {
            Timer = new Timer
            {
                Interval = 1000,
                AutoReset = true,
                Enabled = true
            };
        }

        public SildeShowImageProvider(Setting setting)
        {
            _updateInterval = setting.UpdateImageInterval;
            _lastUpdateTime = DateTime.Now;
            _extensions = setting.Extensions.Split(new[]{",", " "}, StringSplitOptions.RemoveEmptyEntries);

            CreateBitmaps(setting.BackgroundImagesDirectoryAbsolutePath);
            if (!_bitmaps.Any())
            {
                return;
            }
            Timer.Elapsed += (o, e) =>
            {
                if (DateTime.Now - _lastUpdateTime >= _updateInterval)
                {
                    _lastUpdateTime = DateTime.Now;
                    _currentImageIndex = (_currentImageIndex + 1) % _bitmaps.Count;
                    NewImageAvaliable?.Invoke(this, EventArgs.Empty);
                }
            };
        }

        public event EventHandler NewImageAvaliable;

        public BitmapImage GetBitmap(IWpfTextView provider)
        {
            return _bitmaps.Any() ? _bitmaps[_currentImageIndex] : _emptyBitmap;
        }

        private void CreateBitmaps(string backgroundImageAbsolutePath)
        {
            try
            {
                var paths = GetAllImagesFromFolder(backgroundImageAbsolutePath);
                foreach (var path in paths)
                {
                    _bitmaps.Add(CreateBitmap(path));
                }
            }
            catch (Exception)
            {
            }
        }

        private IEnumerable<string> GetAllImagesFromFolder(string backgroundImageAbsolutePath)
        {
            return Directory.GetFiles(new DirectoryInfo(backgroundImageAbsolutePath).FullName)
                .Where(x => _extensions.Contains(Path.GetExtension(x)));
        }

        private BitmapImage CreateBitmap(string imagePath)
        {
            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
            bitmap.EndInit();
            return bitmap;
        }
    }
}