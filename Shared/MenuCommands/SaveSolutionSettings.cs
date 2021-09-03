using System;
using System.ComponentModel.Design;
using System.IO;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ClaudiaIDE.MenuCommands
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class SaveSolutionSettings
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0130;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f0ffaf7c-8feb-40d2-b898-1acfe50e1d6b");

        private readonly Setting _setting;
        private readonly MenuCommand _menuItem;

        private SaveSolutionSettings(AsyncPackage package, OleMenuCommandService commandService, Setting setting)
        {
            _setting = setting;
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            var menuCommandID = new CommandID(CommandSet, CommandId);
            _menuItem = new MenuCommand(this.Execute, menuCommandID)
            {
                Enabled = true
            };
            commandService.AddCommand(_menuItem);
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static SaveSolutionSettings Instance { get; private set; }

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
            Instance = new SaveSolutionSettings(package, commandService, setting);
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
            var solution = VisualStudioUtility.GetSolutionSettingsFileFullPath(false);
            if (!string.IsNullOrWhiteSpace(solution))
            {
                _setting.Serialize(solution);
            }
        }
    }
}