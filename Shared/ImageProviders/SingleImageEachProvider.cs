using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;

namespace ClaudiaIDE.ImageProviders
{
    public class SingleImageEachProvider : ImageProvider
    {
        private ImageFiles _imageFiles;
        private IEnumerator<string> _imageFilesPath;
        private FileSystemWatcher _fileWatcher;

        public SingleImageEachProvider(Setting setting, string solutionfile = null) : base(setting, solutionfile,
            ImageBackgroundType.SingleEach)
        {
            OnSettingChanged(setting, null);
        }

        private ImageFiles GetImagesFromDirectory()
        {
            return new ImageFiles
            {
                Extensions = Setting.Extensions,
                ImageDirectoryPath = Setting.BackgroundImagesDirectoryAbsolutePath,
                Shuffle = Setting.ShuffleSlideshow
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
                }
                var current = _imageFilesPath?.Current;
                if (string.IsNullOrEmpty(current) || !IsStaticImage()) return null;

                var bitmap = new BitmapImage();
                BitmapSource ret_bitmap = null;
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
                    if (Setting.ImageStretch == ImageStretch.None)
                        bitmap = Utils.EnsureMaxWidthHeight(bitmap, Setting.MaxWidth, Setting.MaxHeight);

                    if (Setting.ImageStretch == ImageStretch.None &&
                        (bitmap.Width != bitmap.PixelWidth || bitmap.Height != bitmap.PixelHeight)
                    )
                        ret_bitmap = Utils.ConvertToDpi96(bitmap);

                    if (Setting.SoftEdgeX > 0 || Setting.SoftEdgeY > 0)
                        ret_bitmap = Utils.SoftenEdges(ret_bitmap ?? bitmap, Setting.SoftEdgeX, Setting.SoftEdgeY);
                }
                else
                {
                    ReEnumerationFiles();
                    _imageFilesPath.MoveNext();
                    return GetBitmap();
                }

                if (ret_bitmap != null) return ret_bitmap;
                else return bitmap;
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
            if ((sender as Setting)?.ImageBackgroundType != ImageBackgroundType.SingleEach)
            {
                _fileWatcher?.Dispose();
                return;
            }
            ReEnumerationFiles();
            NextImage();

            _fileWatcher?.Dispose();
            if (File.Exists((sender as Setting)?.BackgroundImagesDirectoryAbsolutePath))
            {
                _fileWatcher = new FileSystemWatcher((sender as Setting)?.BackgroundImagesDirectoryAbsolutePath)
                {
                    IncludeSubdirectories = false,
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
            if (_imageFiles == null)
            {
                ReEnumerationFiles();
            }
            if (!_imageFilesPath.MoveNext())
                if (Setting.LoopSlideshow)
                {
                    _imageFilesPath.Reset();
                    _imageFilesPath.MoveNext();
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