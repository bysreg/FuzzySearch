using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Workspace;
using Task = System.Threading.Tasks.Task;

namespace FuzzySearch
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class FuzzySearchWindowCommand
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("be23469e-a3b5-4322-8f43-fe9f4b08e95c");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        DTE dte;

        /// <summary>
        /// Initializes a new instance of the <see cref="FuzzySearchWindowCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private FuzzySearchWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);

            dte = FuzzySearchWindowPackage.GetGlobalService(typeof(DTE)) as DTE;
        }

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static FuzzySearchWindowCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in FuzzySearchWindowCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new FuzzySearchWindowCommand(package, commandService);

        }

        string workspace_path = "";

        /// <summary>
        /// Shows the tool window when the menu item is clicked.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        private void Execute(object sender, EventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.package.FindToolWindow(typeof(FuzzySearchWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException("Cannot create tool window");
            }

            if (dte.Solution.FullName != "" && workspace_path != dte.Solution.FullName)
            {
                workspace_path = dte.Solution.FullName;
                LoadAllFiles(workspace_path);
            }

            (window as FuzzySearchWindow).Show();
        }

        private List<string> workspace_files = new List<string>();

        public List<string> WorkspaceFiles
        {
            get
            {
                return workspace_files;
            }

        }

        private void LoadAllFiles(string root_path)
        {
            workspace_files.Clear();

            //only iterate on these sub-folders
            string[] subfolders = { "src", "test" };

            foreach (string subfolder in subfolders)
            {
                try
                {
                    string[] files = Directory.GetFiles(root_path + "\\" + subfolder, "*", SearchOption.AllDirectories);
                    workspace_files.AddRange(files);
                }
                catch (DirectoryNotFoundException e)
                {
                    // do nothing
                }
            }
        }
    }
}
