using System.Collections.ObjectModel;
using System.IO;
using NLog;

namespace DLSSUpdater.ViewModels.Pages;

public partial class ChangelogPageViewModel : ObservableObject
{
    private readonly Logger _logger;

    [ObservableProperty] private ObservableCollection<ChangelogEntry> _entries = [];

    public ChangelogPageViewModel(Logger logger)
    {
        _logger = logger;
    }

    public async void Init()
    {
        var lines = await File.ReadAllLinesAsync("changelog.md");

        // Now parse the lines into entries
        ChangelogEntry? currentEntry = null;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            if (line.StartsWith("#"))
            {
                if (currentEntry is not null)
                {
                    Entries.Add(currentEntry);
                }

                currentEntry = new ChangelogEntry();
                currentEntry.Header = line.Replace("#", "");
            }

            if (line.Trim().StartsWith("*") && currentEntry is not null)
            {
                if (!string.IsNullOrEmpty(currentEntry.Content))
                {
                    currentEntry.Content += "\n";
                }

                currentEntry.Content += line.Replace("*", "-");
            }
        }

        if (currentEntry is not null)
        {
            Entries.Add(currentEntry);
        }
    }

    public class ChangelogEntry
    {
        public string Header { get; set; } = "";
        public string Content { get; set; } = "";
    }
}