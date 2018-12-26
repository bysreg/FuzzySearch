namespace FuzzySearch
{
    using EnvDTE;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using static FuzzySearch.FuzzySearchWindowCommand;

    /// <summary>
    /// Interaction logic for FuzzySearchWindowControl.
    /// </summary>
    public partial class FuzzySearchWindowControl : UserControl
    {
        public FuzzySearchWindow parent;

        /// <summary>
        /// Initializes a new instance of the <see cref="FuzzySearchWindowControl"/> class.
        /// </summary>
        public FuzzySearchWindowControl(FuzzySearchWindow window)
        {
            this.InitializeComponent();

            parent = window;
            textBox.PreviewKeyDown += TextBox_PreviewKeyDown;
        }

        private void TextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            // for some reason escape is not detected in here
            // so added "tab" in here in order to close this tool window 
            if (e.Key == System.Windows.Input.Key.Escape || e.Key == System.Windows.Input.Key.Tab && e.KeyboardDevice.Modifiers == System.Windows.Input.ModifierKeys.None)
            {
                Hide();
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Down)
            {
                if (listBox.SelectedIndex < listBox.Items.Count - 1)
                    ++listBox.SelectedIndex;
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Up)
            {
                if (listBox.SelectedIndex > 0)
                    --listBox.SelectedIndex;
                e.Handled = true;
            }
            else if (e.Key == System.Windows.Input.Key.Enter)
            {
                OpenSelected();
                e.Handled = true;
            }
        }

        private void OpenSelected()
        {
            string full_path = ((WorkspaceFileInfo)listBox.Items[listBox.SelectedIndex]).FullPath;
            FuzzySearchWindowCommand.Instance.OpenFile(full_path);

            Hide();
        }

        private void Hide()
        {
            this.parent.Hide();
        }

        private void MyToolWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (this.IsVisible)
            {
                textBox.Focus();
            }
        }

        bool FuzzyMatch(string search, string str)
        {
            int search_i = 0, search_len = search.Length;
            int str_i = 0, full_path_len = str.Length;

            if (search_len > full_path_len)
                return false;

            while (search_i < search_len && str_i < full_path_len)
            {
                if (search[search_i] == Char.ToLower(str[str_i]))
                {
                    ++search_i;
                }
                ++str_i;
            }

            return search_i == search_len;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = textBox.Text.ToLowerInvariant();

            // todo: 
            // - do this in parallel. 
            // - cache the previous search result, if a character is added to the 
            //   search query, then we only need to do fuzzy match against the
            //   the previous search result
            List<WorkspaceFileInfo> results = new List<WorkspaceFileInfo>();
            foreach (WorkspaceFileInfo file_info in FuzzySearchWindowCommand.Instance.WorkspaceFiles)
            {
                // first, do fuzzy match agains the filename
                // if it returns false, then we do fuzzy match against the full path
                if (FuzzyMatch(search, file_info.filename) || FuzzyMatch(search, file_info.full_path))
                {
                    results.Add(file_info);
                }
            }

            // todo: need to rank the results

            // cap the result to the top 100
            if (results.Count > 100)
                results.RemoveRange(100, results.Count - 100);

            listBox.ItemsSource = results;
        }
    }
}