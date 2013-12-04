using System;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using ClaudiaIDE.Settings;

namespace ClaudiaIDE
{
	/// <summary>
	/// Adornment class that draws a square box in the top right hand corner of the viewport
	/// </summary>
	public class ClaudiaIDE
	{
		private Setting _config;
		private Image _image;
		private BitmapImage _bitmap;
		private IWpfTextView _view;
		private IAdornmentLayer _adornmentLayer;

		/// <summary>
		/// Creates a square image and attaches an event handler to the layout changed event that
		/// adds the the square in the upper right-hand corner of the TextView via the adornment layer
		/// </summary>
		/// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
		public ClaudiaIDE(IWpfTextView view)
		{
			try
			{
				_view = view;
				_image = new Image();

				_config = Setting.Desirialize();
				_image.Opacity = _config.Opacity;

				_bitmap = new BitmapImage(new Uri(_config.BackgroundImageAbsolutePath, UriKind.Absolute));
				_image.Source = _bitmap;

				this._adornmentLayer = view.GetAdornmentLayer("ClaudiaIDE");
				_view.ViewportHeightChanged += delegate { this.onSizeChange(); };
				_view.ViewportWidthChanged += delegate { this.onSizeChange(); };
			}
			catch (Exception)
			{
			}
		}

		public void onSizeChange()
		{
			try
			{
				this._adornmentLayer.RemoveAllAdornments();

				switch (_config.PositionHorizon)
				{
					case PositionH.Left:
						Canvas.SetLeft(this._image, 0);
						break;
					case PositionH.Right:
						Canvas.SetLeft(this._image, this._view.ViewportRight - (double)_bitmap.PixelWidth);
						break;
				}
				switch (_config.PositionVertical)
				{
					case PositionV.Top:
						Canvas.SetTop(this._image, 0);
						break;
					case PositionV.Bottom:
						Canvas.SetTop(this._image, this._view.ViewportBottom - (double)_bitmap.PixelHeight);
						break;
				}

				this._adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative, 
					null,
					null,
					this._image,
					null);
			}
			catch (Exception)
			{
			
			}
		}
	}
}
