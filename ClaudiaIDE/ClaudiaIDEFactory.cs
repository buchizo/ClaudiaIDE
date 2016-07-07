using System;
using System.ComponentModel.Composition;
using ClaudiaIDE.ImageProvider;
using ClaudiaIDE.Settings;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.Collections.Generic;

namespace ClaudiaIDE
{
	#region Adornment Factory
	/// <summary>
	/// Establishes an <see cref="IAdornmentLayer"/> to place the adornment on and exports the <see cref="IWpfTextViewCreationListener"/>
	/// that instantiates the adornment on the event of a <see cref="IWpfTextView"/>'s creation
	/// </summary>
	[Export(typeof(IWpfTextViewCreationListener))]
	[ContentType("text")]
	[TextViewRole(PredefinedTextViewRoles.Document)]
	internal sealed class ClaudiaIDEAdornmentFactory : IWpfTextViewCreationListener
	{
        private List<IImageProvider> ImageProviders;
          
        [Import(typeof(SVsServiceProvider))]
		internal System.IServiceProvider ServiceProvider { get; set; }
		
		/// <summary>
		/// Defines the adornment layer for the scarlet adornment. This layer is ordered 
		/// after the selection layer in the Z-order
		/// </summary>
		[Export(typeof(AdornmentLayerDefinition))]
		[Name("ClaudiaIDE")]
		[Order(Before = PredefinedAdornmentLayers.DifferenceChanges)]
		public AdornmentLayerDefinition EditorAdornmentLayer { get; set; }

		/// <summary>
		/// Instantiates a ClaudiaIDE manager when a textView is created.
		/// </summary>
		/// <param name="textView">The <see cref="IWpfTextView"/> upon which the adornment should be placed</param>
		public void TextViewCreated(IWpfTextView textView)
		{
		    var settings = Setting.Initialize(ServiceProvider);
            if(ImageProviders == null)
            {
                ImageProviders = new List<IImageProvider>
                {
                    new SildeShowImageProvider(settings),
                    new SingleImageProvider(settings)
                };
            }

		    new ClaudiaIDE(textView, ImageProviders);
		}
    }
	
	#endregion //Adornment Factory
}
