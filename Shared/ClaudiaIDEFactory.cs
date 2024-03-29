using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using ClaudiaIDE.ImageProviders;
using ClaudiaIDE.Settings;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace ClaudiaIDE
{
    #region Adornment Factory

    /// <summary>
    ///     Establishes an <see cref="IAdornmentLayer" /> to place the adornment on and exports the
    ///     <see cref="IWpfTextViewCreationListener" />
    ///     that instantiates the adornment on the event of a <see cref="IWpfTextView" />'s creation
    /// </summary>
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("text")]
    [ContentType("BuildOutput")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class ClaudiaIDEAdornmentFactory : IWpfTextViewCreationListener
    {
        private List<ImageProvider> ImageProviders;

        [Import(typeof(SVsServiceProvider))] internal IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        ///     Defines the adornment layer for the scarlet adornment. This layer is ordered
        ///     after the selection layer in the Z-order
        /// </summary>
        [Export(typeof(AdornmentLayerDefinition))]
        [Name("ClaudiaIDE")]
        [Order(Before = PredefinedAdornmentLayers.DifferenceChanges)]
        public AdornmentLayerDefinition EditorAdornmentLayer { get; set; }

        /// <summary>
        ///     Instantiates a ClaudiaIDE manager when a textView is created.
        /// </summary>
        /// <param name="textView">The <see cref="IWpfTextView" /> upon which the adornment should be placed</param>
        public void TextViewCreated(IWpfTextView textView)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var settings = Setting.Initialize((DTE)ServiceProvider.GetService(typeof(DTE)));
            var solution = VisualStudioUtility.GetSolutionSettingsFileFullPath();

            if (ImageProviders == null)
            {
                if (ProvidersHolder.Instance.Providers == null)
                {
                    if (string.IsNullOrEmpty(solution))
                    {
                        ProvidersHolder.Initialize(settings, new List<ImageProvider>
                        {
                            new SingleImageEachProvider(settings),
                            new SlideShowImageProvider(settings),
                            new SingleImageProvider(settings),
                            new SingleImageWebProvider(settings),
                            new WebApiImageProvider(settings)
                        });
                    }
                    else
                    {
                        ProvidersHolder.Initialize(settings, new List<ImageProvider>());
                        switch (settings.ImageBackgroundType)
                        {
                            case ImageBackgroundType.Single:
                                ProvidersHolder.Instance.Providers.Add(new SingleImageProvider(settings, solution));
                                break;
                            case ImageBackgroundType.SingleEach:
                                ProvidersHolder.Instance.Providers.Add(new SingleImageEachProvider(settings, solution));
                                break;
                            case ImageBackgroundType.Slideshow:
                                ProvidersHolder.Instance.Providers.Add(new SlideShowImageProvider(settings, solution));
                                break;
                            case ImageBackgroundType.WebSingle:
                                ProvidersHolder.Instance.Providers.Add(new SingleImageWebProvider(settings, solution));
                                break;
                            case ImageBackgroundType.WebApi:
                                ProvidersHolder.Instance.Providers.Add(new WebApiImageProvider(settings, solution));
                                break;
                            default:
                                ProvidersHolder.Instance.Providers.Add(new SingleImageEachProvider(settings, solution));
                                break;
                        }
                    }
                }

                ImageProviders = ProvidersHolder.Instance.Providers;
            }

            if (!string.IsNullOrEmpty(solution))
                if (!ImageProviders.Any(x => x.SolutionConfigFile == solution))
                    switch (settings.ImageBackgroundType)
                    {
                        case ImageBackgroundType.Single:
                            ImageProviders.Add(new SingleImageProvider(settings, solution));
                            break;
                        case ImageBackgroundType.SingleEach:
                            ImageProviders.Add(new SingleImageEachProvider(settings, solution));
                            break;
                        case ImageBackgroundType.Slideshow:
                            ImageProviders.Add(new SlideShowImageProvider(settings, solution));
                            break;
                        case ImageBackgroundType.WebSingle:
                            ImageProviders.Add(new SingleImageWebProvider(settings, solution));
                            break;
                        case ImageBackgroundType.WebApi:
                            ProvidersHolder.Instance.Providers.Add(new WebApiImageProvider(settings, solution));
                            break;
                        default:
                            ImageProviders.Add(new SingleImageEachProvider(settings, solution));
                            break;
                    }

            new ClaudiaIDE(textView, ImageProviders);
        }
    }

    #endregion //Adornment Factory
}