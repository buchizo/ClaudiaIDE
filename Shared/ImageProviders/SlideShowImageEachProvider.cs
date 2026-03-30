using ClaudiaIDE;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.ImageProviders;
using ClaudiaIDE.Interfaces;
using ClaudiaIDE.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;
using System.Windows.Media.Imaging;

namespace ClaudiaIDE.ImageProviders
{
    public class SlideShowImageEachProvider : ImageProvider, IPausable, ISkipable, IMovable
    {
        private PausableTimer _timer;
        private ImageFiles _imageFiles;
        private IEnumerator<string> _imageFilesPath;
        private FileSystemWatcher _fileWatcher;

        public bool IsPaused => _timer.IsPaused;

        public SlideShowImageEachProvider(Setting setting, string solutionfile = null) : base(setting, solutionfile,
            ImageBackgroundType.SlideshowEach)
        {
            OnSettingChanged(setting, null);
        }

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
                Extensions = Setting.Extensions,
                ImageDirectoryPath = Setting.BackgroundImagesDirectoryAbsolutePath,
                Shuffle = Setting.ShuffleSlideshow,
                IncludeSubdirectories = Setting.IncludeSubdirectories,
            };
        }

        public override BitmapSource GetBitmap()
        {
            try
            {
                if (_imageFiles == null)
                {
                    ReEnumerationFiles();
                    _imageFilesPath.MoveNext();
                    _timer.Restart();
                }
                var current = _imageFilesPath?.Current;
                if (string.IsNullOrEmpty(current) || !IsStaticImage()) return null;

                var bitmap = new BitmapImage();
                var fileInfo = new FileInfo(current);
                if (fileInfo.Exists)
                {
                    try
                    {
                        bitmap.BeginInit();
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.CreateOptions = BitmapCreateOptions.None;
                        bitmap.UriSource = new Uri(current, UriKind.RelativeOrAbsolute);
                        bitmap.EndInit();
                        bitmap.Freeze();
                    }
                    catch
                    {
                        // maybe not supported exception
                        bitmap = new BitmapImage();
                        bitmap.Freeze();
                    }
                }
                else
                {
                    ReEnumerationFiles();
                    _imageFilesPath.MoveNext();
                    return GetBitmap();
                }

                BitmapSource ret_bitmap = bitmap;
                if (Setting.ImageStretch == ImageStretch.None)
                {
                    bitmap = Utils.EnsureMaxWidthHeight(bitmap, Setting.MaxWidth, Setting.MaxHeight);

                    if (bitmap.Width != bitmap.PixelWidth || bitmap.Height != bitmap.PixelHeight)
                        ret_bitmap = Utils.ConvertToDpi96(bitmap);
                    else
                        ret_bitmap = bitmap;
                }

                if (Setting.SoftEdgeX > 0 || Setting.SoftEdgeY > 0)
                    ret_bitmap = Utils.SoftenEdges(ret_bitmap, Setting.SoftEdgeX, Setting.SoftEdgeY);

                return ret_bitmap;
            }
            catch (ArgumentOutOfRangeException)
            {
                if (_imageFilesPath?.MoveNext() ?? false)
                {
                    return GetBitmap();
                }
                else
                {
                    _imageFilesPath?.Reset();
                    return null;
                }
            }
            catch
            {
                return null;
            }
        }

        private void ReEnumerationFiles()
        {
            _imageFiles = GetImagesFromDirectory();
            _imageFilesPath = _imageFiles.GetEnumerator();
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
                ReEnumerationFiles();
                ChangeImage();
                _timer.Restart();
            }

            _fileWatcher?.Dispose();
            if (Directory.Exists((sender as Setting)?.BackgroundImagesDirectoryAbsolutePath))
            {
                _fileWatcher = new FileSystemWatcher((sender as Setting)?.BackgroundImagesDirectoryAbsolutePath)
                {
                    IncludeSubdirectories = Setting.IncludeSubdirectories,
                    InternalBufferSize = 32 * 1024,
                    NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastWrite | NotifyFilters.Security | NotifyFilters.Size
                };
                _fileWatcher.Changed += new FileSystemEventHandler(OnDirectoryChanged);
                _fileWatcher.EnableRaisingEvents = true;
            }
        }

        private void OnDirectoryChanged(object source, FileSystemEventArgs e)
        {
            ReEnumerationFiles();
        }

        public void NextImage()
        {
            if (Setting.ImageBackgroundType != ImageBackgroundType.SlideshowEach) return;
            ChangeImage();
        }

        protected void ChangeImage()
        {
            if (_imageFiles == null)
            {
                ReEnumerationFiles();
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
}
