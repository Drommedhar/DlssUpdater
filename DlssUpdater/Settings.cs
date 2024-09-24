using DlssUpdater.Defines;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons.AntiCheatChecker;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DlssUpdater;

public class Settings
{
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

    public void Save()
    {
        DirectoryHelper.EnsureDirectoryExists(Directories.SettingsPath);
        var settingsPath = Path.Combine(Directories.SettingsPath, Constants.SettingsFile);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions());
        File.WriteAllText(settingsPath, json);
    }

    public void Load()
    {
        var settingsPath = Path.Combine(Directories.SettingsPath, Constants.SettingsFile);

        if (File.Exists(settingsPath))
        {
            var jsonData = File.ReadAllText(settingsPath);
            var data = JsonSerializer.Deserialize<Settings>(jsonData, new JsonSerializerOptions())!;
            copyData(data);
        }
    }

    private void copyData(Settings other)
    {
        Directories = other.Directories;
        AntiCheatSettings = other.AntiCheatSettings;
    }
}