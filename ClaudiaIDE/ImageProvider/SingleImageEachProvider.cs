using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;
using System.Threading;
using System.Collections;
using ClaudiaIDE.Helpers;

namespace ClaudiaIDE
{
    public class SingleImageEachProvider : IImageProvider
    {
        private Setting _setting;
        private ImageFiles _imageFiles;
        private IEnumerator<string> _imageFilesPath;

        public SingleImageEachProvider(Setting setting)
        {
            _setting = setting;
            _setting.OnChanged.AddEventHandler(ReloadSettings);
            ReloadSettings(null, null);
        }

        ~SingleImageEachProvider()
        {
            if (_setting != null)
            {
                _setting.OnChanged.RemoveEventHandler(ReloadSettings);
            }
        }

        public event EventHandler NewImageAvaliable;

        private ImageFiles GetImagesFromDirectory()
        {
            return new ImageFiles { Extensions = _setting.Extensions, ImageDirectoryPath = _setting.BackgroundImagesDirectoryAbsolutePath, Shuffle = _setting.ShuffleSlideshow };
        }

        public BitmapSource GetBitmap()
        {
            var current = _imageFilesPath.Current;
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
                if (_setting.ImageStretch == ImageStretch.None)
                {
                    bitmap = Utils.EnsureMaxWidthHeight(bitmap, _setting.MaxWidth, _setting.MaxHeight);
                    if (bitmap.Width != bitmap.PixelWidth || bitmap.Height != bitmap.PixelHeight)
                    {
                        return Utils.ConvertToDpi96(bitmap);
                    }
                }
            }
            else
            {
                ReloadSettings(null, null);
                return GetBitmap();
            }
            return bitmap;
        }

        private void ReloadSettings(object sender, System.EventArgs e)
        {
            _imageFiles = GetImagesFromDirectory();
            _imageFilesPath = _imageFiles.GetEnumerator();
            NextImage();
        }

        public void NextImage()
        {
            if (!_imageFilesPath.MoveNext())
                if (_setting.LoopSlideshow)
                {
                    _imageFilesPath.Reset();
                    _imageFilesPath.MoveNext();
                }
        }

        public ImageBackgroundType ProviderType
        {
            get
            {
                return ImageBackgroundType.SingleEach;
            }
        }
    }
}