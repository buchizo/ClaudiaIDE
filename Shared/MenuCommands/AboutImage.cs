using System;
using System.ComponentModel.Design;
using ClaudiaIDE.Forms;
using ClaudiaIDE.ImageProviders;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ClaudiaIDE.MenuCommands
{
    /// <summary>
    ///     Command handler
    /// </summary>
    internal sealed class AboutImage
    {
        /// <summary>
        ///     Command ID.
        /// </summary>
        public const int CommandId = 0x0160;

        /// <summary>
        ///     Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid(GuidList.MenuSetId);

        private readonly OleMenuCommand _menuItem;
        private readonly AsyncPackage _package;

        private AboutImage(AsyncPackage package, OleMenuCommandService commandService)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            var menuCommandID = new CommandID(CommandSet, CommandId);
            _package = package;
            _menuItem = new OleMenuCommand(Execute, menuCommandID);
            _menuItem.BeforeQueryStatus += (s, e) =>
            {
                ImageProvider provider = ProvidersHolder.Instance.ActiveProvider;
                // It is unavailable with the SingleEach image type as the provider does not track the image within each individual pane.
                _menuItem.Enabled = !(provider is SingleImageEachProvider);
            };
            commandService.AddCommand(_menuItem);
        }

        /// <summary>
        ///     Gets the instance of the command.
        /// </summary>
        public static AboutImage Instance { get; private set; }

        /// <summary>
        ///     Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in AboutImage's constructor requires
            // the UI thread.
            if (Instance != null) return;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService =
                await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new AboutImage(package, commandService);
        }

        /// <summary>
        ///     This function is the callback used to execute the command when the menu item is clicked.
        ///     See the constructor to see how the menu item is associated with this function using
        ///     OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ImageProvider provider = ProvidersHolder.Instance.ActiveProvider;
            if (provider == null) return;

            using (AboutImageForm form = new AboutImageForm(provider))
            {
                form.ShowDialog();
            }
        }
    }
}