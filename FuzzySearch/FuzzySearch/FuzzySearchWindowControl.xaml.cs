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

        // First element of the tuple would be true if search "matches" the str string
        // Second element of the tuple would be the score of the "matchness"
        Tuple<bool, int> FuzzyMatch(string search, string str)
        {
            int search_i = 0, search_len = search.Length;
            int str_i = 0, str_len = str.Length;

            bool prev_matched = false;
            int first_str_match = -1;

            int score = 0;

            if (search_len > str_len)
                return Tuple.Create(false, score);

            while (search_i < search_len && str_i < str_len)
            {
                if (search[search_i] == Char.ToLower(str[str_i]))
                {
                    ++search_i;

                    // consecutive matches worth more
                    if (prev_matched)
                        score += 5;

                    prev_matched = true;

                    if (first_str_match == -1)
                        first_str_match = str_i;
                }
                else
                {
                    prev_matched = false;
                }
                ++str_i;
            }

            // unmatched letter scores negatively (every unmatch letter is -1)
            score -= (str_len - search_i);

            // unmatched leading letter affect negatively (capped to -9)
            score -= Math.Min(first_str_match * 3, 9);

            return Tuple.Create(search_i == search_len, score);
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