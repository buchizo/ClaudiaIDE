using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;

namespace ClaudiaIDE
{
    public class ImageProvider : IImageProvider
    {
        private readonly static Timer Timer;
        private readonly TimeSpan _updateInterval;
        private DateTime _lastUpdateTime;
        private readonly List<BitmapImage> _bitmaps = new List<BitmapImage>();
        private readonly Image _image;
        private readonly PositionH _positionHorizon;
        private readonly PositionV _positionVertical;
        private readonly string[] _extensions;
        private int _currentImageIndex = 0;

        static ImageProvider()
        {
            Timer = new Timer
            {
                Interval = 1000,
                AutoReset = true,
                Enabled = true
            };
        }

        public ImageProvider(Setting setting)
        {
            _updateInterval = setting.UpdateImageInterval;
            _lastUpdateTime = DateTime.Now;
            _positionHorizon = setting.PositionHorizon;
            _positionVertical = setting.PositionVertical;
            _extensions = setting.Extensions.Split(new[]{",", " "}, StringSplitOptions.RemoveEmptyEntries);
            _image = new Image
            {
                Opacity = setting.Opacity,
                IsHitTestVisible = false
            };
            CreateBitmaps(setting.BackgroundImageAbsolutePath);
            Timer.Elapsed += (o, e) =>
            {
                if (DateTime.Now - _lastUpdateTime > _updateInterval)
                {
                    _lastUpdateTime = DateTime.Now;
                    _currentImageIndex = (_currentImageIndex + 1) % _bitmaps.Count;
                    NewImageAvaliable?.Invoke(this, EventArgs.Empty);
                }
            };
        }

        public event EventHandler NewImageAvaliable;

        public Image GetImage(IWpfTextView provider)
        {
            var bitmap = GetCurrentBitmap();
            if (bitmap != _image.Source)
            {
                _image.Source = bitmap;
                _image.Width = bitmap.PixelWidth;
                _image.Height = bitmap.PixelHeight;
            }
            switch (_positionHorizon)
            {
                case PositionH.Left:
                    Canvas.SetLeft(_image, provider.ViewportLeft);
                    break;
                case PositionH.Right:
                    Canvas.SetLeft(_image, provider.ViewportRight - (double)bitmap.PixelWidth);
                    break;
                case PositionH.Center:
                    Canvas.SetLeft(_image,
                        provider.ViewportRight - provider.ViewportWidth +
                        ((provider.ViewportWidth / 2) - ((double)bitmap.PixelWidth / 2)));
                    break;
            }
            switch (_positionVertical)
            {
                case PositionV.Top:
                    Canvas.SetTop(_image, provider.ViewportTop);
                    break;
                case PositionV.Bottom:
                    Canvas.SetTop(_image, provider.ViewportBottom - (double)bitmap.PixelHeight);
                    break;
                case PositionV.Center:
                    Canvas.SetTop(_image,
                        provider.ViewportBottom - provider.ViewportHeight +
                        ((provider.ViewportHeight / 2) - ((double)bitmap.PixelHeight / 2)));
                    break;
            }        
            return _image;
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
            return Directory.GetFiles(new FileInfo(backgroundImageAbsolutePath).Directory.FullName)
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

        private BitmapImage GetCurrentBitmap()
        {
            return _bitmaps[_currentImageIndex];
        }
    }
}