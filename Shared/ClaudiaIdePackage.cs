using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Task = System.Threading.Tasks.Task;
using System.Windows.Media;
using ClaudiaIDE.Helpers;
using ClaudiaIDE.ImageProviders;
using ClaudiaIDE.MenuCommands;
using ClaudiaIDE.Options;
using ClaudiaIDE.Settings;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Window = System.Windows.Window;
using Microsoft.VisualStudio.Threading;
using System.Windows.Media.Effects;

namespace ClaudiaIDE
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "3.1.23", IconResourceID = 400)]
    [ProvideOptionPage(typeof(ClaudiaIdeOptionPageGrid), "ClaudiaIDE", "Light theme", 110, 116, true)]
    [ProvideOptionPage(typeof(ClaudiaIdeDarkThemeOptionPageGrid), "ClaudiaIDE", "Dark theme", 110, 117, true)]
    [ProvideOptionPage(typeof(ClaudiaIdeGeneralOptionPageGrid), "ClaudiaIDE", "General", 110, 118, true)]
    [Guid(GuidList.PackageId)]
    [ProvideAutoLoad("{ADFC4E65-0397-11D1-9F4E-00A0C911004F}",
        PackageAutoLoadFlags.BackgroundLoad)] //UIContextGuids.EmptySolution
    [ProvideAutoLoad("{ADFC4E64-0397-11D1-9F4E-00A0C911004F}",
        PackageAutoLoadFlags.BackgroundLoad)] //UIContextGuids.NoSolution
    [ProvideAutoLoad("{F1536EF8-92EC-443C-9ED7-FDADF150DA82}",
        PackageAutoLoadFlags.BackgroundLoad)] //UIContextGuids.SolutionExists
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ClaudiaIdePackage : AsyncPackage
    {
        private Image _current;
        private MediaElement _currentMediaElement;
        private ImageProvider _imageProvider;
        private List<ImageProvider> _imageProviders;
        private Window _mainWindow;
        private Setting _settings;

        protected override async Task InitializeAsync(CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            Application.Current.MainWindow.Loaded += (s, e) =>
            {
                ThreadHelper.ThrowIfNotOnUIThread();
                _mainWindow = (Window)s;
                _settings = Setting.Initialize((DTE)GetService(typeof(DTE)));
                _settings.OnChanged.AddEventHandler(InvokeChangeImage);
                if (ProvidersHolder.Instance.Providers == null)
                    ProvidersHolder.Initialize(_settings, new List<ImageProvider>
                    {
                        new SingleImageEachProvider(_settings),
                        new SlideShowImageProvider(_settings),
                        new SingleImageProvider(_settings),
                        new SingleImageWebProvider(_settings),
                        new WebApiImageProvider(_settings)
                    });

                _imageProviders = ProvidersHolder.Instance.Providers;
                _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);
                _imageProvider.IsActive = true;
                _imageProviders.ForEach(x => x.NewImageAvailable += InvokeChangeImage);

                NextImage.InitializeAsync(this)
                    .FileAndForget("claudiaide/nextimage/initializeasync");
                PauseSlideshow.InitializeAsync(this).FileAndForget("claudiaide/pauseslideshow/initializeasync");
                SaveSolutionSettings.InitializeAsync(this, _settings)
                    .FileAndForget("claudiaide/saveSolutionSettings/initializeasync");
                ResetSolutionSettings.InitializeAsync(this, _settings)
                    .FileAndForget("claudiaide/resetSolutionSettings/initializeasync");
                ToggleHiddenImage.InitializeAsync(this)
                    .FileAndForget("claudiaide/toggleHiddenImage/initializeasync");
                InvokeChangeImage(null, null);
            };
            Application.Current.MainWindow.Closing += (s, e) =>
            {
                _imageProviders.ForEach(x => x.NewImageAvailable -= InvokeChangeImage);
                if (_settings != null) _settings.OnChanged.RemoveEventHandler(InvokeChangeImage);
            };
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await JoinableTaskFactory.SwitchToMainThreadAsync(true, cancellationToken);
                    _settings = Setting.Initialize((DTE)await GetServiceAsync(typeof(DTE)));
                    if (_settings == null) return;
                    _mainWindow = Application.Current.MainWindow;
                    _settings.OnChanged.AddEventHandler(InvokeChangeImage);
                    if (ProvidersHolder.Instance.Providers == null)
                        ProvidersHolder.Initialize(_settings, new List<ImageProvider>
                        {
                            new SingleImageEachProvider(_settings),
                            new SlideShowImageProvider(_settings),
                            new SingleImageProvider(_settings),
                            new SingleImageWebProvider(_settings),
                            new WebApiImageProvider(_settings)
                        });

                    _imageProviders = ProvidersHolder.Instance.Providers;
                    _imageProvider =
                        _imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);
                    _imageProvider.IsActive = true;
                    _imageProviders.ForEach(x => x.NewImageAvailable += InvokeChangeImage);

                    await NextImage.InitializeAsync(this);
                    await PauseSlideshow.InitializeAsync(this);
                    await SaveSolutionSettings.InitializeAsync(this, _settings);
                    await ResetSolutionSettings.InitializeAsync(this, _settings);
                    await ToggleHiddenImage.InitializeAsync(this);
                    InvokeChangeImage(null, null);
                }
                catch
                {
                }
            }).FileAndForget("claudiaide/initializeasync");

            MetroRadiance.Platform.WindowsTheme.Theme.Changed += (sender, e) =>
            {
                Setting.DefaultInstance.OnColorThemeChange();
            };
        }

        private void InvokeChangeImage(object sender, EventArgs e)
        {
            ChangeImageAsync().Forget();
        }

        private async Task ChangeImageAsync()
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                ChangeImage();
            }
            catch
            {
            }
        }

        private void ChangeImage()
        {
            try
            {
                var rRootGrid = (Grid)_mainWindow.Template.FindName("RootGrid", _mainWindow);
                if (rRootGrid != null)
                {
                    var removeTargets = new List<UIElement>();
                    foreach (UIElement el in rRootGrid.Children)
                    {
                        if (el.GetType() == typeof(Image))
                        {
                            if (_settings.ImageBackgroundType == ImageBackgroundType.Single
                                || !_settings.ExpandToIDE
                                || ProvidersHolder.Instance.ActiveProvider?.IsStaticImage() == false)
                            {
                                removeTargets.Add(el);
                                _current = null;
                            }
                            else
                            {
                                _currentMediaElement = null;
                                _current = el as Image;
                            }
                        }
                        if (el.GetType() == typeof(MediaElement))
                        {
                            if (_settings.ImageBackgroundType == ImageBackgroundType.Single
                                || !_settings.ExpandToIDE
                                || ProvidersHolder.Instance.ActiveProvider?.IsStaticImage() == true)
                            {
                                removeTargets.Add(el);
                                _currentMediaElement = null;
                            }
                            else
                            {
                                _current = null;
                                _currentMediaElement = el as MediaElement;
                            }
                        }
                    }
                    removeTargets.ForEach(x => rRootGrid.Children.Remove(x));

                    if (!_settings.ExpandToIDE) return;

                    if (ProvidersHolder.Instance.ActiveProvider?.IsStaticImage() == true)
                    {
                        _currentMediaElement = null;
                        var newimage = ProvidersHolder.Instance.ActiveProvider?.GetBitmap();
                        if (_settings.ImageBackgroundType == ImageBackgroundType.Single || _current == null)
                        {
                            var rImageControl = new Image
                            {
                                Source = newimage,
                                Stretch = _settings.ImageStretch.ConvertTo(),
                                HorizontalAlignment = _settings.PositionHorizon.ConvertToHorizontalAlignment(),
                                VerticalAlignment = _settings.PositionVertical.ConvertToVerticalAlignment(),
                                Opacity = _settings.IsHidden ? 0.0 : _settings.Opacity,
                                Effect = _settings.BlurRadius > 0 ? new BlurEffect()
                                {
                                    Radius = _settings.BlurRadius
                                } : null
                            };

                            Grid.SetRowSpan(rImageControl, 4);
                            RenderOptions.SetBitmapScalingMode(rImageControl, BitmapScalingMode.Fant);

                            rRootGrid.Children.Insert(0, rImageControl);
                            SetTransparentBackground(rRootGrid);
                        }
                        else
                        {
                            _current.AnimateImageSourceChange(
                                newimage,
                                n =>
                                {
                                    n.Stretch = _settings.ImageStretch.ConvertTo();
                                    n.HorizontalAlignment = _settings.PositionHorizon.ConvertToHorizontalAlignment();
                                    n.VerticalAlignment = _settings.PositionVertical.ConvertToVerticalAlignment();
                                },
                                new AnimateImageChangeParams
                                {
                                    FadeTime = _settings.ImageFadeAnimationInterval,
                                    TargetOpacity = _settings.IsHidden ? 0.0 : _settings.Opacity
                                }
                            );
                        }
                    }
                    else
                    {
                        // movie, animation gif..
                        if (_currentMediaElement == null)
                        {
                            _currentMediaElement = new MediaElement
                            {
                                Source = new Uri(ProvidersHolder.Instance.ActiveProvider?.GetCurrentImageUri()),
                                LoadedBehavior = MediaState.Play,
                                UnloadedBehavior = MediaState.Manual,
                                IsMuted = true,
                                HorizontalAlignment = _settings.PositionHorizon.ConvertToHorizontalAlignment(),
                                VerticalAlignment = _settings.PositionVertical.ConvertToVerticalAlignment(),
                                Stretch = _settings.ImageStretch.ConvertTo(),
                                Opacity = _settings.Opacity,
                                Effect = _settings.BlurRadius > 0 ? new BlurEffect()
                                {
                                    Radius = _settings.BlurRadius,
                                    KernelType = KernelType.Box
                                } : null
                            };
                            _currentMediaElement.MediaEnded += (s, e) =>
                            {
                                if (_currentMediaElement == null) return;
                                _currentMediaElement.Position = TimeSpan.FromMilliseconds(1);
                                _currentMediaElement.Play();
                            };
                            Grid.SetRowSpan(_currentMediaElement, 4);
                            RenderOptions.SetBitmapScalingMode(_currentMediaElement, BitmapScalingMode.Fant);
                            rRootGrid.Children.Insert(0, _currentMediaElement);
                            SetTransparentBackground(rRootGrid);
                        }
                        else
                        {
                            _currentMediaElement.Source = new Uri(ProvidersHolder.Instance.ActiveProvider?.GetCurrentImageUri());
                        }
                    }
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// mainwindow background set to transparent
        /// </summary>
        /// <param name="rRootGrid"></param>
        public static void SetTransparentBackground(Grid rRootGrid)
        {
            var docktargets = rRootGrid.Descendants<DependencyObject>().Where(x =>
                x.GetType().FullName == "Microsoft.VisualStudio.PlatformUI.Shell.Controls.DockTarget");
            foreach (var docktarget in docktargets)
            {
                var grids = docktarget?.Descendants<Grid>();
                foreach (var g in grids)
                {
                    try
                    {
                        if (g == null) continue;
                        var prop = g.GetType().GetProperty("Background");
                        if (prop.GetValue(g) is LinearGradientBrush)
                        {
                            prop.SetValue(g, new SolidColorBrush(new Color
                            {
                                A = 0x00
                            }));
                            continue;
                        }
                        if (!(prop.GetValue(g) is SolidColorBrush bg) || bg.Color.A == 0x00) continue;

                        prop.SetValue(g, new SolidColorBrush(new Color
                        {
                            A = 0x00,
                            B = bg.Color.B,
                            G = bg.Color.G,
                            R = bg.Color.R
                        }));
                    }
                    catch { }
                }
            }
        }
    }
}