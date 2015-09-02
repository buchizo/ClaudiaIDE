using System;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using System.Linq;
using ClaudiaIDE.ImageProvider;

namespace ClaudiaIDE
{
	/// <summary>
	/// Adornment class that draws a square box in the top right hand corner of the viewport
	/// </summary>
	public class ClaudiaIDE
	{
        private readonly List<IImageProvider> _imageProviders;
        private readonly IWpfTextView _view;
		private readonly IAdornmentLayer _adornmentLayer;
	    private readonly Dispatcher _dispacher;
	    private Image _image;
        private Setting _setting;
        private IImageProvider _imageProvider;

        /// <summary>
        /// Creates a square image and attaches an event handler to the layout changed event that
        /// adds the the square in the upper right-hand corner of the TextView via the adornment layer
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
        /// <param name="imageProvider">The <see cref="IImageProvider"/> which provides bitmaps to draw</param>
        /// <param name="setting">The <see cref="Setting"/> contains user image preferences</param>
        public ClaudiaIDE(IWpfTextView view, List<IImageProvider> imageProvider, Setting setting)
		{
		    try
		    {
		        _dispacher = Dispatcher.CurrentDispatcher;
                _imageProviders = imageProvider;
                _imageProvider = imageProvider.FirstOrDefault(x=>x.ProviderType == setting.ImageBackgroundType);
                _setting = setting;
                if (_imageProvider == null)
                {
                    _imageProvider = new SingleImageProvider(_setting);
                }
                _view = view;
                _image = new Image
                {
                    Opacity = setting.Opacity,
                    IsHitTestVisible = false
                };
                _adornmentLayer = view.GetAdornmentLayer("ClaudiaIDE");
				_view.ViewportHeightChanged += delegate { RepositionImage(); };
				_view.ViewportWidthChanged += delegate { RepositionImage(); };     
                _view.ViewportLeftChanged += delegate { RepositionImage(); };
                _setting.OnChanged += delegate { ReloadSettings(); };

                _imageProviders.ForEach(x => x.NewImageAvaliable += delegate { _dispacher.Invoke(ChangeImage); });

                ChangeImage();
            }
			catch
			{
			}
		}

        ~ClaudiaIDE()
        {
            try
            {
                if (_view != null)
                {
                    _view.ViewportHeightChanged -= delegate { RepositionImage(); };
                    _view.ViewportWidthChanged -= delegate { RepositionImage(); };
                    _view.ViewportLeftChanged -= delegate { RepositionImage(); };
                }
                _imageProviders.ForEach(x => x.NewImageAvaliable -= delegate { _dispacher.Invoke(ChangeImage); });
                if (_setting != null)
                {
                    _setting.OnChanged -= delegate { ReloadSettings(); };
                }
            }
            catch
            {
            }
        }

        private void ReloadSettings()
        {
            _imageProviders.ForEach(x => x.ReloadSettings());
            _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _setting.ImageBackgroundType);
            _dispacher.Invoke(ChangeImage);
        }

        private void ChangeImage()
		{
			try
			{
                var bitmap = _imageProvider.GetBitmap(_view);
                var fadetime = _setting.ImageFadeAnimationInterval;
                if (_setting.ImageBackgroundType == ImageBackgroundType.Single)
                {
                    _image = new Image
                    {
                        Opacity = _setting.Opacity,
                        IsHitTestVisible = false
                    };
                    _image.Source = bitmap;
                    SetImagePosition(_image);
                    ResizeImage(_image);
                }
                else
                {
                    _image.AnimateImageSourceChange(
                        bitmap,
                        img =>
                        {
                            SetImagePosition(img);
                            ResizeImage(img);
                        },
                        new AnimateImageChangeParams
                        {
                            TargetOpacity = _setting.Opacity,
                            FadeTime = fadetime
                        });
                }
                RefreshAdornment();
            }
            catch
			{
			}
		}

	    private void RepositionImage()
	    {
            try
            {
                SetImagePosition(_image);
                RefreshAdornment();
            }
            catch
            {
            }
        }

	    private void RefreshAdornment()
	    {
	        _adornmentLayer.RemoveAllAdornments();
	        _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative,
	            null,
	            null,
	            _image,
	            null);
	    }

	    private void SetImagePosition(Image image)
	    {
	        var bitmap = (BitmapImage)image.Source;
            switch (_setting.PositionHorizon)
	        {
	            case PositionH.Left:
	                Canvas.SetLeft(image, _view.ViewportLeft);
	                break;
	            case PositionH.Right:
	                Canvas.SetLeft(image, _view.ViewportRight - (double) bitmap.PixelWidth);
	                break;
	            case PositionH.Center:
	                Canvas.SetLeft(image,
	                    _view.ViewportRight - _view.ViewportWidth +
	                    ((_view.ViewportWidth/2) - ((double) bitmap.PixelWidth/2)));
	                break;
	        }
	        switch (_setting.PositionVertical)
	        {
	            case PositionV.Top:
	                Canvas.SetTop(image, _view.ViewportTop);
	                break;
	            case PositionV.Bottom:
	                Canvas.SetTop(image, _view.ViewportBottom - (double) bitmap.PixelHeight);
	                break;
	            case PositionV.Center:
	                Canvas.SetTop(image,
	                    _view.ViewportBottom - _view.ViewportHeight +
	                    ((_view.ViewportHeight/2) - ((double) bitmap.PixelHeight/2)));
	                break;
	        }
	    }

	    private void ResizeImage(Image image)
	    {
            var bitmap = (BitmapImage)image.Source;
            image.Width = bitmap.PixelWidth;
	        image.Height = bitmap.PixelHeight;
	    }
	}
}
