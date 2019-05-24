using System.Windows.Controls;
using System.Windows.Threading;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Text.Editor;
using System.Collections.Generic;
using System.Linq;
using ClaudiaIDE.ImageProvider;
using System.Windows.Media;
using System;
using System.Threading.Tasks;
using System.Windows;

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
        private Canvas _editorCanvas = new Canvas() { IsHitTestVisible = false };
        private Setting _settings = Setting.Instance;
        private IImageProvider _imageProvider;
        private bool _isMainWindow;
        private System.Windows.DependencyObject _wpfTextViewHost = null;
        private Dictionary<int, System.Windows.DependencyObject> _defaultThemeColor = new Dictionary<int, DependencyObject>();
        private bool _hasImage = false;

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
                _imageProviders = imageProvider;
                _imageProvider = imageProvider.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);

                if (_imageProvider == null)
                {
                    _imageProvider = new SingleImageProvider(_settings);
                }
                _view = view;
                _adornmentLayer = view.GetAdornmentLayer("ClaudiaIDE");
                _view.LayoutChanged += (s, e) =>
                {
                    if (!_hasImage)
                    {
                        ChangeImage();
                    }
                    else
                    {
                        RefreshBackground();
                    }
                };
                _view.Closed += (s, e) =>
                {
                    _imageProviders.ForEach(x => x.NewImageAvaliable -= InvokeChangeImage);
                    if (_settings != null)
                    {
                        _settings.OnChanged.RemoveEventHandler(ReloadSettings);
                    }
                };
                _view.BackgroundBrushChanged += (s, e) =>
                {
                    _hasImage = false;
                    SetCanvasBackground();
                };
                _settings.OnChanged.AddEventHandler(ReloadSettings);

                _imageProviders.ForEach(x => x.NewImageAvaliable += InvokeChangeImage);

                SetCanvasBackground();
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
                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    ChangeImage();
                    GC.Collect();
                });
            }
            catch
            {
            }
        }

        private void ReloadSettings(object sender, System.EventArgs e)
        {
            _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);
            _hasImage = false;
            ChangeImage();
        }

        private void ChangeImage()
        {
            try
            {
                SetCanvasBackground();
                FindWpfTextView(_editorCanvas as System.Windows.DependencyObject);
                if (_wpfTextViewHost == null) return;

                var newimage = _imageProvider.GetBitmap();
                var opacity = _settings.ExpandToIDE && _isMainWindow ? 0.0 : _settings.Opacity;

                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    try
                    {
                        var nib = new ImageBrush(newimage)
                        {
                            Stretch = _settings.ImageStretch.ConvertTo(),
                            AlignmentX = _settings.PositionHorizon.ConvertTo(),
                            AlignmentY = _settings.PositionVertical.ConvertTo(),
                            Opacity = _settings.Opacity
                        };
                        _wpfTextViewHost.SetValue(Panel.BackgroundProperty, nib);
                        _hasImage = true;
                    }
                    catch
                    {
                    }
                });
            }
            catch
            {
            }
        }

        private void RefreshBackground()
        {
            SetCanvasBackground();
            FindWpfTextView(_editorCanvas as System.Windows.DependencyObject);
            if (_wpfTextViewHost == null) return;
            var opacity = _settings.ExpandToIDE && _isMainWindow ? 0.0 : _settings.Opacity;

            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    var background = (_wpfTextViewHost as System.Windows.Controls.Panel).Background;
                    if (opacity < 0.01)
                    {
                        background.Opacity = 0.01;
                        background.Opacity = opacity;
                    }
                    else
                    {
                        background.Opacity = opacity - 0.01;
                        background.Opacity = opacity;
                    }
                }
                catch
                {
                }
            });
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

        private void SetCanvasBackground()
        {
            _isMainWindow = IsRootWindow();
            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    SetBackgroundBrush(_editorCanvas as System.Windows.DependencyObject);
                }
                catch
                {
                }
            });
        }

        private bool IsRootWindow()
        {
            var root = FindUI(_view as System.Windows.DependencyObject, "Microsoft.VisualStudio.PlatformUI.MainWindow");
            if (root != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FindWpfTextView(System.Windows.DependencyObject d)
        {
            if (_wpfTextViewHost == null)
            {
                _wpfTextViewHost = FindUI(d, "Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost");
                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    try
                    {
                        if (_wpfTextViewHost == null) return;
                        RenderOptions.SetBitmapScalingMode(_wpfTextViewHost, BitmapScalingMode.Fant);
                    }
                    catch
                    {
                    }
                });
            }
        }

        private System.Windows.DependencyObject FindUI(System.Windows.DependencyObject d, string name)
        {
            var p = VisualTreeHelper.GetParent(d);
            if (d.GetType().FullName.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return d;
            }
            else if (p == null)
            {
                return null;
            }
            else
            {
                return FindUI(p, name);
            }
        }

        private void SetBackgroundBrush(System.Windows.DependencyObject d, bool isTransparent = true, bool untilRoot = false)
        {
            var refd = d.GetType();
            if (untilRoot)
            {
                if (refd.FullName.Equals("Microsoft.VisualStudio.PlatformUI.MainWindow", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            else
            {
                if (refd.FullName.Equals("Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost", StringComparison.OrdinalIgnoreCase))
                {
                    untilRoot = true;
                    isTransparent = _settings.ExpandToIDE && _isMainWindow;
                    var p2 = VisualTreeHelper.GetParent(d);
                    if (p2 == null) return;
                    SetBackgroundBrush(p2, isTransparent, untilRoot);
                    return;
                }
            }
            var property = refd.GetProperty("Background");
            var t = property?.GetValue(d) as Brush;
            if (t != null)
            {
                if (isTransparent)
                {
                    if (t as SolidColorBrush != null)
                    {
                        if (!_defaultThemeColor.Any(x => x.Key == d.GetHashCode()))
                        {
                            _defaultThemeColor[d.GetHashCode()] = t as DependencyObject;
                        }
                        property.SetValue(d, (Brush)Brushes.Transparent);
                    }
                }
                else
                {
                    var d1 = _defaultThemeColor.FirstOrDefault(x => x.Key == t.GetHashCode());
                    if (d1.Value != null)
                    {
                        property.SetValue(d, (Brush)d1.Value);
                    }
                }
            }
            var p = VisualTreeHelper.GetParent(d);
            if (p == null) return;
            SetBackgroundBrush(p, isTransparent, untilRoot);
        }
    }
}
