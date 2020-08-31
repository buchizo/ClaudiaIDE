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
    internal sealed class PauseSlideshow
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int PauseCommandId = 0x0110;

        public const int ResumeCommandId = 0x0120;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("f0ffaf7c-8feb-40d2-b898-1acfe50e1d6b");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        private readonly Setting _setting;
        private bool _paused;

        /// <summary>
        /// Initializes a new instance of the <see cref="PauseSlideshow"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private PauseSlideshow(AsyncPackage package, OleMenuCommandService commandService, Setting setting)
        {
            _setting = setting;
            _setting.OnChanged.AddEventHandler(ReloadSettings);
            ReloadSettings(null, EventArgs.Empty);
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var pauseCommandID = new CommandID(CommandSet, PauseCommandId);
            var resumeCommandID = new CommandID(CommandSet, ResumeCommandId);
            var pauseMenuItem = new OleMenuCommand(this.Execute, pauseCommandID);
            var resumeMenuItem = new OleMenuCommand(this.Execute, resumeCommandID);
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

        private void UpdatePauseVisibility(OleMenuCommand cmd)
        {
            cmd.Enabled = _setting.ImageBackgroundType == ImageBackgroundType.Slideshow && !_paused;
            cmd.Visible = !_paused;
        }

        private void UpdateResumeVisibility(OleMenuCommand cmd)
        {
            cmd.Enabled = _paused;
            cmd.Visible = _paused;
        }


        private SlideShowImageProvider GetSlideshow()
        {
            return (SlideShowImageProvider) ProvidersHolder.Instance.Providers.First(p =>
                p.ProviderType == ImageBackgroundType.Slideshow);
        }


        private void ReloadSettings(object sender, EventArgs args)
        {
            GetSlideshow().Pause = false;
            _paused = false;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static PauseSlideshow Instance { get; private set; }

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
            // Switch to the main thread - the call to AddCommand in PauseSlideshow's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService =
                await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new PauseSlideshow(package, commandService, setting);
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
            var slideshow = GetSlideshow();
            if (slideshow.Pause)
            {
                slideshow.Pause = false;
                _paused = false;
            }
            else
            {
                slideshow.Pause = true;
                _paused = true;
            }
        }
    }
}