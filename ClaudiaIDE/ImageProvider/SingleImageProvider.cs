﻿using System;
using System.IO;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;
using ClaudiaIDE.Helpers;
using System.Threading.Tasks;

namespace ClaudiaIDE.ImageProvider
{
    public class SingleImageProvider : IImageProvider
    {
        private BitmapImage _bitmap;
        private Setting _setting;

        public SingleImageProvider(Setting setting)
        {
            _setting = setting;
            _setting.OnChanged.AddEventHandler(ReloadSettings);

            LoadImage();
        }

        ~SingleImageProvider()
        {
            if(_setting != null)
            {
                _setting.OnChanged.RemoveEventHandler(ReloadSettings);
            }
        }

        private void ReloadSettings(object sender, System.EventArgs e)
        {
            LoadImage();
            NewImageAvaliable?.Invoke(this, EventArgs.Empty);
        }


        public async Task<BitmapSource> GetBitmap()
        {
            return await Task.Factory.StartNew(() => {
                if (_setting.ImageStretch == ImageStretch.None &&
                    (_bitmap.Width != _bitmap.PixelWidth || _bitmap.Height != _bitmap.PixelHeight)
                )
                {
                    return Utils.ConvertToDpi96(_bitmap);
                }
                else
                {
                    return _bitmap;
                }
            });
        }

        private void LoadImage()
        {
            var fileUri = new Uri(_setting.BackgroundImageAbsolutePath, UriKind.RelativeOrAbsolute);
            var fileInfo = new FileInfo(_setting.BackgroundImageAbsolutePath);
            _bitmap = new BitmapImage();
            if (fileInfo.Exists)
            {
                _bitmap.BeginInit();
                _bitmap.CacheOption = BitmapCacheOption.OnLoad;
                _bitmap.CreateOptions = BitmapCreateOptions.None;
                _bitmap.UriSource = fileUri;
                _bitmap.EndInit();
                _bitmap.Freeze();

                if (_setting.ImageStretch == ImageStretch.None)
                {
                    _bitmap = Utils.EnsureMaxWidthHeight(_bitmap, _setting.MaxWidth, _setting.MaxHeight);
                }
            }
        }

        public event EventHandler NewImageAvaliable;

        public ImageBackgroundType ProviderType
        {
            get
            {
                return ImageBackgroundType.Single;
            }
        }
    }
}
