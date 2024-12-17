using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DLSSUpdater.Defines;
using DlssUpdater.GameLibrary;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons.AntiCheatChecker;

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

    public Settings()
    {
        foreach (LibraryType libType in Enum.GetValues(typeof(LibraryType)))
        {
            if (libType == LibraryType.Manual)
            {
                continue;
            }

            Libraries.Add(new LibraryConfig(libType, ILibrary.GetName(libType)));
        }
    }

    public Paths Directories { get; set; } = new();
    public AntiCheat AntiCheatSettings { get; set; } = new();
    public bool ShowChangelogOnStartup { get; set; }
    public bool ShowNotifications { get; set; } = true;
    public WindowState WindowState { get; set; } = WindowState.Normal;
    public SortableObservableCollection<LibraryConfig> Libraries { get; set; } = [];

    public void Save()
    {
        DirectoryHelper.EnsureDirectoryExists(Path.Combine(Environment.GetFolderPath(
    Environment.SpecialFolder.ApplicationData), Directories.SettingsPath));
        var settingsPath = Path.Combine(Environment.GetFolderPath(
    Environment.SpecialFolder.ApplicationData), Directories.SettingsPath, Constants.SettingsFile);
        var json = JsonSerializer.Serialize(this, _jsonOptions);
        File.WriteAllText(settingsPath, json);
    }

    public void Load()
    {
        var settingsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Directories.SettingsPath, Constants.SettingsFile);

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
        ShowNotifications = other.ShowNotifications;
        foreach (var lib in other.Libraries)
        {
            var index = Libraries.IndexOf(l => l.LibraryType == lib.LibraryType);
            if (index == -1)
            {
                continue;
            }

            Libraries[index] = lib;
        }
    }

    public static class Constants
    {
        public static string GamesFile { get; } = "games.json";
        public static string CacheFile { get; } = "cache.json";
        public static string SettingsFile { get; } = "settings.json";
        public static TimeSpan CacheTime { get; } = TimeSpan.FromMinutes(30);
        public static int CoreCount { get; } = Environment.ProcessorCount;
    }

    public class Paths
    {
        [JsonIgnore] public string InstallPath { get; set; } = "DlssUpdater/installed";

        [JsonIgnore] public string DownloadPath { get; set; } = "DlssUpdater/download";

        [JsonIgnore] public string SettingsPath { get; set; } = "DlssUpdater/settings";
    }

    public class AntiCheat
    {
        public AntiCheatProvider ActiveAntiCheatChecks { get; set; } = AntiCheatProvider.All;
    }
}