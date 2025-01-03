﻿using System.Globalization;
using System.Windows.Controls;
using System.Windows.Media;

namespace DLSSUpdater.Controls;

/// <summary>
///     Interaction logic for AboutPanel.xaml
/// </summary>
public partial class AboutPanel : UserControl
{
    public static readonly DependencyProperty HeaderTextProperty =
        DependencyProperty.Register("HeaderText", typeof(string), typeof(AboutPanel));

    public static readonly DependencyProperty ContentTextProperty =
        DependencyProperty.Register("ContentText", typeof(string), typeof(AboutPanel));

    public static readonly DependencyProperty ExpandedProperty =
        DependencyProperty.Register("Expanded", typeof(Visibility), typeof(AboutPanel));

    private int indentSize = 30; // Indentation size for wrapped lines

    public AboutPanel()
    {
        InitializeComponent();
    }

    public string HeaderText
    {
        get => (string)GetValue(HeaderTextProperty);
        set => SetValue(HeaderTextProperty, value);
    }

    public string ContentText
    {
        get => (string)GetValue(ContentTextProperty);
        set => SetValue(ContentTextProperty, value);
    }

    public Visibility Expanded
    {
        get => (Visibility)GetValue(ExpandedProperty);
        set => SetValue(ExpandedProperty, value);
    }

    // Function to split text and create individual TextBlocks for each line
    private void UpdateTextBlocks()
    {
        // Clear existing TextBlocks
        TextBlockContainer.Children.Clear();

        // Replace tab with 4 spaces for consistency
        var text = ContentText.Replace("\t", new string(' ', 4));

        // Create a FormattedText to help measure the width of the text
        var availableWidth = TextBlockContainer.ActualWidth;

        if (availableWidth <= 0)
        {
            return; // No point in proceeding if the width isn't ready
        }

        var words = text.Split(' ');
        var currentLine = "";
        var isFirstLine = true;

        foreach (var word in words)
        {
            // Add the next word to the line (with a space if not the first word)
            var testLine = (currentLine.Length == 0 ? currentLine : currentLine + " ") + word;

            // Measure the test line's width
            var formattedTestLine = CreateFormattedText(testLine);

            // Check if the current line exceeds the available width
            if (formattedTestLine.Width > availableWidth)
            {
                // The current line is too long, so we finalize the current line and move to the next
                AddTextBlockToContainer(currentLine, isFirstLine);

                // Get indent from last line
                var lines = currentLine.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                var spaces = lines[^1].TakeWhile(char.IsWhiteSpace).Count();
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
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), // Use the font you prefer
            16, // Font size
            Brushes.White,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
    }

    // Add a new TextBlock to the StackPanel container for each line
    private void AddTextBlockToContainer(string text, bool isFirstLine)
    {
        var textBlock = new TextBlock
        {
            Text = text,
            FontSize = 16,
            FontFamily = new FontFamily("Segoe UI"),
            TextWrapping = TextWrapping.NoWrap, // We handle wrapping manually
            Margin = new Thickness(0, 0, 0, 0) // Indent wrapped lines
        };

        TextBlockContainer.Children.Add(textBlock);
    }

    private void ctrlAbout_Loaded(object sender, RoutedEventArgs e)
    {
        UpdateTextBlocks();
    }

    private void ctrlAbout_SizeChanged(object sender, SizeChangedEventArgs e)
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