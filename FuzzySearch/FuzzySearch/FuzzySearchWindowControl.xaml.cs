namespace FuzzySearch
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows;
    using System.Windows.Controls;

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
            if (e.Key == System.Windows.Input.Key.Escape || e.Key == System.Windows.Input.Key.Tab)
            {
                Hide();
                e.Handled = true;
            }
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
            int str_i = 0, str_len = str.Length;

            if (search_len > str_len)
                return false;

            while (search_i < search_len && str_i < str_len)
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

            List<string> results = new List<string>();
            foreach (string str in FuzzySearchWindowCommand.Instance.WorkspaceFiles)
            {
                if (FuzzyMatch(search, str))
                {
                    results.Add(str);
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