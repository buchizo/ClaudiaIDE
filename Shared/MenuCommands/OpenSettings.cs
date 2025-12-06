using System;
using System.ComponentModel.Design;
using ClaudiaIDE.Options;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ClaudiaIDE.MenuCommands
{
    /// <summary>
    ///     Command handler
    /// </summary>
    internal sealed class OpenSettings
    {
        /// <summary>
        ///     Command ID.
        /// </summary>
        public const int CommandId = 0x0170;

        /// <summary>
        ///     Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid(GuidList.MenuSetId);

        private readonly MenuCommand _menuItem;
        private readonly AsyncPackage _package;

        private OpenSettings(AsyncPackage package, OleMenuCommandService commandService)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            var menuCommandID = new CommandID(CommandSet, CommandId);
            _package = package;
            _menuItem = new MenuCommand(Execute, menuCommandID)
            {
                Enabled = true
            };
            commandService.AddCommand(_menuItem);
        }

        /// <summary>
        ///     Gets the instance of the command.
        /// </summary>
        public static OpenSettings Instance { get; private set; }

        /// <summary>
        ///     Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in OpenSettings's constructor requires
            // the UI thread.
            if (Instance != null) return;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService =
                await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new OpenSettings(package, commandService);
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
            Instance._package?.ShowOptionPage(typeof(ClaudiaIdeGeneralOptionPageGrid));
        }
    }
}