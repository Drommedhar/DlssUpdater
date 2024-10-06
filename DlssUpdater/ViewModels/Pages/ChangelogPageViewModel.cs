using DlssUpdater;
using DlssUpdater.Singletons.AntiCheatChecker;
using DlssUpdater.Singletons;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DlssUpdater.Helpers;
using DlssUpdater.GameLibrary;
using DLSSUpdater.Defines;
using Microsoft.Win32;
using DLSSUpdater.Defines.UI.Pages;
using System.Collections.ObjectModel;

namespace DLSSUpdater.ViewModels.Pages
{
    public partial class ChangelogPageViewModel : ObservableObject
    {
        public class ChangelogEntry
        {
            public string Header { get; set; } = "";
            public string Content { get; set; } = "";
        }

        [ObservableProperty]
        private ObservableCollection<ChangelogEntry> _entries = [];
        
        private readonly NLog.Logger _logger;

        public ChangelogPageViewModel(NLog.Logger logger)
        {
            _logger = logger;
        }

        public async void Init()
        {
            var lines = await System.IO.File.ReadAllLinesAsync("changelog.md");

            // Now parse the lines into entries
            ChangelogEntry? currentEntry = null;
            foreach (var line in lines)
            {
                if(string.IsNullOrWhiteSpace(line)) continue;

                if(line.StartsWith("#"))
                {
                    if(currentEntry is not null)
                    {
                        Entries.Add(currentEntry);
                    }
                    currentEntry = new();
                    currentEntry.Header = line.Replace("#", "");
                }

                if(line.Trim().StartsWith("*") && currentEntry is not null)
                {
                    if(!string.IsNullOrEmpty(currentEntry.Content))
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
    }
}
