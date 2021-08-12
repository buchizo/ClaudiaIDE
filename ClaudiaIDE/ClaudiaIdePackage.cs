using System.Runtime.InteropServices;
using ClaudiaIDE.Options;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using System.Windows.Media;
using System;
using System.Windows.Controls;
using ClaudiaIDE.Settings;
using System.Collections.Generic;
using ClaudiaIDE.ImageProvider;
using System.Linq;
using ClaudiaIDE.Helpers;
using Task = System.Threading.Tasks.Task;
using ClaudiaIDE.MenuCommands;
using EnvDTE;

namespace ClaudiaIDE
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [InstalledProductRegistration("#110", "#112", "3.0.0.8", IconResourceID = 400)]
    [ProvideOptionPage(typeof(ClaudiaIdeOptionPageGrid), "ClaudiaIDE", "General", 110, 116, true)]
    [Guid("7442ac19-889b-4699-a817-e6e054877ee3")]
    [ProvideAutoLoad("{ADFC4E65-0397-11D1-9F4E-00A0C911004F}", PackageAutoLoadFlags.BackgroundLoad)] //UIContextGuids.EmptySolution
    [ProvideAutoLoad("{ADFC4E64-0397-11D1-9F4E-00A0C911004F}", PackageAutoLoadFlags.BackgroundLoad)] //UIContextGuids.NoSolution
    [ProvideAutoLoad("{F1536EF8-92EC-443C-9ED7-FDADF150DA82}", PackageAutoLoadFlags.BackgroundLoad)] //UIContextGuids.SolutionExists
    [ProvideMenuResource("Menus.ctmenu", 1)]
    public sealed class ClaudiaIdePackage : AsyncPackage
    {
        private Setting _settings;
        private System.Windows.Window _mainWindow;
        private List<IImageProvider> _imageProviders;
        private IImageProvider _imageProvider;
        private Image _current = null;

        protected override async Task InitializeAsync(System.Threading.CancellationToken cancellationToken,
            IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            Application.Current.MainWindow.Loaded += (s, e) =>
            {
                Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
                _mainWindow = (System.Windows.Window) s;
                _settings = Setting.Initialize((DTE)this.GetService(typeof(DTE)));
                _settings.OnChanged.AddEventHandler(ReloadSettings);
                if (ProvidersHolder.Instance.Providers == null)
                {
                    ProvidersHolder.Initialize(_settings, new List<IImageProvider>
                    {
                        new SingleImageEachProvider(_settings),
                        new SlideShowImageProvider(_settings),
                        new SingleImageProvider(_settings)
                    });
                }

                _imageProviders = ProvidersHolder.Instance.Providers;
                _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);
                _imageProviders.ForEach(x => x.NewImageAvaliable += InvokeChangeImage);
                
                NextImage.InitializeAsync(this)
                    .FileAndForget("claudiaide/nextimage/initializeasync");
                PauseSlideshow.InitializeAsync(this).FileAndForget("claudiaide/pauseslideshow/initializeasync");
                SaveSolutionSettings.InitializeAsync(this, _settings).FileAndForget("claudiaide/saveSolutionSettings/initializeasync"); ;
                ReloadSettings(null, null);
            };
            Application.Current.MainWindow.Closing += (s, e) =>
            {
                _imageProviders.ForEach(x => x.NewImageAvaliable -= InvokeChangeImage);
                if (_settings != null)
                {
                    _settings.OnChanged.RemoveEventHandler(ReloadSettings);
                }
            };
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
            catch { }
        }

        private void ChangeImage()
        {
            try
            {
                var rRootGrid = (Grid) _mainWindow.Template.FindName("RootGrid", _mainWindow);
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

                    var newimage = _imageProvider.GetBitmap();
                    if (_settings.ImageBackgroundType == ImageBackgroundType.Single || _current == null)
                    {
                        var rImageControl = new Image()
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

                                prop.SetValue(g, new SolidColorBrush(new Color()
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
                            (n) =>
                            {
                                n.Stretch = _settings.ImageStretch.ConvertTo();
                                n.HorizontalAlignment = _settings.PositionHorizon.ConvertToHorizontalAlignment();
                                n.VerticalAlignment = _settings.PositionVertical.ConvertToVerticalAlignment();
                            },
                            new Helpers.AnimateImageChangeParams
                            {
                                FadeTime = _settings.ImageFadeAnimationInterval,
                                TargetOpacity = _settings.Opacity
                            }
                        );
                    }
                }
            }
            catch { }
        }

        private void ReloadSettings(object sender, System.EventArgs e)
        {
            _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);
            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.Run(async () =>
            {
                await Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                ChangeImage();
            });
        }
    }
}