using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;
using System.Threading;
using System.Collections;
using ClaudiaIDE.Helpers;
using System.Windows.Threading;

namespace ClaudiaIDE
{
    public class SildeShowImageProvider : IImageProvider
    {
        private readonly Timer _timer;
        private Setting _setting;
        private ImageFiles _imageFiles;
        private IEnumerator<string> _imageFilesPath;

        public SildeShowImageProvider(Setting setting)
        {
            _setting = setting;

            _imageFiles = GetImagesFromDirectory();
            _imageFilesPath = _imageFiles.GetEnumerator();
            ChangeImage(null);

            _timer = new Timer(new TimerCallback(ChangeImage));
            _timer.Change((int)_setting.UpdateImageInterval.TotalMilliseconds, (int)_setting.UpdateImageInterval.TotalMilliseconds);
        }

        public event EventHandler NewImageAvaliable;

        private ImageFiles GetImagesFromDirectory()
        {
            return new ImageFiles{ Extensions = _setting.Extensions, ImageDirectoryPath = _setting.BackgroundImagesDirectoryAbsolutePath };
        }

        public BitmapImage GetBitmap(IWpfTextView provider)
        {
            var current = _imageFilesPath.Current;
            if (string.IsNullOrEmpty(current)) return null;

            var bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.CacheOption = BitmapCacheOption.OnLoad;
            bitmap.UriSource = new Uri(current, UriKind.RelativeOrAbsolute);
            bitmap.EndInit();
            bitmap.Freeze();
            if (_setting.ImageStretch == ImageStretch.None)
            {
                bitmap = Utils.EnsureMaxWidthHeight(bitmap, _setting.MaxWidth, _setting.MaxHeight);
            }
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
                _imageFiles = GetImagesFromDirectory();
                _imageFilesPath = _imageFiles.GetEnumerator();
                ChangeImage(null);
                _timer.Change(0, (int)_setting.UpdateImageInterval.TotalMilliseconds);
            }
        }

        private void ChangeImage(object args)
        {
            if(_imageFilesPath.MoveNext())
            {
                NewImageAvaliable?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                // Reached the end of the images. Loop to beginning?
                if (_setting.LoopSlideshow)
                {
                    _imageFilesPath.Reset();
                    if (_imageFilesPath.MoveNext())
                    {
                        NewImageAvaliable?.Invoke(this, EventArgs.Empty);
                    }
                }
            }
        }

        public ImageBackgroundType ProviderType
        {
            get
            {
                return ImageBackgroundType.Slideshow;
            }
        }
    }

    public class ImageFiles : IEnumerable<string>
    {
        public string Extensions { get; set; }
        public string ImageDirectoryPath { get; set; }

        public IEnumerator<string> GetEnumerator()
        {
            if (string.IsNullOrEmpty(Extensions) || string.IsNullOrEmpty(ImageDirectoryPath))
            {
                return new ImageFilesEnumerator(new List<string>());
            }

            var extensions = Extensions
                .Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToUpper());
            List<string> imageFilePaths = Directory.GetFiles(new DirectoryInfo(ImageDirectoryPath).FullName)
                .Where(x => extensions.Contains(Path.GetExtension(x).ToUpper()))
                .OrderBy(x => Guid.NewGuid())
                .ToList();

            if (!imageFilePaths.Any())
            {
                return new ImageFilesEnumerator(new List<string>());
            }
            else
            {
                return new ImageFilesEnumerator(imageFilePaths);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ImageFilesEnumerator : IEnumerator<string>
    {
        private int position;
        private List<string> imageFilePaths;
        public ImageFilesEnumerator(List<string> imageFilePaths)
        {
            this.imageFilePaths = imageFilePaths;
            position = -1;
        }

        public string Current
        {
            get
            {
                return imageFilePaths[position];
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }

        public void Dispose() { }

        public bool MoveNext()
        {
            position++;
            return position < imageFilePaths.Count;
        }

        public void Reset()
        {
            position = -1;
        }
    }
}