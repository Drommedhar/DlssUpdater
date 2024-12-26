using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using DLSSUpdater.Defines;
using DlssUpdater.GameLibrary;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons.AntiCheatChecker;
using DLSSUpdater.Helpers;
using Microsoft.Win32;

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
    public bool ShowAboutOnStartup { get; set; }
    public bool ShowNotifications { get; set; } = true;
    public WindowState WindowState { get; set; } = WindowState.Normal;
    public SortableObservableCollection<LibraryConfig> Libraries { get; set; } = [];

    public void Save()
    {
        RegistryHelper.WriteRegistryValue(Constants.RegistryPath, AntiCheat.RegistryName, AntiCheatSettings.ActiveAntiCheatChecks, RegistryHive.CurrentUser, RegistryView.Registry64);
        RegistryHelper.WriteRegistryValue(Constants.RegistryPath, "WindowState", WindowState, RegistryHive.CurrentUser, RegistryView.Registry64);
        RegistryHelper.WriteRegistryValue(Constants.RegistryPath, "ShowNotifications", ShowNotifications, RegistryHive.CurrentUser, RegistryView.Registry64);
        foreach (var lib in Libraries)
        {
            RegistryHelper.WriteRegistryValue(Constants.RegistryPath + @$"\Libraries\{lib.LibraryType}", "Enabled", lib.IsChecked, RegistryHive.CurrentUser, RegistryView.Registry64);
            if(lib.NeedsInstallPath)
            {
                RegistryHelper.WriteRegistryValue(Constants.RegistryPath + @$"\Libraries\{lib.LibraryType}", "InstallPath", lib.InstallPath, RegistryHive.CurrentUser, RegistryView.Registry64);
            }
        }
    }

    public void Load()
    {
        AntiCheatSettings.ActiveAntiCheatChecks = EnumHelper.GetAs<AntiCheatProvider>(RegistryHelper.ReadRegistryValue(Constants.RegistryPath, AntiCheat.RegistryName, RegistryHive.CurrentUser, RegistryView.Registry64) as string);
        WindowState = EnumHelper.GetAs<WindowState>(RegistryHelper.ReadRegistryValue(Constants.RegistryPath, "WindowState", RegistryHive.CurrentUser, RegistryView.Registry64) as string);
        ShowNotifications = bool.Parse((RegistryHelper.ReadRegistryValue(Constants.RegistryPath, "ShowNotifications", RegistryHive.CurrentUser, RegistryView.Registry64) as string) ?? "True");
        var libNames = RegistryHelper.ReadRegistrySubKeys(Settings.Constants.RegistryPath + @$"\Libraries", RegistryHive.CurrentUser, RegistryView.Registry64);
        foreach (var lib in libNames)
        {
            var enabled = bool.Parse((RegistryHelper.ReadRegistryValue(Constants.RegistryPath + $@"\Libraries\{lib}", "Enabled", RegistryHive.CurrentUser, RegistryView.Registry64) as string) ?? "True");
            var library = Libraries.FirstOrDefault(l => l.LibraryType.ToString().Equals(lib, StringComparison.OrdinalIgnoreCase));
            if(library is null)
            {
                continue;
            }

            library.IsChecked = enabled;
            if (library.NeedsInstallPath)
            {
                library.InstallPath = (RegistryHelper.ReadRegistryValue(Constants.RegistryPath + @$"\Libraries\{lib}", "InstallPath", RegistryHive.CurrentUser, RegistryView.Registry64) as string) ?? "";
            }
        }
    }

    public static class Constants
    {
        //public static string GamesFile { get; } = "games.json";
        public static string CacheFile { get; } = "cache.json";
        //public static string SettingsFile { get; } = "settings.json";
        public static TimeSpan CacheTime { get; } = TimeSpan.FromMinutes(30);
        public static int CoreCount { get; } = Environment.ProcessorCount;
        public static string RegistryPath { get; } = @"SOFTWARE\DlssUpdater";
    }

    public class Paths
    {
        [JsonIgnore] public string InstallPath { get; } = "DlssUpdater/installed";

        [JsonIgnore] public string DownloadPath { get; } = "DlssUpdater/download";

        //[JsonIgnore] public string SettingsPath { get; set; } = "DlssUpdater/settings";
    }

    public class AntiCheat
    {
        public static string RegistryName { get; } = @"AntiCheatChecks";
        public AntiCheatProvider ActiveAntiCheatChecks { get; set; } = AntiCheatProvider.All;
    }
}