using System;
using System.ComponentModel.Design;
using System.IO;
using ClaudiaIDE.Settings;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace ClaudiaIDE.MenuCommands
{
    /// <summary>
    ///     Command handler
    /// </summary>
    internal sealed class ResetSolutionSettings
    {
        /// <summary>
        ///     Command ID.
        /// </summary>
        public const int CommandId = 0x0150;

        /// <summary>
        ///     Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid(GuidList.MenuSetId);

        private readonly MenuCommand _menuItem;

        private readonly Setting _setting;

        private ResetSolutionSettings(AsyncPackage package, OleMenuCommandService commandService, Setting setting)
        {
            _setting = setting;
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));
            var menuCommandID = new CommandID(CommandSet, CommandId);
            _menuItem = new MenuCommand(Execute, menuCommandID)
            {
                Enabled = true
            };
            commandService.AddCommand(_menuItem);
        }

        /// <summary>
        ///     Gets the instance of the command.
        /// </summary>
        public static ResetSolutionSettings Instance { get; private set; }

        /// <summary>
        ///     Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package, Setting setting)
        {
            // Switch to the main thread - the call to AddCommand in NextImage's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);
            var commandService =
                await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ResetSolutionSettings(package, commandService, setting);
        }

        /// <summary>
        /// reset/remove .claudiaconfig file from solution directory.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var solution = VisualStudioUtility.GetSolutionSettingsFileFullPath(false);
            if (string.IsNullOrWhiteSpace(solution) || !File.Exists(solution)) return;
            try
            {
                File.Delete(solution);
                _setting.SolutionConfigFilePath = null;
                Setting.Instance.OnApplyChanged();
            }
            catch { }
        }
    }
}