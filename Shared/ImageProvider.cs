using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;

namespace ClaudiaIDE
{
    public abstract class ImageProvider
    {
        protected readonly Setting Setting;
        protected BitmapSource Image;

        public ImageProvider(Setting setting, string solutionConfigFile, ImageBackgroundType providerType)
        {
            Setting = setting;
            Setting.OnChanged.AddEventHandler(OnSettingChanged);
            SolutionConfigFile = solutionConfigFile;
            ProviderType = providerType;
        }

        public ImageBackgroundType ProviderType { get; }

        public string SolutionConfigFile { get; }

        public bool IsActive { get; set; }

        public virtual BitmapSource GetBitmap()
        {
            return Image;
        }

        public event EventHandler NewImageAvailable;

        ~ImageProvider()
        {
            Setting.OnChanged.RemoveEventHandler(OnSettingChanged);
        }

        protected void FireImageAvailable()
        {
            NewImageAvailable?.Invoke(this, EventArgs.Empty);
        }

        protected abstract void OnSettingChanged(object sender, EventArgs e);
    }

    public class ProvidersHolder
    {
        private static readonly Lazy<ProvidersHolder>
            _instance = new Lazy<ProvidersHolder>(() => new ProvidersHolder());

        private ProvidersHolder()
        {
        }

        public static ProvidersHolder Instance => _instance.Value;

        public List<ImageProvider> Providers { get; private set; }

        public ImageProvider ActiveProvider
        {
            get { return Providers.FirstOrDefault(provider => provider.IsActive); }
        }

        public static void Initialize(Setting settings, List<ImageProvider> providers)
        {
            if (_instance.Value.Providers == null) _instance.Value.Providers = providers;
        }
    }
}