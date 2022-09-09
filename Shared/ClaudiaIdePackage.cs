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

namespace ClaudiaIDE
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "3.1.2", IconResourceID = 400)]
    [ProvideOptionPage(typeof(ClaudiaIdeOptionPageGrid), "ClaudiaIDE", "General", 110, 116, true)]
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
                ;
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
                    InvokeChangeImage(null, null);
                }
                catch
                {
                }
            }).FileAndForget("claudiaide/initializeasync");
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
                    foreach (UIElement el in rRootGrid.Children)
                    {
                        if (el.GetType() != typeof(Image)) continue;
                        if (_current == null) _current = el as Image;
                        if (_settings.ImageBackgroundType == ImageBackgroundType.Single || !_settings.ExpandToIDE)
                        {
                            rRootGrid.Children.Remove(el);
                            _current = null;
                        }

                        break;
                    }

                    if (!_settings.ExpandToIDE) return;

                    var newimage = ProvidersHolder.Instance.ActiveProvider.GetBitmap();
                    if (_settings.ImageBackgroundType == ImageBackgroundType.Single || _current == null)
                    {
                        var rImageControl = new Image
                        {
                            Source = newimage,
                            Stretch = _settings.ImageStretch.ConvertTo(),
                            HorizontalAlignment = _settings.PositionHorizon.ConvertToHorizontalAlignment(),
                            VerticalAlignment = _settings.PositionVertical.ConvertToVerticalAlignment(),
                            Opacity = _settings.Opacity
                        };

                        Grid.SetRowSpan(rImageControl, 4);
                        RenderOptions.SetBitmapScalingMode(rImageControl, BitmapScalingMode.Fant);

                        rRootGrid.Children.Insert(0, rImageControl);

                        // mainwindow background set to transparent
                        var docktargets = rRootGrid.Descendants<DependencyObject>().Where(x =>
                            x.GetType().FullName == "Microsoft.VisualStudio.PlatformUI.Shell.Controls.DockTarget");
                        foreach (var docktarget in docktargets)
                        {
                            var grids = docktarget?.Descendants<Grid>();
                            foreach (var g in grids)
                            {
                                if (g == null) continue;
                                var prop = g.GetType().GetProperty("Background");
                                if (!(prop.GetValue(g) is SolidColorBrush bg) || bg.Color.A == 0x00) continue;

                                prop.SetValue(g, new SolidColorBrush(new Color
                                {
                                    A = 0x00,
                                    B = bg.Color.B,
                                    G = bg.Color.G,
                                    R = bg.Color.R
                                }));
                            }
                        }
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
                                TargetOpacity = _settings.Opacity
                            }
                        );
                    }
                }
            }
            catch
            {
            }
        }
    }
}