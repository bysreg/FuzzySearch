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
            var menuItem = new MenuCommand(this.ExecuteAsync, menuCommandID);
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
        private async void ExecuteAsync(object sender, EventArgs e)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

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
                await LoadAllFilesAsync(workspace_path);
            }

            //// HACK: hardcoded the x position of the tool window for testing
#if DEBUG
            IVsWindowFrame frame = window.Frame as IVsWindowFrame;
            VSSETFRAMEPOS[] temp = new VSSETFRAMEPOS[100]; // not really sure what array size for this should be
            Guid temp2;
            int px, py, pcx, pcy;
            frame.GetFramePos(temp, out temp2, out px, out py, out pcx, out pcy);
            frame.SetFramePos(VSSETFRAMEPOS.SFP_fMove, ref temp2, 400, py, pcx, pcy);
#endif

            (window as FuzzySearchWindow).Show();
        }

        private List<WorkspaceFileInfo> workspace_files = new List<WorkspaceFileInfo>();

        public struct WorkspaceFileInfo
        {
            public readonly string full_path;
            public readonly string filename;

            public string FullPath { get { return full_path; } }
            public string FileName { get { return filename; } }

            public WorkspaceFileInfo(string full_path, string filename)
            {
                this.full_path = full_path;
                this.filename = filename;
            }
        }

        public List<WorkspaceFileInfo> WorkspaceFiles
        {
            get
            {
                return workspace_files;
            }
        }

        private async Task LoadAllFilesAsync(string root_path)
        {
            workspace_files.Clear();

            if (!File.GetAttributes(root_path).HasFlag(FileAttributes.Directory))
            {
                // if the root_path is not a directory, then get the directory of the file
                root_path = Path.GetDirectoryName(root_path);
            }

            List<string> subfolders = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(Path.Combine(root_path, ".fuzzysearchsettings")))
                {
                    string line;
                    while ((line = await sr.ReadLineAsync()) != null)
                    {
                        subfolders.Add(line);
                    }
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException)
            {
                // do nothing
            }

            // if subfolders is still zero count, then default to everything in this root_path
            if (subfolders.Count == 0)
                subfolders.Add(root_path);

            foreach (string subfolder in subfolders)
            {
                try
                {
                    string[] files = Directory.GetFiles(Path.Combine(root_path, subfolder), "*", SearchOption.AllDirectories);

                    foreach (string full_path in files)
                    {
                        string filename = Path.GetFileName(full_path);
                        workspace_files.Add(new WorkspaceFileInfo(full_path, filename));
                    }
                }
                catch (DirectoryNotFoundException)
                {
                    // do nothing
                }
            }
        }

        public void OpenFile(string full_path)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            dte.ItemOperations.OpenFile(full_path, EnvDTE.Constants.vsViewKindPrimary);
        }
    }
}
