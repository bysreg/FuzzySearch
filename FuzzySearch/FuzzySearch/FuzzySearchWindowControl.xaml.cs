namespace FuzzySearch
{
    using EnvDTE;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;
    using static FuzzySearch.FuzzySearchWindowCommand;
    using static FuzzySearch.Algorithm;

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

        struct SearchResult
        {

            public string FullPath { get; set; }
            public string FileName { get; set; }

            public int score;

            public SearchResult(string full_path, string filename, int score)
            {
                FullPath = full_path;
                FileName = filename;
                this.score = score;
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string search = textBox.Text.ToLowerInvariant();

            // todo: 
            // - do this in parallel. 
            // - cache the previous search result, if a character is added to the 
            //   search query, then we only need to do fuzzy match against the
            //   the previous search result
            List<SearchResult> results = new List<SearchResult>();
            foreach (WorkspaceFileInfo file_info in FuzzySearchWindowCommand.Instance.WorkspaceFiles)
            {
                // first, do fuzzy match agains the filename instead of the fullpath
                Tuple<bool, int> result = FuzzyMatch(search, file_info.filename);
                if (!result.Item1)
                {
                    // if there's no match agains the filename, then we do fuzzy match against the full path
                    result = FuzzyMatch(search, file_info.full_path);
                }
                if (result.Item1)
                    results.Add(new SearchResult(file_info.full_path, file_info.filename, result.Item2));
            }

            results.Sort(delegate (SearchResult a, SearchResult b)
            {
                return b.score - a.score;
            });

            // cap the result to the top 100
            if (results.Count > 100)
                results.RemoveRange(100, results.Count - 100);

            listBox.ItemsSource = results;
        }
    }
}