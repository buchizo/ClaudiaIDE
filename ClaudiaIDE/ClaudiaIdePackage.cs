using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using ClaudiaIDE.Options;
using Microsoft.VisualStudio.Shell;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell.Interop;
using ClaudiaIDE.Settings;
using System.Collections.Generic;
using System.Windows.Threading;
using ClaudiaIDE.ImageProvider;
using System.Linq;
using ClaudiaIDE.Helpers;

namespace ClaudiaIDE
{
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.28.6", IconResourceID = 400)]
	[ProvideOptionPage(typeof(ClaudiaIdeOptionPageGrid), "ClaudiaIDE", "General", 110, 116, true)]
	[Guid("7442ac19-889b-4699-a817-e6e054877ee3")]
    [ProvideAutoLoad(UIContextGuids.NoSolution)]
    [ProvideAutoLoad(UIContextGuids.SolutionExists)]
    public sealed class ClaudiaIdePackage : Package
	{
        private Setting _settings;
        private System.Windows.Window _mainWindow;
        private List<IImageProvider> _imageProviders;
        private IImageProvider _imageProvider;
        private readonly Dispatcher _dispacher;
        private Image _current = null;

        public ClaudiaIdePackage()
		{
            _dispacher = Dispatcher.CurrentDispatcher;
        }

        protected override void Initialize()
		{
            Application.Current.MainWindow.Loaded += (s,e) =>
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

            base.Initialize();
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
            _dispacher.Invoke(ChangeImage);
        }

    }
}
