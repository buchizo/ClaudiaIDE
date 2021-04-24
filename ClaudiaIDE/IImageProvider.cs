using System;
using System.Windows.Media.Imaging;
using ClaudiaIDE.Settings;
using System.Collections.Generic;

namespace ClaudiaIDE
{
    public interface IImageProvider
    {
        BitmapSource GetBitmap();
        event EventHandler NewImageAvaliable;
        ImageBackgroundType ProviderType { get; }
        string SolutionConfigFile { get; }
    }

    public class ProvidersHolder
    {
        private ProvidersHolder() { }
        private static Lazy<ProvidersHolder> _instance = new Lazy<ProvidersHolder>(() => new ProvidersHolder());

        public static ProvidersHolder Instance
        {
            get { return _instance.Value; }
        }

        public static void Initialize(Setting settings, List<IImageProvider> providers)
        {
            if (_instance.Value.Providers == null)
            {
                _instance.Value.Providers = providers;
            }
        }

        public List<IImageProvider> Providers { get; private set; }
    }
}