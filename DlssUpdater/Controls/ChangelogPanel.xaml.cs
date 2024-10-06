using System;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using static DlssUpdater.Defines.DlssTypes;

namespace DLSSUpdater.Controls
{
    /// <summary>
    /// Interaction logic for ChangelogPanel.xaml
    /// </summary>
    public partial class ChangelogPanel : UserControl
    {
        public static readonly DependencyProperty HeaderTextProperty =
        DependencyProperty.Register("HeaderText", typeof(string), typeof(ChangelogPanel));

        public string HeaderText
        {
            get => (string)GetValue(HeaderTextProperty);
            set => SetValue(HeaderTextProperty, value);
        }

        public static readonly DependencyProperty ContentTextProperty =
        DependencyProperty.Register("ContentText", typeof(string), typeof(ChangelogPanel));

        public string ContentText
        {
            get => (string)GetValue(ContentTextProperty);
            set => SetValue(ContentTextProperty, value);
        }

        public static readonly DependencyProperty ExpandedProperty =
        DependencyProperty.Register("Expanded", typeof(Visibility), typeof(ChangelogPanel));

        private int indentSize = 30; // Indentation size for wrapped lines

        public Visibility Expanded
        {
            get => (Visibility)GetValue(ExpandedProperty);
            set => SetValue(ExpandedProperty, value);
        }

        public ChangelogPanel()
        {
            InitializeComponent();
        }

        // Function to split text and create individual TextBlocks for each line
        private void UpdateTextBlocks()
        {
            // Clear existing TextBlocks
            TextBlockContainer.Children.Clear();

            // Replace tab with 4 spaces for consistency
            string text = ContentText.Replace("\t", new string(' ', 4));

            // Create a FormattedText to help measure the width of the text
            double availableWidth = TextBlockContainer.ActualWidth;

            if (availableWidth <= 0) return; // No point in proceeding if the width isn't ready

            string[] words = text.Split(' ');
            string currentLine = "";
            bool isFirstLine = true;

            foreach (var word in words)
            {
                // Add the next word to the line (with a space if not the first word)
                string testLine = (currentLine.Length == 0 ? currentLine : currentLine + " ") + word;

                // Measure the test line's width
                var formattedTestLine = CreateFormattedText(testLine);

                // Check if the current line exceeds the available width
                if (formattedTestLine.Width > availableWidth)
                {
                    // The current line is too long, so we finalize the current line and move to the next
                    AddTextBlockToContainer(currentLine, isFirstLine);

                    // Get indent from last line
                    var lines = currentLine.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                    var spaces = lines[^1].TakeWhile(Char.IsWhiteSpace).Count();
                    // Now check if the line starts with a '-", so we add 2 more spaces
                    if (lines[^1].Trim().StartsWith("-"))
                    {
                        spaces += 3;
                    }

                    // Start a new line with indentation for wrapped lines
                    currentLine = new string(' ', spaces) + word;
                    isFirstLine = false;
                }
                else
                {
                    // If the line fits, continue adding to the current line
                    currentLine = testLine;
                }
            }

            // Add the final line (if any)
            if (!string.IsNullOrEmpty(currentLine))
            {
                AddTextBlockToContainer(currentLine, isFirstLine);
            }
        }

        // Create and measure a FormattedText for line width calculation
        private FormattedText CreateFormattedText(string text)
        {
            return new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), // Use the font you prefer
                16, // Font size
                Brushes.White,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
        }

        // Add a new TextBlock to the StackPanel container for each line
        private void AddTextBlockToContainer(string text, bool isFirstLine)
        {
            TextBlock textBlock = new TextBlock
            {
                Text = text,
                FontSize = 16,
                FontFamily = new FontFamily("Segoe UI"),
                TextWrapping = TextWrapping.NoWrap, // We handle wrapping manually
                Margin = new Thickness(0, 0, 0, 0) // Indent wrapped lines
            };

            TextBlockContainer.Children.Add(textBlock);
        }

        private void ctrlChangelog_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTextBlocks();
        }

        private void ctrlChangelog_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateTextBlocks();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (Expanded == Visibility.Visible)
            {
                Expanded = Visibility.Collapsed;
            }
            else
            {
                Expanded = Visibility.Visible;
            }
        }
    }
}
