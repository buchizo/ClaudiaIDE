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

        public SingleImageEachProvider(Setting setting, string solutionfile = null) : base(setting, solutionfile,
            ImageBackgroundType.SingleEach)
        {
            OnSettingChanged(setting, null);
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
                }
                var current = _imageFilesPath?.Current;
                if (string.IsNullOrEmpty(current) || !IsStaticImage()) return null;

                var bitmap = new BitmapImage();
                BitmapSource ret_bitmap = null;
                var fileInfo = new FileInfo(current);
                if (fileInfo.Exists)
                {
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.CreateOptions = BitmapCreateOptions.None;
                    bitmap.UriSource = new Uri(current, UriKind.RelativeOrAbsolute);
                    bitmap.EndInit();
                    bitmap.Freeze();
                    if (Setting.ImageStretch == ImageStretch.None)
                        bitmap = Utils.EnsureMaxWidthHeight(bitmap, Setting.MaxWidth, Setting.MaxHeight);

                    if (Setting.ImageStretch == ImageStretch.None &&
                        (bitmap.Width != bitmap.PixelWidth || bitmap.Height != bitmap.PixelHeight)
                    )
                        ret_bitmap = Utils.ConvertToDpi96(bitmap);

                    if (Setting.BlurRadius > 0)
                        ret_bitmap = Utils.Blur(ret_bitmap ?? bitmap, Setting.BlurRadius);

                    if (Setting.SoftEdgeX > 0 || Setting.SoftEdgeY > 0)
                        ret_bitmap = Utils.SoftenEdges(ret_bitmap ?? bitmap, Setting.SoftEdgeX, Setting.SoftEdgeY);
                }
                else
                {
                    OnSettingChanged(null, null);
                    return GetBitmap();
                }

                if (ret_bitmap != null) return ret_bitmap;
                else return bitmap;
            }
            catch
            {
                return null;
            }
        }

        protected override void OnSettingChanged(object sender, EventArgs e)
        {
            if ((sender as Setting)?.ImageBackgroundType != ImageBackgroundType.SingleEach) return;
            _imageFiles = GetImagesFromDirectory();
            _imageFilesPath = _imageFiles.GetEnumerator();
            NextImage();
        }

        public void NextImage()
        {
            if (_imageFiles == null)
            {
                _imageFiles = GetImagesFromDirectory();
                _imageFilesPath = _imageFiles.GetEnumerator();
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