using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;
using System.Threading;

namespace ClaudiaIDE
{
    public class SildeShowImageProvider : IImageProvider
    {
        private readonly BitmapImage _emptyBitmap = new BitmapImage();
        private readonly Timer _timer;
        private readonly List<BitmapImage> _bitmaps = new List<BitmapImage>();
        private readonly string[] _extensions;
        private int _currentImageIndex = 0;
        private Setting _setting;


        public SildeShowImageProvider(Setting setting)
        {
            _setting = setting;
            _timer = new Timer(new TimerCallback(ChangeImage));
            _timer.Change(0, (int)_setting.UpdateImageInterval.TotalMilliseconds);

            _extensions = setting.Extensions.Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries);

            CreateBitmaps(setting.BackgroundImagesDirectoryAbsolutePath);
            if (!_bitmaps.Any())
            {
                return;
            }
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
                _bitmaps.Clear();
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

        public void ReloadSettings()
        {
            if( _setting.ImageBackgroundType == ImageBackgroundType.Single)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            else
            {
                CreateBitmaps(_setting.BackgroundImagesDirectoryAbsolutePath);
                _timer.Change(0, (int)_setting.UpdateImageInterval.TotalMilliseconds);
            }
        }

        private void ChangeImage(object args)
        {
            _currentImageIndex = (_currentImageIndex + 1) % _bitmaps.Count;
            NewImageAvaliable?.Invoke(this, EventArgs.Empty);
        }

        public ImageBackgroundType ProviderType
        {
            get
            {
                return ImageBackgroundType.Slideshow;
            }
        }
    }
}