using System;
using System.Windows.Controls;
using System.Windows.Threading;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;

namespace ClaudiaIDE
{
	/// <summary>
	/// Adornment class that draws a square box in the top right hand corner of the viewport
	/// </summary>
	public class ClaudiaIDE
	{
	    private readonly IImageProvider _imageProvider;
		private readonly IWpfTextView _view;
		private readonly IAdornmentLayer _adornmentLayer;
	    private readonly Dispatcher _dispacher;
	    private readonly Image _image;
	    private readonly PositionV _positionVertical;
	    private readonly PositionH _positionHorizon;

	    /// <summary>
		/// Creates a square image and attaches an event handler to the layout changed event that
		/// adds the the square in the upper right-hand corner of the TextView via the adornment layer
		/// </summary>
		/// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
		public ClaudiaIDE(IWpfTextView view, IImageProvider imageProvider, Setting setting)
		{
		    try
		    {
		        _dispacher = Dispatcher.CurrentDispatcher;
                _imageProvider = imageProvider;
				_view = view;
                _image = new Image
                {
                    Opacity = setting.Opacity,
                    IsHitTestVisible = false
                };
                _positionHorizon = setting.PositionHorizon;
                _positionVertical = setting.PositionVertical;
                _adornmentLayer = view.GetAdornmentLayer("ClaudiaIDE");
				_view.ViewportHeightChanged += delegate { RefreshImage(); };
				_view.ViewportWidthChanged += delegate { RefreshImage(); };     
                _view.ViewportLeftChanged += delegate { RefreshImage(); };
                _imageProvider.NewImageAvaliable += delegate { _dispacher.Invoke(() => this.RefreshImage()); };
            }
			catch (Exception)
			{
			}
		}

		public void RefreshImage()
		{
			try
			{
                var bitmap = _imageProvider.GetBitmap(_view);
                if (bitmap != _image.Source)
                {
                    _image.Source = bitmap;
                    _image.Width = bitmap.PixelWidth;
                    _image.Height = bitmap.PixelHeight;
                }
                switch (_positionHorizon)
                {
                    case PositionH.Left:
                        Canvas.SetLeft(_image, _view.ViewportLeft);
                        break;
                    case PositionH.Right:
                        Canvas.SetLeft(_image, _view.ViewportRight - (double)bitmap.PixelWidth);
                        break;
                    case PositionH.Center:
                        Canvas.SetLeft(_image,
                            _view.ViewportRight - _view.ViewportWidth +
                            ((_view.ViewportWidth / 2) - ((double)bitmap.PixelWidth / 2)));
                        break;
                }
                switch (_positionVertical)
                {
                    case PositionV.Top:
                        Canvas.SetTop(_image, _view.ViewportTop);
                        break;
                    case PositionV.Bottom:
                        Canvas.SetTop(_image, _view.ViewportBottom - (double)bitmap.PixelHeight);
                        break;
                    case PositionV.Center:
                        Canvas.SetTop(_image,
                            _view.ViewportBottom - _view.ViewportHeight +
                            ((_view.ViewportHeight / 2) - ((double)bitmap.PixelHeight / 2)));
                        break;
                }

                _adornmentLayer.RemoveAllAdornments();
				_adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative,
					null,
					null,
					_image,
					null);
			}
			catch (Exception)
			{
			
			}
		}
	}
}
