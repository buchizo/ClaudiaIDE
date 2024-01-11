using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;
using System.Xml.Linq;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.ImageProviders;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Threading;
using Task = System.Threading.Tasks.Task;

namespace ClaudiaIDE
{
    /// <summary>
    ///     Adornment class that draws a square box in the top right hand corner of the viewport
    /// </summary>
    public class ClaudiaIDE
    {
        private readonly IAdornmentLayer _adornmentLayer;
        private readonly Dictionary<string, DependencyObject> _defaultThemeColor = new Dictionary<string, DependencyObject>();
        private readonly Canvas _editorCanvas = new Canvas() { IsHitTestVisible = false };
        private readonly List<ImageProvider> _imageProviders;
        private readonly Setting _settings = Setting.Instance;
        private readonly IWpfTextView _view;
        private bool _hasImage = false;
        private ImageProvider _imageProvider;
        private bool _isMainWindow;
        private bool _isRootWindow = false;
        private bool _isTargetWindow = false;
        private DependencyObject _wpfTextViewHost = null;
        private VisualBrush _visualBrush = null;
        private VisualBrush _visualBrushStatic = null;
        private System.Drawing.Color _currentThemeColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);

        /// <summary>
        ///     Creates a square image and attaches an event handler to the layout changed event that
        ///     adds the the square in the upper right-hand corner of the TextView via the adornment layer
        /// </summary>
        /// <param name="view">The <see cref="IWpfTextView" /> upon which the adornment will be drawn</param>
        /// <param name="imageProvider">The <see cref="IImageProvider" /> which provides bitmaps to draw</param>
        /// <param name="setting">The <see cref="Setting" /> contains user image preferences</param>
        public ClaudiaIDE(IWpfTextView view, List<ImageProvider> imageProvider)
        {
            try
            {
                _imageProviders = imageProvider;
                _imageProvider = GetImageProvider();
                _view = view;
                _adornmentLayer = view.GetAdornmentLayer("ClaudiaIDE");
                _view.LayoutChanged += (s, e) =>
                {
                    if (!_hasImage)
                    {
                        InvokeChangeImage(null, null);
                    }
                    else
                    {
                        RefreshBackground();
                    }
                };
                _view.Closed += (s, e) =>
                {
                    _imageProviders.ForEach(x => x.NewImageAvailable -= InvokeChangeImage);
                    _settings?.OnChanged.RemoveEventHandler(ReloadSettings);
                };
                _view.BackgroundBrushChanged += (s, e) =>
                {
                    _hasImage = false;
                    SetCanvasBackground();
                };
                _settings.OnChanged.AddEventHandler(ReloadSettings);

                _imageProviders.ForEach(x => x.NewImageAvailable += InvokeChangeImage);
                VSColorTheme.ThemeChanged += VSColorTheme_ThemeChanged;

                SetCanvasBackground();
                InvokeChangeImage(null, null);
                RefreshAdornment();
            }
            catch
            {
            }
        }

        private void InvokeChangeImage(object sender, EventArgs e)
        {
            ChangeImageAsync().Forget();
        }

        private ImageProvider GetImageProvider()
        {
            var ret = ProvidersHolder.Instance.ActiveProvider ?? new SingleImageProvider(Setting.Instance);
            return ret;
        }

        private void ReloadSettings(object sender, EventArgs e)
        {
            _currentThemeColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
            _imageProvider = GetImageProvider();
            _hasImage = false;
            InvokeChangeImage(null, null);
        }

        private void SetCanvasBackground()
        {
            SetCanvasBackgroundAsync().Forget();
        }

        private async Task ChangeImageAsync()
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                await SetCanvasBackgroundAsync();
                await FindWpfTextViewAsync(_editorCanvas as DependencyObject);
                if (_wpfTextViewHost == null) return;
                if (!_isTargetWindow) return;

                var newimage = ProvidersHolder.Instance.ActiveProvider?.GetBitmap();
                var opacity = _settings.ExpandToIDE && _isMainWindow ? 0.0 : (_settings.IsHidden ? 0.0 : _settings.Opacity);

                if (_isRootWindow)
                {
                    var grid = new Grid()
                    {
                        Name = "ClaudiaIdeImage",
                        HorizontalAlignment = HorizontalAlignment.Stretch,
                        VerticalAlignment = VerticalAlignment.Stretch,
                        IsHitTestVisible = false
                    };
                    if (ProvidersHolder.Instance.ActiveProvider?.IsStaticImage() == true)
                    {
                        var imageControl = new Image
                        {
                            Source = newimage,
                            Effect = _settings.BlurRadius > 0 ? new BlurEffect()
                            {
                                Radius = _settings.BlurRadius,
                                KernelType = BlurMethodConverter.ConvertTo(_settings.BlurMethod)
                            } : null
                        };
                        RenderOptions.SetBitmapScalingMode(imageControl, BitmapScalingMode.Fant);
                        _visualBrushStatic = null;
                        _visualBrushStatic = new VisualBrush(imageControl)
                        {
                            Stretch = _settings.ImageStretch.ConvertTo(),
                            AlignmentX = _settings.PositionHorizon.ConvertTo(),
                            AlignmentY = _settings.PositionVertical.ConvertTo(),
                            Opacity = opacity,
                            Viewbox = new Rect(new Point(_settings.ViewBoxPointX, _settings.ViewBoxPointY),
                                new Size(1, 1)),
                            TileMode = _settings.TileMode.ConvertTo(),
                            Viewport = new Rect(_settings.ViewPortPointX, _settings.ViewPortPointY,
                                _settings.ViewPortWidth, _settings.ViewPortHeight)
                        };
                        grid.Background = _visualBrushStatic;
                    }
                    else
                    {
                        _visualBrush = null;
                        _visualBrush = new VisualBrush();
                        var me = new MediaElement
                        {
                            Source = new Uri(ProvidersHolder.Instance.ActiveProvider?.GetCurrentImageUri()),
                            LoadedBehavior = MediaState.Play,
                            UnloadedBehavior = MediaState.Manual,
                            IsMuted = true,
                            Effect = _settings.BlurRadius > 0 ? new BlurEffect()
                            {
                                Radius = _settings.BlurRadius,
                                KernelType = BlurMethodConverter.ConvertTo(_settings.BlurMethod)
                            } : null
                        };
                        me.MediaEnded += (s, e) =>
                        {
                            if (me == null) return;
                            me.Position = TimeSpan.FromMilliseconds(1);
                            me.Play();
                        };
                        RenderOptions.SetBitmapScalingMode(me, BitmapScalingMode.Fant);
                        _visualBrush.Visual = me;
                        _visualBrush.Opacity = opacity;
                        _visualBrush.AlignmentX = _settings.PositionHorizon.ConvertTo();
                        _visualBrush.AlignmentY = _settings.PositionVertical.ConvertTo();
                        _visualBrush.Stretch = _settings.ImageStretch.ConvertTo();
                        grid.Background = _visualBrush;
                    }
                    Grid.SetRowSpan(grid, 3);
                    Grid.SetColumnSpan(grid, 3);
                    if (VisualTreeHelper.GetParent(_wpfTextViewHost) is Grid p)
                    {
                        foreach (var c in p.Children)
                        {
                            if ((c as Grid)?.Name == "ClaudiaIdeImage")
                            {
                                p.Children.Remove(c as UIElement);
                                break;
                            }
                        }

                        p.Children.Insert(0, grid);
                    }
                }
                else
                {
                    if (ProvidersHolder.Instance.ActiveProvider?.IsStaticImage() == true)
                    {
                        var imageControl = new Image
                        {
                            Source = newimage,
                            Effect = _settings.BlurRadius > 0 ? new BlurEffect()
                            {
                                Radius = _settings.BlurRadius,
                                KernelType = BlurMethodConverter.ConvertTo(_settings.BlurMethod)
                            } : null,
                        };
                        RenderOptions.SetBitmapScalingMode(imageControl, BitmapScalingMode.Fant);

                        _visualBrushStatic = null;
                        _visualBrushStatic = new VisualBrush(imageControl)
                        {
                            Stretch = _settings.ImageStretch.ConvertTo(),
                            AlignmentX = _settings.PositionHorizon.ConvertTo(),
                            AlignmentY = _settings.PositionVertical.ConvertTo(),
                            Opacity = opacity,
                            Viewbox = new Rect(new Point(_settings.ViewBoxPointX, _settings.ViewBoxPointY),
                                new Size(1, 1)),
                            TileMode = _settings.TileMode.ConvertTo(),
                            Viewport = new Rect(_settings.ViewPortPointX, _settings.ViewPortPointY,
                                _settings.ViewPortWidth, _settings.ViewPortHeight)
                        };

                        if (_settings.ImageBackgroundType == ImageBackgroundType.Slideshow ||
                            _settings.ImageBackgroundType == ImageBackgroundType.WebApi)
                        {
                            (_wpfTextViewHost as Panel).Background.AnimateImageSourceChange(
                                _visualBrushStatic,
                                (n) => { /*(_wpfTextViewHost as Panel).Background = n;*/ _wpfTextViewHost.SetValue(Panel.BackgroundProperty, n); },
                                new AnimateImageChangeParams
                                {
                                    FadeTime = _settings.ImageFadeAnimationInterval,
                                    TargetOpacity = opacity
                                }
                            );
                        }
                        else
                        {
                            _wpfTextViewHost.SetValue(Panel.BackgroundProperty, _visualBrushStatic);
                        }
                    }
                    else
                    {
                        _visualBrush = null;
                        _visualBrush = new VisualBrush();
                        var me = new MediaElement
                        {
                            Source = new Uri(ProvidersHolder.Instance.ActiveProvider?.GetCurrentImageUri()),
                            LoadedBehavior = MediaState.Play,
                            UnloadedBehavior = MediaState.Manual,
                            IsMuted = true,
                            Effect = _settings.BlurRadius > 0 ? new BlurEffect()
                            {
                                Radius = _settings.BlurRadius,
                                KernelType = BlurMethodConverter.ConvertTo(_settings.BlurMethod)
                            } : null
                        };
                        me.MediaEnded += (s, e) =>
                        {
                            if (me == null) return;
                            me.Position = TimeSpan.FromMilliseconds(1);
                            me.Play();
                        };
                        RenderOptions.SetBitmapScalingMode(me, BitmapScalingMode.Fant);
                        _visualBrush.Visual = me;
                        _visualBrush.Opacity = opacity;
                        _visualBrush.AlignmentX = _settings.PositionHorizon.ConvertTo();
                        _visualBrush.AlignmentY = _settings.PositionVertical.ConvertTo();
                        _visualBrush.Stretch = _settings.ImageStretch.ConvertTo();
                        _wpfTextViewHost.SetValue(Panel.BackgroundProperty, _visualBrush);
                    }
                }

                _hasImage = true;
                if (_settings.ImageBackgroundType == ImageBackgroundType.SingleEach)
                    ((SingleImageEachProvider)_imageProvider).NextImage();
            }
            catch
            {
            }
        }

        private void RefreshBackground()
        {
            RefreshBackgroundAsync().Forget();
        }

        private async Task RefreshBackgroundAsync()
        {
            await SetCanvasBackgroundAsync();
            await FindWpfTextViewAsync(_editorCanvas as DependencyObject);
            if (_wpfTextViewHost == null) return;
            var opacity = _settings.ExpandToIDE && _isMainWindow ? 0.0 : (_settings.IsHidden ? 0.0 : _settings.Opacity);

            var refd = _wpfTextViewHost.GetType();
            var prop = refd.GetProperty("Background");
            var background = prop.GetValue(_wpfTextViewHost) as VisualBrush;
            if (background == null && _isRootWindow)
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    var c = (_wpfTextViewHost as UIElement).Opacity;
                    (_wpfTextViewHost as UIElement).Opacity = c < 0.01 ? 0.01 : c - 0.01;
                    (_wpfTextViewHost as UIElement).Opacity = c;
                }
                catch
                {
                }
            }
            else
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    if (background != null)
                    {
                        background.Opacity = opacity < 0.01 ? 0.01 : opacity - 0.01;
                        background.Opacity = opacity;
                    }
                }
                catch
                {
                }
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

        private async Task SetCanvasBackgroundAsync()
        {
            (_isMainWindow, _isTargetWindow) = IsMainWindow();
            if (!_isTargetWindow) return;

            var isTransparent = true;
            var current = _editorCanvas as DependencyObject;
            while (current != null)
            {
                var refd = current.GetType();
                var nameprop = refd.GetProperty("Name");
                var objname = nameprop?.GetValue(current) as string ?? "";
                if (objname.Equals("RootDockPanel", StringComparison.OrdinalIgnoreCase)) return; // stop for childs object
                if (!string.IsNullOrEmpty(objname) && (objname.Equals("RootGrid", StringComparison.OrdinalIgnoreCase) ||
                                                        objname.Equals("MainWindow",
                                                            StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                if (_isRootWindow &&
                    refd.FullName.Equals("Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextView",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (refd.FullName.Equals("Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost",
                        StringComparison.OrdinalIgnoreCase))
                {
                    if (_isMainWindow)
                    {
                        // set to transparent or default brush for chilren
                        foreach (var c in current.Children())
                        {
                            await SetTransparentForChildAsync(c, parentName: $"{refd.Name}|{objname}");
                        }
                    }
                }
                else if (refd.FullName.Equals("Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextView",
                                StringComparison.OrdinalIgnoreCase))
                {
                    // for visualize history(using Gource)
                    var wpmvh = FindUI(current, "Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost");
                    if (wpmvh == null)
                    {
                        return;
                    }
                    else if (!_isMainWindow)
                    {
                        // maybe floating window
                        foreach (var c in wpmvh.Children())
                        {
                            // set transparent to same level objects
                            await SetTransparentForChildAsync(c, parentName: $"{refd.Name}|{objname}");
                        }
                    }
                }
                else
                {
                    await SetBackgroundToTransparentAsync(current, isTransparent, parentName: refd.FullName);
                }

                if (current is Visual || current is Visual3D)
                {
                    current = VisualTreeHelper.GetParent(current);
                }
                else
                {
                    current = LogicalTreeHelper.GetParent(current);
                }
            }
        }

        private (bool isMainWindow, bool isTargetWindow) IsMainWindow()
        {
            var initial = _view as DependencyObject;
            var current = initial;
            var result = initial;

            while (current != null)
            {
                result = current;
                if (current is Visual || current is Visual3D)
                {
                    current = VisualTreeHelper.GetParent(current);
                }
                else
                {
                    current = LogicalTreeHelper.GetParent(current);
                }
            }

            var fullName = result.GetType().FullName;

            if (fullName.Equals("Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost",
                    StringComparison.OrdinalIgnoreCase) ||
                fullName.Equals("Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextViewHost",
                    StringComparison.OrdinalIgnoreCase))
            {
                // maybe editor with designer area or other view window
                _isRootWindow = true;
            }

            if (fullName.Equals("Microsoft.VisualStudio.PlatformUI.MainWindow", StringComparison.OrdinalIgnoreCase))
            {
                return (true, true);
            }
            else if (fullName.Equals("Microsoft.VisualStudio.Platform.WindowManagement.WindowFrame+ContentHostingPanel",
                         StringComparison.OrdinalIgnoreCase))
            {
                // Mainwindows and non-active tab
                return (true, true);
            }
            else if (
                !fullName.Equals("Microsoft.VisualStudio.PlatformUI.Shell.Controls.FloatingWindow",
                    StringComparison.OrdinalIgnoreCase)
                && !fullName.Equals("Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextView",
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                return (false, !(_settings.IsLimitToMainlyEditorWindow));
            }
            else
            {
                return (false, true);
            }
        }

        private async Task FindWpfTextViewAsync(DependencyObject d)
        {
            if (_wpfTextViewHost == null)
            {
                if (_isRootWindow)
                {
                    _wpfTextViewHost = FindUI(d, "Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextView");
                }
                else
                {
                    _wpfTextViewHost = FindUI(d, "Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost");
                }

                try
                {
                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    if (_wpfTextViewHost == null) return;
                    RenderOptions.SetBitmapScalingMode(_wpfTextViewHost, BitmapScalingMode.Fant);
                }
                catch
                {
                }
            }
        }

        private DependencyObject FindUI(DependencyObject d, string name)
        {
            var current = d;

            while (current != null)
            {
                if (current.GetType().FullName.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return current;
                }

                if (current is Visual || current is Visual3D)
                {
                    current = VisualTreeHelper.GetParent(current);
                }
                else
                {
                    current = LogicalTreeHelper.GetParent(current);
                }
            }

            return null;
        }

        private async Task SetTransparentForChildAsync(DependencyObject d, ParentControlInfo p = null, string parentName = "")
        {
            if (d == null) return;
            foreach (var c in d.Children())
            {
                var tp = new ParentControlInfo()
                {
                    StickyScroll = p?.StickyScroll ?? false,
                    ContentMargin = p?.ContentMargin ?? false
                };
                if (c == null) continue;
                var type = c.GetType();
                if (type == null) continue;
                if (type.FullName.Equals("System.Windows.Controls.Primitives.Thumb", StringComparison.OrdinalIgnoreCase)) return;
                if (type.FullName.Equals("Microsoft.VisualStudio.Text.Utilities.ContainerMargin", StringComparison.OrdinalIgnoreCase))
                {
                    tp.ContentMargin = true;
                }
                if (type.FullName.Equals("Microsoft.VisualStudio.Text.Structure.StickyScroll.StickyScrollMargin", StringComparison.OrdinalIgnoreCase))
                {
                    tp.StickyScroll = true;
                }
                await SetBackgroundToTransparentAsync(c, true, tp, parentName: parentName);
                if (type.FullName.Equals("Microsoft.VisualStudio.Text.Editor.Implementation.AdornmentLayer", StringComparison.OrdinalIgnoreCase)) continue; // stop for childs object
                await SetTransparentForChildAsync(c, tp, parentName: $"{parentName}|{type.Name}|{type.GetProperty("Name")?.GetValue(c)}");
            }
        }

        internal class ParentControlInfo
        {
            public bool StickyScroll;
            public bool ContentMargin;
        }

        private async Task SetBackgroundToTransparentAsync(DependencyObject d, bool isTransparent, ParentControlInfo p = null, string parentName = "")
        {
            var type = d.GetType();
            var name = type?.GetProperty("Name")?.GetValue(d)?.ToString() ?? "";
            if (name.Equals("WhitePadding", StringComparison.OrdinalIgnoreCase)) return;
            if (type?.Name == "TextBlock") return; // maybe caret
            var property = type.GetProperty("Background");
            if (!(property?.GetValue(d) is SolidColorBrush current)) return;
            var c = current.Color;

            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
            try
            {
                if (p?.StickyScroll ?? false)
                {
                    isTransparent = _settings.IsTransparentToStickyScroll;
                }
                else if (p?.ContentMargin ?? false)
                {
                    isTransparent = _settings.IsTransparentToContentMargin;
                }
                var key = $"#{_currentThemeColor.Name}|{parentName}|{type.Name}|{name}|{isTransparent}|{_settings.ExpandToIDE}|{p?.StickyScroll}_{_settings.IsTransparentToStickyScroll}|{p?.ContentMargin}_{_settings.IsTransparentToContentMargin}";
                if (isTransparent)
                {
                    if (!_defaultThemeColor.TryGetValue(key, out var d1))
                    {
                        _defaultThemeColor[key] = current;
                    }
                    if (c.A != 0)
                    {
                        c.A = 0;
                        var b = new SolidColorBrush(c);
                        property.SetValue(d, (Brush)b);
                    }
                }
                else
                {
                    if (_defaultThemeColor.TryGetValue(key, out var d1))
                    {
                        if (c.A == 0 && ((d1 as SolidColorBrush)?.Color.A ?? 0) != 0)
                        {
                            // transparent -> not transparent
                            property.SetValue(d, (SolidColorBrush)d1);
                        }
                    }
                    else
                    {
                        _defaultThemeColor[key] = current;
                    }
                }
            }
            catch
            {
            }
        }

        private void VSColorTheme_ThemeChanged(ThemeChangedEventArgs e)
        {
            _currentThemeColor = VSColorTheme.GetThemedColor(EnvironmentColors.ToolWindowBackgroundColorKey);
        }
    }
}