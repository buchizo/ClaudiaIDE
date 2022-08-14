using System;
using System.ComponentModel.Design;
using Task = System.Threading.Tasks.Task;
using ClaudiaIDE.Interfaces;
using Microsoft.VisualStudio.Shell;

namespace ClaudiaIDE.MenuCommands
{
    /// <summary>
    ///     Command handler
    /// </summary>
    internal sealed class PauseSlideshow
    {
        /// <summary>
        ///     Command ID.
        /// </summary>
        public const int PauseCommandId = 0x0110;

        public const int ResumeCommandId = 0x0120;

        /// <summary>
        ///     Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid(GuidList.MenuSetId);

        private IPausable pausable;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PauseSlideshow" /> class.
        ///     Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private PauseSlideshow(AsyncPackage package, OleMenuCommandService commandService)
        {
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var pauseCommandID = new CommandID(CommandSet, PauseCommandId);
            var resumeCommandID = new CommandID(CommandSet, ResumeCommandId);
            var pauseMenuItem = new OleMenuCommand(Execute, pauseCommandID);
            var resumeMenuItem = new OleMenuCommand(Execute, resumeCommandID);
            UpdatePauseVisibility(pauseMenuItem);
            UpdateResumeVisibility(resumeMenuItem);
            pauseMenuItem.BeforeQueryStatus += (sender, args) =>
            {
                if (!(sender is OleMenuCommand cmd)) return;
                UpdatePauseVisibility(cmd);
            };
            resumeMenuItem.BeforeQueryStatus += (sender, args) =>
            {
                if (!(sender is OleMenuCommand cmd)) return;
                UpdateResumeVisibility(cmd);
            };
            commandService.AddCommand(pauseMenuItem);
            commandService.AddCommand(resumeMenuItem);
        }

        /// <summary>
        ///     Gets the instance of the command.
        /// </summary>
        public static PauseSlideshow Instance { get; private set; }

        private void UpdatePauseVisibility(OleMenuCommand cmd)
        {
            var activePaused = GetActivePausable();
            cmd.Visible = activePaused == null || !activePaused.IsPaused;
            cmd.Enabled = activePaused != null && !activePaused.IsPaused;
        }

        private void UpdateResumeVisibility(OleMenuCommand cmd)
        {
            var activePaused = GetActivePausable();
            cmd.Visible = activePaused != null && activePaused.IsPaused;
            cmd.Enabled = activePaused != null;
        }


        private IPausable GetActivePausable()
        {
            if (pausable != null)
                if (pausable is ImageProvider imageProvider && imageProvider.IsActive)
                    return pausable;

            var activeProvider = ProvidersHolder.Instance.ActiveProvider;
            if (activeProvider is IPausable provider)
            {
                pausable = provider;
                return provider;
            }

            return null;
        }

        /// <summary>
        ///     Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in PauseSlideshow's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService =
                await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new PauseSlideshow(package, commandService);
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
            var pausable = GetActivePausable();
            if (pausable != null)
            {
                if (pausable.IsPaused)
                    pausable.Resume();
                else
                    pausable.Pause();
            }
        }
    }
}