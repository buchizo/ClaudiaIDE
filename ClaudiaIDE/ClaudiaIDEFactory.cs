using System;
using System.ComponentModel.Composition;
using ClaudiaIDE.Settings;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

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
			new ClaudiaIDE(textView, new ImageProvider(GetConfigFromVisualStudioSettings()));
		}

        private Setting GetConfigFromVisualStudioSettings()
        {
            try
            {
                var config = new Setting();

                var _DTE2 = (DTE2)ServiceProvider.GetService(typeof(DTE));
                var props = _DTE2.Properties["ClaudiaIDE", "General"];

                config.BackgroundImageAbsolutePath = Setting.ToFullPath(props.Item("BackgroundImageDirectoryAbsolutePath").Value);
                config.Opacity = props.Item("Opacity").Value;
                config.PositionHorizon = (PositionH)props.Item("PositionHorizon").Value;
                config.PositionVertical = (PositionV)props.Item("PositionVertical").Value;
                config.UpdateImageInterval = (TimeSpan) props.Item("UpdateImageInterval").Value;
                config.Extensions = (string)props.Item("Extensions").Value;
                return config;
            }
            catch (Exception)
            {
                return Setting.Deserialize();
            }
        }
    }
	
	#endregion //Adornment Factory
}
