using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Interfaces;
using ClaudiaIDE.Settings;

namespace ClaudiaIDE.ImageProviders
{
    public class SlideShowImageProvider : ImageProvider, IPausable, ISkipable
    {
        private PausableTimer _timer;
        private ImageFiles _imageFiles;
        private IEnumerator<string> _imageFilesPath;

        public SlideShowImageProvider(Setting setting, string solutionfile = null) : base(setting, solutionfile,
            ImageBackgroundType.Slideshow)
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
            ChangeImage();
        }

        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            NextImage();
        }

        private ImageFiles GetImagesFromDirectory()
        {
            return new ImageFiles
            {
                Extensions = Setting.Extensions, ImageDirectoryPath = Setting.BackgroundImagesDirectoryAbsolutePath,
                Shuffle = Setting.ShuffleSlideshow
            };
        }

        public override BitmapSource GetBitmap()
        {
            try
            {
                if (_imageFiles == null)
                {
                    _imageFiles = GetImagesFromDirectory();
                    _imageFilesPath = _imageFiles.GetEnumerator();
                    _imageFilesPath.MoveNext();
                    _timer.Restart();
                }
                var current = _imageFilesPath?.Current;
                if (string.IsNullOrEmpty(current) || !IsStaticImage()) return null;

                var bitmap = new BitmapImage();
                var fileInfo = new FileInfo(current);
                if (fileInfo.Exists)
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.None;
                    bitmap.UriSource = new Uri(current, UriKind.RelativeOrAbsolute);
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                else
                {
                    OnSettingChanged(null, null);
                    return GetBitmap();
                }


                BitmapSource ret_bitmap = bitmap;
                if (Setting.ImageStretch == ImageStretch.None)
                {
                    bitmap = Utils.EnsureMaxWidthHeight(bitmap, Setting.MaxWidth, Setting.MaxHeight);
                    if (bitmap.Width != bitmap.PixelWidth || bitmap.Height != bitmap.PixelHeight)
                        ret_bitmap = Utils.ConvertToDpi96(bitmap);
                }

                if (Setting.BlurRadius > 0)
                    ret_bitmap = Utils.Blur(ret_bitmap, Setting.BlurRadius);

                if (Setting.SoftEdgeX > 0 || Setting.SoftEdgeY > 0)
                    ret_bitmap = Utils.SoftenEdges(ret_bitmap, Setting.SoftEdgeX, Setting.SoftEdgeY);

                return ret_bitmap;
            }
            catch
            {
                return null;
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
            _timer = new PausableTimer(Setting.UpdateImageInterval.TotalMilliseconds);
            _timer.Elapsed += OnTimerElapsed;

            if ((sender as Setting)?.ImageBackgroundType != ImageBackgroundType.Slideshow)
            {
                _timer.Stop();
            }
            else
            {
                _imageFiles = GetImagesFromDirectory();
                _imageFilesPath = _imageFiles.GetEnumerator();
                ChangeImage();
                _timer.Restart();
            }
        }


        public void NextImage()
        {
            if (Setting.ImageBackgroundType != ImageBackgroundType.Slideshow) return;
            ChangeImage();
        }

        protected void ChangeImage()
        {
            if (_imageFiles == null)
            {
                _imageFiles = GetImagesFromDirectory();
                _imageFilesPath = _imageFiles.GetEnumerator();
            }
            if (_imageFilesPath.MoveNext())
            {
                FireImageAvailable();
                _timer.Restart();
            }
            else
            {
                // Reached the end of the images. Loop to beginning?
                if (Setting.LoopSlideshow)
                {
                    _imageFilesPath.Reset();
                    if (_imageFilesPath.MoveNext())
                    {
                        FireImageAvailable();
                        _timer.Restart();
                    }
                    else
                    {
                        _timer.Stop();
                    }
                }
                else
                {
                    _timer.Stop();
                }
            }
        }

        public override bool IsStaticImage()
        {
            if (string.IsNullOrEmpty(_imageFilesPath?.Current)) return true;
            var fileInfo = new FileInfo(_imageFilesPath?.Current);
            return !Setting.SupportVideoFileExtensions.Any(x => x == fileInfo.Extension.ToLower());
        }

        public override string GetCurrentImageUri()
        {
            return _imageFilesPath?.Current;
        }
    }

    public class ImageFiles : IEnumerable<string>
    {
        public string Extensions { get; set; }
        public string ImageDirectoryPath { get; set; }
        public bool Shuffle { get; set; }

        public IEnumerator<string> GetEnumerator()
        {
            if (string.IsNullOrEmpty(Extensions) || string.IsNullOrEmpty(ImageDirectoryPath) ||
                !Directory.Exists(ImageDirectoryPath))
                return new ImageFilesEnumerator(new List<string>());

            var extensions = Extensions
                .Split(new[] { ",", " " }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.ToUpper());
            var imageFilePaths = Directory.GetFiles(new DirectoryInfo(ImageDirectoryPath).FullName)
                .Where(x => extensions.Contains(Path.GetExtension(x).ToUpper())).OrderBy(x => x).ToList();
            if (Shuffle) imageFilePaths.Shuffle();
            if (!imageFilePaths.Any())
                return new ImageFilesEnumerator(new List<string>());
            return new ImageFilesEnumerator(imageFilePaths);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class ImageFilesEnumerator : IEnumerator<string>
    {
        private readonly List<string> imageFilePaths;
        private int position;

        public ImageFilesEnumerator(List<string> imageFilePaths)
        {
            this.imageFilePaths = imageFilePaths;
            position = -1;
        }

        public string Current => imageFilePaths[position];

        object IEnumerator.Current => Current;

        public void Dispose()
        {
        }

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