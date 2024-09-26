using DlssUpdater.Defines;
using DlssUpdater.GameLibrary;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons.AntiCheatChecker;
using DLSSUpdater.Defines;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime;
using System.Text.Json;
using System.Text.Json.Serialization;
using static DlssUpdater.Defines.DlssTypes;

namespace DlssUpdater;

public class Settings
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        Converters =
        {
            new LibraryConvert()
        }
    };

    public static class Constants
    {
        public static string GamesFile { get; } = "games.json";
        public static string CacheFile { get; } = "cache.json";
        public static string SettingsFile { get; } = "settings.json";
        public static TimeSpan CacheTime { get; } = TimeSpan.FromMinutes(30);
    }

    public class Paths
    {
        public string InstallPath { get; set; } = "installed";
        public string DownloadPath { get; set; } = "download";
        [JsonIgnore]
        public string SettingsPath { get; set; } = "settings";
    }

    public class AntiCheat
    {
        public bool DisclaimerAccepted { get; set; } = false;
        public AntiCheatProvider ActiveAntiCheatChecks { get; set; } = AntiCheatProvider.None;
    }

    public Paths Directories { get; set; } = new();
    public AntiCheat AntiCheatSettings { get; set; } = new();
    public bool ShowChangelogOnStartup { get; set; } = false;
    public WindowState WindowState { get; set; } = WindowState.Normal;
    public SortableObservableCollection<LibraryConfig> Libraries { get; set; } = [];

    public Settings()
    {
        foreach (LibraryType libType in Enum.GetValues(typeof(LibraryType)))
        {
            if(libType == LibraryType.Manual)
            {
                continue;
            }

            Libraries.Add(new(libType, ILibrary.GetName(libType)));
        }
    }

    public void Save()
    {
        DirectoryHelper.EnsureDirectoryExists(Directories.SettingsPath);
        var settingsPath = Path.Combine(Directories.SettingsPath, Constants.SettingsFile);
        var json = JsonSerializer.Serialize(this, _jsonOptions);
        File.WriteAllText(settingsPath, json);
    }

    public void Load()
    {
        var settingsPath = Path.Combine(Directories.SettingsPath, Constants.SettingsFile);

        if (File.Exists(settingsPath))
        {
            var jsonData = File.ReadAllText(settingsPath);
            var data = JsonSerializer.Deserialize<Settings>(jsonData, _jsonOptions)!;
            copyData(data);
        }
    }

    private void copyData(Settings other)
    {
        Directories = other.Directories;
        AntiCheatSettings = other.AntiCheatSettings;
        ShowChangelogOnStartup = other.ShowChangelogOnStartup;
        WindowState = other.WindowState;
        if(other.Libraries.Count != 0)
        {
            Libraries = other.Libraries;
        }
    }
}