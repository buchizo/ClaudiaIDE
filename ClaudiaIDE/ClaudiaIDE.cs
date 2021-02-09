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
using System.Windows.Media.Media3D;

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
        private DependencyObject _wpfTextViewHost = null;
        private Dictionary<int, DependencyObject> _defaultThemeColor = new Dictionary<int, DependencyObject>();
        private bool _hasImage = false;
        private bool _isRootWindow = false;

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
                FindWpfTextView(_editorCanvas as DependencyObject);
                if (_wpfTextViewHost == null) return;

                var newimage = _imageProvider.GetBitmap();
                var opacity = _settings.ExpandToIDE && _isMainWindow ? 0.0 : _settings.Opacity;

                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    try
                    {
                        if (_isRootWindow)
                        {
                            var grid = new Grid()
                            {
                                Name = "ClaudiaIdeImage",
                                HorizontalAlignment = HorizontalAlignment.Stretch,
                                VerticalAlignment = VerticalAlignment.Stretch,
                                IsHitTestVisible = false
                            };
                            var ni = new Image()
                            {
                                Source = newimage,
                                Stretch = _settings.ImageStretch.ConvertTo(),
                                HorizontalAlignment = _settings.PositionHorizon.ConvertToHorizontalAlignment(),
                                VerticalAlignment = _settings.PositionVertical.ConvertToVerticalAlignment(),
                                Opacity = opacity,
                                IsHitTestVisible = false
                            };
                            grid.Children.Insert(0, ni);
                            Grid.SetRowSpan(grid, 3);
                            Grid.SetColumnSpan(grid, 3);
                            var p = VisualTreeHelper.GetParent(_wpfTextViewHost) as Grid;
                            if(p != null)
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
                            var nib = new ImageBrush(newimage)
                            {
                                Stretch = _settings.ImageStretch.ConvertTo(),
                                AlignmentX = _settings.PositionHorizon.ConvertTo(),
                                AlignmentY = _settings.PositionVertical.ConvertTo(),
                                Opacity = opacity,
                                Viewbox = new Rect(new Point(_settings.ViewBoxPointX, _settings.ViewBoxPointY), new Size(1,1))
                            };
                            _wpfTextViewHost.SetValue(Panel.BackgroundProperty, nib);
                        }
                        _hasImage = true;
                        if (_settings.ImageBackgroundType == ImageBackgroundType.SingleEach)
                            ((SingleImageEachProvider)_imageProvider).NextImage();
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
            FindWpfTextView(_editorCanvas as DependencyObject);
            if (_wpfTextViewHost == null) return;
            var opacity = _settings.ExpandToIDE && _isMainWindow ? 0.0 : _settings.Opacity;

            var refd = _wpfTextViewHost.GetType();
            var prop = refd.GetProperty("Background");
            var background = prop.GetValue(_wpfTextViewHost) as ImageBrush;
            if (background == null && _isRootWindow)
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
                {
                    await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    try
                    {
                        var c = (_wpfTextViewHost as UIElement).Opacity;
                        (_wpfTextViewHost as UIElement).Opacity = c < 0.01 ? 0.01 : c - 0.01;
                        (_wpfTextViewHost as UIElement).Opacity = c;
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
                        background.Opacity = opacity < 0.01 ? 0.01 : opacity - 0.01;
                        background.Opacity = opacity;
                    }
                    catch
                    {
                    }
                });
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
            _isMainWindow = IsMainWindow();
            var isTransparent = true;
            var current = _editorCanvas as DependencyObject;

            while (current != null)
            {
                var refd = current.GetType();
                var nameprop = refd.GetProperty("Name");
                var objname = nameprop?.GetValue(current) as string;

                if (!string.IsNullOrEmpty(objname) && (objname.Equals("RootGrid", StringComparison.OrdinalIgnoreCase) || objname.Equals("MainWindow", StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                if (_isRootWindow && refd.FullName.Equals("Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextView", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (refd.FullName.Equals("Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost", StringComparison.OrdinalIgnoreCase))
                {
                    isTransparent = _settings.ExpandToIDE && _isMainWindow;
                }
                else if (refd.FullName.Equals("Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextView", StringComparison.OrdinalIgnoreCase))
                {
                    // for visualize history(using Gource)
                    if (FindUI(current, "Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost") == null)
                    {
                        return;
                    }
                }
                else
                {
                    SetBackgroundToTransparent(current, isTransparent);
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

        private bool IsMainWindow()
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

            if (result.GetType().FullName.Equals("Microsoft.VisualStudio.Editor.Implementation.WpfMultiViewHost", StringComparison.OrdinalIgnoreCase) ||
                result.GetType().FullName.Equals("Microsoft.VisualStudio.Text.Editor.Implementation.WpfTextViewHost", StringComparison.OrdinalIgnoreCase))
            {
                // maybe editor with designer area or other view window
                _isRootWindow = true;
            }

            if (result.GetType().FullName.Equals("Microsoft.VisualStudio.PlatformUI.MainWindow", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private void FindWpfTextView(DependencyObject d)
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

        private void SetBackgroundToTransparent(DependencyObject d, bool isTransparent)
        {
            var property = d.GetType().GetProperty("Background");
            var current = property?.GetValue(d) as Brush;
            if (current == null) return;

            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                try
                {
                    if (isTransparent)
                    {
                        if (!_defaultThemeColor.Any(x => x.Key == d.GetHashCode()))
                        {
                            _defaultThemeColor[d.GetHashCode()] = current as DependencyObject;
                        }
                        property.SetValue(d, (Brush)Brushes.Transparent);
                    }
                    else
                    {
                        var d1 = _defaultThemeColor.FirstOrDefault(x => x.Key == current.GetHashCode());
                        if (d1.Value != null)
                        {
                            property.SetValue(d, (Brush)d1.Value);
                        }
                    }
                }
                catch
                {
                }
            });
        }
    }
}
