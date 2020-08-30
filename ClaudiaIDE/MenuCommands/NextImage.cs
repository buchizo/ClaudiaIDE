using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Task = System.Threading.Tasks.Task;

namespace ClaudiaIDE.MenuCommands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class NextImage
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f0ffaf7c-8feb-40d2-b898-1acfe50e1d6b");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private readonly Setting _setting;
        private readonly MenuCommand _menuItem;

        /// <summary>
        /// Initializes a new instance of the <see cref="NextImage"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private NextImage(AsyncPackage package, OleMenuCommandService commandService, Setting setting)
        {
            _setting = setting;
            _setting.OnChanged.AddEventHandler(ReloadSettings);
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            var menuCommandID = new CommandID(CommandSet, CommandId);
            _menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(_menuItem);
            ReloadSettings(null, EventArgs.Empty);
        }

        public void ReloadSettings(object sender, EventArgs args)
        {
            _menuItem.Enabled = _setting.ImageBackgroundType == ImageBackgroundType.Slideshow;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static NextImage Instance { get; private set; }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get { return this.package; }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package, Setting setting)
        {
            // Switch to the main thread - the call to AddCommand in NextImage's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            OleMenuCommandService commandService =
                await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new NextImage(package, commandService, setting);
        }

        ~NextImage()
        {
            _setting.OnChanged.RemoveEventHandler(ReloadSettings);
        }

        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var slideshow =
                (SlideShowImageProvider) ProvidersHolder.Instance.Providers.First(p => p.ProviderType == ImageBackgroundType.Slideshow);
            slideshow.NextImage();
        }
    }
}