namespace FuzzySearch
{
    using System;
    using System.Runtime.InteropServices;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Interop;

    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("ec38d12f-b1ed-4c45-8c44-616edd5fa15c")]
    public class FuzzySearchWindow : ToolWindowPane
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FuzzySearchWindow"/> class.
        /// </summary>
        public FuzzySearchWindow() : base(null)
        {
            this.Caption = "Fuzzy Search Window";

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on
            // the object returned by the Content property.
            this.Content = new FuzzySearchWindowControl(this);
        }

        public void Show()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure((this.Frame as IVsWindowFrame).Show());
        }

        public void Hide()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure((this.Frame as IVsWindowFrame).Hide());
        }
    }
}
