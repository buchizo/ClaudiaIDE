using System;
using System.ComponentModel.Design;
using ClaudiaIDE.Interfaces;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ClaudiaIDE.MenuCommands
{
    /// <summary>
    ///     Command handler
    /// </summary>
    internal sealed class NextImage
    {
        /// <summary>
        ///     Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        ///     Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid(GuidList.MenuSetId);

        private readonly OleMenuCommand _menuItem;


        private ISkipable activeSkipabe;

        /// <summary>
        ///     Initializes a new instance of the <see cref="NextImage" /> class.
        ///     Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private NextImage(AsyncPackage package, OleMenuCommandService commandService)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            var menuCommandID = new CommandID(CommandSet, CommandId);
            _menuItem = new OleMenuCommand(Execute, menuCommandID);
            _menuItem.BeforeQueryStatus += ReloadSettings;
            commandService.AddCommand(_menuItem);
        }

        /// <summary>
        ///     Gets the instance of the command.
        /// </summary>
        public static NextImage Instance { get; private set; }

        public void ReloadSettings(object sender, EventArgs args)
        {
            if (ProvidersHolder.Instance.ActiveProvider is ISkipable skipable)
                activeSkipabe = skipable;
            else
                activeSkipabe = null;

            _menuItem.Enabled = activeSkipabe != null;
        }

        /// <summary>
        ///     Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in NextImage's constructor requires
            // the UI thread.
            if (Instance != null) return;
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService =
                await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new NextImage(package, commandService);
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
            ThreadHelper.ThrowIfNotOnUIThread();
            activeSkipabe.Skip();
        }
    }
}