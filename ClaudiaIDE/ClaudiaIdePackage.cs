using System.Runtime.InteropServices;
using ClaudiaIDE.Options;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using System.Windows.Media;
using System;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell.Interop;
using ClaudiaIDE.Settings;
using System.Collections.Generic;
using ClaudiaIDE.ImageProvider;
using System.Linq;
using ClaudiaIDE.Helpers;
using Task = System.Threading.Tasks.Task;

namespace ClaudiaIDE
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[InstalledProductRegistration("#110", "#112", "2.0.2", IconResourceID = 400)]
	[ProvideOptionPage(typeof(ClaudiaIdeOptionPageGrid), "ClaudiaIDE", "General", 110, 116, true)]
	[Guid("7442ac19-889b-4699-a817-e6e054877ee3")]
    [ProvideAutoLoad(UIContextGuids.EmptySolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids.NoSolution, PackageAutoLoadFlags.BackgroundLoad)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class ClaudiaIdePackage : AsyncPackage
	{
        private Setting _settings;
        private System.Windows.Window _mainWindow;
        private List<IImageProvider> _imageProviders;
        private IImageProvider _imageProvider;
        private Image _current = null;

        protected override async Task InitializeAsync(System.Threading.CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
            Application.Current.MainWindow.Loaded += (s, e) =>
            {
                _mainWindow = (System.Windows.Window)s;
                _settings = Setting.Initialize(this);
                _settings.OnChanged.AddEventHandler(ReloadSettings);
                if (ProvidersHolder.Instance.Providers == null)
                {
                    ProvidersHolder.Initialize(_settings, new List<IImageProvider>
                    {
                        new SildeShowImageProvider(_settings),
                        new SingleImageProvider(_settings)
                    });
                }
                _imageProviders = ProvidersHolder.Instance.Providers;
                _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);
                _imageProviders.ForEach(x => x.NewImageAvaliable += InvokeChangeImage);

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
            Microsoft.VisualStudio.Shell.ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    await this.JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
                    _settings = await Setting.InitializeAsync(this);
                    if (_settings == null) return;
                    _mainWindow = (System.Windows.Window)Application.Current.MainWindow;
                    _settings.OnChanged.AddEventHandler(ReloadSettings);
                    if (ProvidersHolder.Instance.Providers == null)
                    {
                        ProvidersHolder.Initialize(_settings, new List<IImageProvider>
                    {
                        new SildeShowImageProvider(_settings),
                        new SingleImageProvider(_settings)
                    });
                    }
                    _imageProviders = ProvidersHolder.Instance.Providers;
                    _imageProvider = _imageProviders.FirstOrDefault(x => x.ProviderType == _settings.ImageBackgroundType);
                    _imageProviders.ForEach(x => x.NewImageAvaliable += InvokeChangeImage);

                    ReloadSettings(null, null);
                }
                catch
                {
                }
            }).FileAndForget("claudiaide/initializeasync");
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

        private void ChangeImage()
        {
            try
            {
                var rRootGrid = (Grid)_mainWindow.Template.FindName("RootGrid", _mainWindow);
                var newimage = _imageProvider.GetBitmap();

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
                }
                else
                {
                    _current.AnimateImageSourceChange(
                            newimage,
                            (n) => {
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
            catch
            {
            }
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
