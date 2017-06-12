using System.Windows.Controls;
using System.Windows.Threading;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using System.Linq;
using ClaudiaIDE.ImageProvider;
using System.Windows.Media;
using Microsoft.VisualStudio.PlatformUI;
using System;
using System.Threading.Tasks;

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
        private Brush _themeViewBackground = null;
        private Brush _themeViewStackBackground = null;
        private bool _isMainWindow;

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
                _themeViewBackground = _view.Background;
                _adornmentLayer = view.GetAdornmentLayer("ClaudiaIDE");
                _view.LayoutChanged += (s,e) => {
                    RepositionImage();
                    _isMainWindow = IsRootWindow();
                    SetCanvasBackground(_setting.ExpandToIDE);
                };
                _view.Closed += (s,e) =>
                {
                    _imageProviders.ForEach(x => x.NewImageAvaliable -= InvokeChangeImage);
                    if (_setting != null)
                    {
                        _setting.OnChanged.RemoveEventHandler(ReloadSettings);
                    }
                };
                _view.BackgroundBrushChanged += (s, e) =>
                {
                    _isMainWindow = IsRootWindow();
                    SetCanvasBackground(_setting.ExpandToIDE);
                };
                _setting.OnChanged.AddEventHandler(ReloadSettings);

                _imageProviders.ForEach(x => x.NewImageAvaliable += InvokeChangeImage);

                ChangeImage();
                RefreshAdornment();
            }
            catch
			{
			}
		}

        private void InvokeChangeImage(object sender, System.EventArgs e)
        {
            try
            {
                _dispacher.Invoke(ChangeImage);
                GC.Collect();
            }
            catch
            {
            }
        }

        private void ReloadSettings(object sender, System.EventArgs e)
        {
            _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _setting.ImageBackgroundType);
            _isMainWindow = IsRootWindow();
            _dispacher.Invoke(ChangeImage);
        }

        private async void ChangeImage()
		{
			try
			{
                SetCanvasBackground(_setting.ExpandToIDE);

                var newimage = await _imageProvider.GetBitmap();
                var opacity = _setting.ExpandToIDE && _isMainWindow ? 0.0 : _setting.Opacity;

                if (_setting.ImageBackgroundType == ImageBackgroundType.Single)
                {
                    _editorCanvas.Background = new ImageBrush(newimage)
                    {
                        Opacity = opacity,
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
                                Stretch = _setting.ImageStretch.ConvertTo(),
                                AlignmentX = _setting.PositionHorizon.ConvertTo(),
                                AlignmentY = _setting.PositionVertical.ConvertTo()
                            },
                            (n) => { _editorCanvas.Background = n; },
                            new AnimateImageChangeParams
                            {
                                FadeTime = _setting.ImageFadeAnimationInterval,
                                TargetOpacity = opacity
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
	        _adornmentLayer.RemoveAdornmentsByTag("ClaudiaIDE");
            _adornmentLayer.AddAdornment(AdornmentPositioningBehavior.ViewportRelative,
	            null,
	            "ClaudiaIDE",
                _editorCanvas,
	            null);
	    }

        private void SetCanvasBackground(bool isTransparent)
        {
            var control = (ContentControl)_view;
            var parent = (Grid)control.Parent;
            var viewstack = (Canvas)control.Content;
            var opacity = isTransparent && _isMainWindow ? 0.0 : _setting.Opacity;
            if (_themeViewBackground == null)
            {
                _themeViewBackground = _view.Background;
            }
            if (_themeViewStackBackground == null)
            {
                _themeViewStackBackground = viewstack.Background;
            }

            if (isTransparent && _isMainWindow)
            {
                _dispacher.Invoke(() =>
                {
                    try
                    {
                        viewstack.Background = Brushes.Transparent;
                        _view.Background = Brushes.Transparent;
                        var b = _editorCanvas.Background;
                        if (b != null)
                        {
                            b.Opacity = opacity;
                            _editorCanvas.Background = b;
                        }
                        parent?.ClearValue(Grid.BackgroundProperty);
                    }
                    catch
                    {
                    }
                });
            }
            else
            {
                _dispacher.Invoke(() =>
                {
                    try
                    {
                        viewstack.Background = _themeViewStackBackground;
                        _view.Background = _themeViewBackground;
                        var b = _editorCanvas.Background;
                        if (b != null)
                        {
                            b.Opacity = opacity;
                            _editorCanvas.Background = b;
                        }
                    }
                    catch
                    {
                    }
                });
            }
        }

        private bool IsRootWindow()
        {
            var root = FindUI(_view as System.Windows.DependencyObject);
            if (root.GetType().FullName.Equals("Microsoft.VisualStudio.PlatformUI.MainWindow", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private System.Windows.DependencyObject FindUI(System.Windows.DependencyObject d)
        {
            var p = VisualTreeHelper.GetParent(d);
            if( p == null)
            {
                // is root
                return d;
            }
            else {
                return FindUI(p);
            }
        }
    }
}
