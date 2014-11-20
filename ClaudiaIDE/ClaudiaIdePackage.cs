using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using ClaudiaIDE.Options;
using Microsoft.VisualStudio.Shell;

namespace ClaudiaIDE
{
	[PackageRegistration(UseManagedResourcesOnly = true)]
	[InstalledProductRegistration("#110", "#112", "1.15", IconResourceID = 400)]
	[ProvideOptionPage(typeof(ClaudiaIdeOptionPageGrid), "ClaudiaIDE", "General", 110, 116, true)]
	[Guid("7442ac19-889b-4699-a817-e6e054877ee3")]
	public sealed class ClaudiaIdePackage : Package
	{
		public ClaudiaIdePackage()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
		}

		protected override void Initialize()
		{
			Debug.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
			base.Initialize();
		}
	}
}
