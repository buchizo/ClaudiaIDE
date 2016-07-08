using System.Windows.Controls;
using System.Windows.Threading;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using System.Linq;
using ClaudiaIDE.ImageProvider;
using System.Windows.Media;

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
        private Canvas _editorCanvas = new Canvas() { IsHitTestVisible = false };
        private Setting _setting = Setting.Instance;
        private IImageProvider _imageProvider;

        /// <summary>
        /// Creates a square image and attaches an event handler to the layout changed event that
        /// adds the the square in the upper right-hand corner of the TextView via the adornment layer
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView"/> upon which the adornment will be drawn</param>
        /// <param name="imageProvider">The <see cref="IImageProvider"/> which provides bitmaps to draw</param>
        /// <param name="setting">The <see cref="Setting"/> contains user image preferences</param>
        public ClaudiaIDE(IWpfTextView view, List<IImageProvider> imageProvider)
		{
		    try
		    {
                RenderOptions.SetBitmapScalingMode(_editorCanvas, BitmapScalingMode.Fant);

                _dispacher = Dispatcher.CurrentDispatcher;
                _imageProviders = imageProvider;
                _imageProvider = imageProvider.FirstOrDefault(x=>x.ProviderType == _setting.ImageBackgroundType);

                if (_imageProvider == null)
                {
                    _imageProvider = new SingleImageProvider(_setting);
                }
                _view = view;
                _adornmentLayer = view.GetAdornmentLayer("ClaudiaIDE");
                _view.LayoutChanged += (s,e) => { RepositionImage(); };
                _view.Closed += (s,e) =>
                {
                    _imageProviders.ForEach(x => x.NewImageAvaliable -= delegate { InvokeChangeImage(); });
                    if (_setting != null)
                    {
                        _setting.OnChanged -= delegate { ReloadSettings(); };
                    }
                };
                _setting.OnChanged += delegate { ReloadSettings(); };

                _imageProviders.ForEach(x => x.NewImageAvaliable += delegate { InvokeChangeImage(); });

                ChangeImage();
                RefreshAdornment();
            }
            catch
			{
			}
		}

        private void InvokeChangeImage()
        {
            try
            {
                _dispacher.Invoke(ChangeImage);
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
                var newimage = _imageProvider.GetBitmap(_view);
                if (_setting.ImageBackgroundType == ImageBackgroundType.Single)
                {
                    _editorCanvas.Background = new ImageBrush(newimage)
                    {
                        Opacity = _setting.Opacity,
                        Stretch = _setting.ImageStretch.ConvertTo(),
                        AlignmentX = _setting.PositionHorizon.ConvertTo(),
                        AlignmentY = _setting.PositionVertical.ConvertTo()
                    };
                }
                else
                {
                    _editorCanvas.Background
                        .AnimateImageSourceChange(
                            new ImageBrush(newimage)
                            {
                                Opacity = 0d,
                                Stretch = _setting.ImageStretch.ConvertTo(),
                                AlignmentX = _setting.PositionHorizon.ConvertTo(),
                                AlignmentY = _setting.PositionVertical.ConvertTo()
                            },
                            (n) => { _editorCanvas.Background = n; },
                            new AnimateImageChangeParams
                            {
                                FadeTime = _setting.ImageFadeAnimationInterval,
                                TargetOpacity = _setting.Opacity
                            }
                        );
                }
            }
            catch
			{
			}
		}

	    private void RepositionImage()
	    {
            try
            {
                Canvas.SetLeft(_editorCanvas, _view.ViewportLeft);
                Canvas.SetTop(_editorCanvas, _view.ViewportTop);

                _editorCanvas.Width = _view.ViewportWidth;
                _editorCanvas.Height = _view.ViewportHeight;
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
                _editorCanvas,
	            null);
	    }
    }
}
