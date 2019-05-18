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
        private Canvas _editorCanvas = new Canvas() { IsHitTestVisible = false, Background = Brushes.Transparent };
        private Setting _settings = Setting.Instance;
        private IImageProvider _imageProvider;
        private Brush _themeViewBackground = null;
        private bool _isMainWindow;
        private System.Windows.DependencyObject _wpfTextViewHost = null;
        private Dictionary<int, System.Windows.DependencyObject> _defaultThemeColor = new Dictionary<int, DependencyObject>();

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
                _themeViewBackground = _view.Background;
                _adornmentLayer = view.GetAdornmentLayer("ClaudiaIDE");
                _view.LayoutChanged += (s, e) =>
                {
                    ChangeImage();
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
            ChangeImage();
        }

        private void ChangeImage()
        {
            try
            {
                SetCanvasBackground();

                if (_wpfTextViewHost == null)
                {
                    _wpfTextViewHost = FindWpfTextView(_editorCanvas as System.Windows.DependencyObject);
                    Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        try
                        {
                            RenderOptions.SetBitmapScalingMode(_wpfTextViewHost, BitmapScalingMode.Fant);
                        }
                        catch
                        {
                        }
                    });
                }

                var newimage = _imageProvider.GetBitmap();
                var opacity = _settings.ExpandToIDE && _isMainWindow ? 0.0 : _settings.Opacity;

                if (_settings.ImageBackgroundType == ImageBackgroundType.Single)
                {
                    Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        try
                        {
                            (_wpfTextViewHost as System.Windows.Controls.Panel).Background = new ImageBrush(newimage)
                            {
                                Opacity = opacity,
                                Stretch = _settings.ImageStretch.ConvertTo(),
                                AlignmentX = _settings.PositionHorizon.ConvertTo(),
                                AlignmentY = _settings.PositionVertical.ConvertTo()
                            };
                        }
                        catch
                        {
                        }
                    });
                }
                else
                {
                    Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                    {
                        await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                        try
                        {
                            (_wpfTextViewHost as System.Windows.Controls.Panel).Background.AnimateImageSourceChange(
                                new ImageBrush(newimage)
                                {
                                    Stretch = _settings.ImageStretch.ConvertTo(),
                                    AlignmentX = _settings.PositionHorizon.ConvertTo(),
                                    AlignmentY = _settings.PositionVertical.ConvertTo()
                                },
                                (n) => { (_wpfTextViewHost as System.Windows.Controls.Panel).Background = n; },
                                new AnimateImageChangeParams
                                {
                                    FadeTime = _settings.ImageFadeAnimationInterval,
                                    TargetOpacity = opacity
                                }
                            );
                        }
                        catch
                        {
                        }
                    });
                }
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

        private System.Windows.DependencyObject FindWpfTextView(System.Windows.DependencyObject d)
        {
            return FindUI(d, "Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost");
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
            if (untilRoot)
            {
                if (d.GetType().FullName.Equals("Microsoft.VisualStudio.PlatformUI.MainWindow", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }
            else
            {
                if (d.GetType().FullName.Equals("Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost", StringComparison.OrdinalIgnoreCase))
                {
                    untilRoot = true;
                    isTransparent = _settings.ExpandToIDE && _isMainWindow;
                    var p1 = VisualTreeHelper.GetParent(d);
                    if (p1 == null) return;
                    SetBackgroundBrush(p1, isTransparent, untilRoot);
                    return;
                }
            }
            var t = d as System.Windows.Controls.Panel;
            var t2 = d as System.Windows.Controls.Control;
            if (t != null)
            {
                if (t.Background != null)
                {
                    if (isTransparent)
                    {
                        if (!_defaultThemeColor.Any(x => x.Key == t.GetHashCode()))
                        {
                            _defaultThemeColor[t.GetHashCode()] = t.Background;
                        }
                        t.Background = Brushes.Transparent;
                    }
                    else
                    {
                        var d1 = _defaultThemeColor.FirstOrDefault(x => x.Key == t.GetHashCode());
                        if (d1.Value != null)
                        {
                            t.Background = (Brush)d1.Value;
                        }
                    }
                }
            }
            else if (t2 != null)
            {
                if (t2.Background != null)
                {
                    if (isTransparent)
                    {
                        if (!_defaultThemeColor.Any(x => x.Key == t2.GetHashCode()))
                        {
                            _defaultThemeColor[t2.GetHashCode()] = t2.Background;
                        }
                        t2.Background = Brushes.Transparent;
                    }
                    else
                    {
                        var d2 = _defaultThemeColor.FirstOrDefault(x => x.Key == t2.GetHashCode());
                        if (d2.Value != null)
                        {
                            t2.Background = (Brush)d2.Value;
                        }
                    }
                }
            }
            var p = VisualTreeHelper.GetParent(d);
            if (p == null) return;
            SetBackgroundBrush(p, isTransparent, untilRoot);
        }
    }
}
