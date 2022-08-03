using System;
using System.Collections.Generic;
using System.IO;
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
            OnSettingChanged(null, null);
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
            var current = _imageFilesPath?.Current;
            if (string.IsNullOrEmpty(current)) return null;

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
                if (Setting.ImageStretch == ImageStretch.None)
                {
                    bitmap = Utils.EnsureMaxWidthHeight(bitmap, Setting.MaxWidth, Setting.MaxHeight);
                    if (bitmap.Width != bitmap.PixelWidth || bitmap.Height != bitmap.PixelHeight)
                        return Utils.ConvertToDpi96(bitmap);
                }
            }
            else
            {
                OnSettingChanged(null, null);
                return GetBitmap();
            }

            return bitmap;
        }

        protected override void OnSettingChanged(object sender, EventArgs e)
        {
            _imageFiles = GetImagesFromDirectory();
            _imageFilesPath = _imageFiles.GetEnumerator();
            NextImage();
        }

        public void NextImage()
        {
            if (!_imageFilesPath.MoveNext())
                if (Setting.LoopSlideshow)
                {
                    _imageFilesPath.Reset();
                    _imageFilesPath.MoveNext();
                }
        }
    }
}