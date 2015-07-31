using System;
using System.Windows.Threading;
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

	    /// <summary>
		/// Creates a square image and attaches an event handler to the layout changed event that
		/// adds the the square in the upper right-hand corner of the TextView via the adornment layer
		/// </summary>
		/// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
		public ClaudiaIDE(IWpfTextView view, IImageProvider imageProvider)
		{
		    try
		    {
		        _dispacher = Dispatcher.CurrentDispatcher;
                _imageProvider = imageProvider;
				_view = view;

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
				_adornmentLayer.RemoveAllAdornments();
			    var image = _imageProvider.GetImage(_view);
				_adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative,
					null,
					null,
					image,
					null);
			}
			catch (Exception)
			{
			
			}
		}
	}
}
