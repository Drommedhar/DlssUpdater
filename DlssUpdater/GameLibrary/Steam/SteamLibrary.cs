using System.IO;
using DlssUpdater.Helpers;
using Microsoft.Win32;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.GameLibrary.Steam;

public class SteamLibrary : ILibrary
{
    private string? _installPath;

    internal SteamLibrary()
    {
        getInstallationDirectory();
    }

    public async Task<List<GameInfo>> GatherGamesAsync()
    {
        if (_installPath is null) return [];

        // We now know steam is installed, so we can carry on by parsing the 'libraryfolder.vdf'
        var folders = await getLibraryFolders();
        return await getGames(folders);
    }

    private async Task<List<LibraryFolder>> getLibraryFolders()
    {
        List<LibraryFolder> ret = [];
        var vdfPath = Path.Combine(_installPath!, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdfPath)) return ret;

        VdfParser parser = new(vdfPath);
        var success = await parser.Load();
        if (!success) return ret;

        // We can now parse the stuff
        var paths = parser.GetValuesForKey<string>("\"path\"");
        var apps = parser.GetValuesForKey<List<string>>("\"apps\"");

        if (paths.Count != apps.Count) return ret;

        for (var i = 0; i < paths.Count; i++)
        {
            var path = paths[i];
            var appIds = apps[i];
            LibraryFolder folder = new(Path.Combine(path, "steamapps"))
            {
                Apps = appIds
            };
            ret.Add(folder);
        }

        return ret;
    }

    private void getInstallationDirectory()
    {
        // We are getting the steam installation path from the user registry (if it exists)
        using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        using var steamRegistryKey = hklm.OpenSubKey(@"SOFTWARE\Valve\Steam");
        var installPath = steamRegistryKey?.GetValue("InstallPath") as string;
        if (!string.IsNullOrEmpty(installPath)) _installPath = installPath;
    }

    private async Task<List<GameInfo>> getGames(List<LibraryFolder> folder)
    {
        List<GameInfo> ret = [];

        foreach (var folderItem in folder)
        foreach (var app in folderItem.Apps)
        {
            var appPath = Path.Combine(folderItem.Path, $"appmanifest_{app}.acf");
            if (!File.Exists(appPath)) continue;

            var info = await getGameFromManifest(appPath, app);
            if (info is not null) ret.Add(info);
        }

        return ret;
    }

    private async Task<GameInfo?> getGameFromManifest(string path, string appId)
    {
        VdfParser parser = new(path);
        if (!await parser.Load()) return null;

        var gameName = parser.GetValuesForKey<string>("name");
        var gamePath = parser.GetValuesForKey<string>("installdir");
        if (gameName.Count != 1 || gamePath.Count != 1) return null;

        var finalGamePath = Path.Combine(Path.GetDirectoryName(path)!, "common", gamePath[0]);
        if (!Directory.Exists(finalGamePath)) return null;
        var info = new GameInfo(gameName[0], finalGamePath, LibraryType.Steam);
        var imageUri = getGameImage(appId);
        if (imageUri != null)
        {
            info.SetGameImageUri(imageUri);
            info.TextVisible = Visibility.Hidden;
        }

        await info.GatherInstalledVersions();
        if (info.HasInstalledDlls()) return info;

        return null;
    }

    private string? getGameImage(string appId)
    {
        var cachedImage = Path.Combine(_installPath!, "appcache", "librarycache", $"{appId}_library_600x900.jpg");
        if (File.Exists(cachedImage)) return new string(cachedImage);

        var url = $"https://steamcdn-a.akamaihd.net/steam/apps/{appId}/library_600x900_2x.jpg";
        var valid = WebHelper.UrlIsValid(url);

        if (valid) return url;
        return null;
    }

    public class LibraryFolder
    {
        public LibraryFolder(string path)
        {
            Path = path;
        }

        public string Path { get; init; }
        public List<string> Apps { get; internal set; } = [];
    }
}