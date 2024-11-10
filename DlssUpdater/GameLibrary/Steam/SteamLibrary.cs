using System.IO;
using DLSSUpdater.Defines;
using DlssUpdater.Helpers;
using DLSSUpdater.Helpers;
using NLog;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.GameLibrary.Steam;

public class SteamLibrary : ILibrary
{
    private readonly LibraryConfig _config;

    private readonly Logger _logger;

    internal SteamLibrary(LibraryConfig config, Logger logger)
    {
        _config = config;
        _logger = logger;

        if (string.IsNullOrEmpty(config.InstallPath))
        {
            GetInstallationDirectory();
        }
    }

    public event EventHandler<Tuple<int, int, LibraryType>>? LoadingProgress;

    public LibraryType GetLibraryType()
    {
        return _config.LibraryType;
    }

    public async Task<List<GameInfo>> GatherGamesAsync()
    {
        if (string.IsNullOrEmpty(_config.InstallPath))
        {
            return [];
        }

        // We now know steam is installed, so we can carry on by parsing the 'libraryfolder.vdf'
        var folders = await getLibraryFolders();
        return await getGames(folders);
    }

    public void GetInstallationDirectory()
    {
        // We are getting the steam installation path from the user registry (if it exists)
        var installPath = RegistryHelper.GetRegistryValue(@"SOFTWARE\Valve\Steam", "InstallPath") as string;

        _logger.Debug($"Steam install directory: {installPath ?? "N/A"}");

        if (!string.IsNullOrEmpty(installPath))
        {
            _config.InstallPath = installPath;
        }
    }

    private async Task<List<LibraryFolder>> getLibraryFolders()
    {
        List<LibraryFolder> ret = [];
        var vdfPath = Path.Combine(_config.InstallPath!, "steamapps", "libraryfolders.vdf");
        if (!File.Exists(vdfPath))
        {
            _logger.Error($"Steam: Could not find 'libraryfolder.vdf' in '{vdfPath}'");
            return ret;
        }

        VdfParser parser = new(vdfPath);
        var success = await parser.Load();
        if (!success)
        {
            _logger.Error($"Steam: Could not parse 'libraryfolder.vdf' in '{vdfPath}'");
            return ret;
        }

        // We can now parse the stuff
        var paths = parser.GetValuesForKey<string>("\"path\"");
        var apps = parser.GetValuesForKey<List<string>>("\"apps\"");

        if (paths.Count != apps.Count)
        {
            _logger.Error("Steam: 'libraryfolder.vdf' contained count mismatch for paths and apps segments");
            return ret;
        }

        for (var i = 0; i < paths.Count; i++)
        {
            var path = paths[i];
            List<string> appIds = [];
            foreach (var a in apps[i])
            {
                appIds.Add(a);
            }

            LibraryFolder folder = new(Path.Combine(path, "steamapps"))
            {
                Apps = appIds
            };
            ret.Add(folder);
        }

        _logger.Debug($"Steam: Library paths found:\n{string.Join("\n", ret)}");

        return ret;
    }

    private async Task<List<GameInfo>> getGames(List<LibraryFolder> folder)
    {
        List<Task> tasks = [];
        List<GameInfo> ret = [];

        var throttler = new SemaphoreSlim(Settings.Constants.CoreCount);

        var amount = folder.Sum(folderItem => folderItem.Apps.Count);
        var current = 0;

        foreach (var folderItem in folder)
        foreach (var app in folderItem.Apps)
        {
            var task = Task.Run(async () =>
            {
                // do an async wait until we can schedule again
                await throttler.WaitAsync();

                current += 1;
                LoadingProgress?.Invoke(this, new Tuple<int, int, LibraryType>(current, amount, GetLibraryType()));

                var appPath = Path.Combine(folderItem.Path, $"appmanifest_{app}.acf");
                if (!File.Exists(appPath))
                {
                    _logger.Warn($"Steam: {appPath} not found.");
                }
                else
                {
                    var info = await getGameFromManifest(appPath, app);
                    if (info is not null)
                    {
                        ret.Add(info);
                    }
                }

                throttler.Release();
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);
        ret = ret.GroupBy(g => g.GamePath)
            .Select(g => g.First())
            .ToList();
        return ret;
    }

    private async Task<GameInfo?> getGameFromManifest(string path, string appId)
    {
        VdfParser parser = new(path);
        if (!await parser.Load())
        {
            return null;
        }

        var gameName = parser.GetValuesForKey<string>("name");
        var gamePath = parser.GetValuesForKey<string>("installdir");
        if (gameName.Count != 1 || gamePath.Count != 1)
        {
            _logger.Warn($"Steam: getGameFromManifest returned {gameName.Count} and {gamePath.Count}");
            return null;
        }

        var finalGamePath = Path.Combine(Path.GetDirectoryName(path)!, "common", gamePath[0]);
        if (!Directory.Exists(finalGamePath))
        {
            _logger.Warn($"Steam: getGameFromManifest could not find file {finalGamePath}");
            return null;
        }

        var info = new GameInfo(gameName[0], finalGamePath, LibraryType.Steam)
        {
            UniqueId = "steam_" + appId
        };
        var imageUri = getGameImage(appId);
        if (imageUri != null)
        {
            info.SetGameImageUri(imageUri);
        }

        await info.GatherInstalledVersions();
        if (info.HasInstalledDlls())
        {
            _logger.Debug($"Steam: '{info.GamePath}' has DLSS dll and is being added.");
            return info;
        }

        _logger.Debug($"Steam: '{info.GamePath}' does not have any DLSS dll and is being ignored.");
        return null;
    }

    private string? getGameImage(string appId)
    {
        var cachedImage =
            Path.Combine(_config.InstallPath!, "appcache", "librarycache", $"{appId}_library_600x900.jpg");
        if (File.Exists(cachedImage))
        {
            return new string(cachedImage);
        }

        var url = $"https://steamcdn-a.akamaihd.net/steam/apps/{appId}/library_600x900_2x.jpg";
        var valid = WebHelper.UrlIsValid(url);

        if (valid)
        {
            return url;
        }

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

        public override string ToString()
        {
            return $"'{Path}': -> {string.Join("\t", Apps)}";
        }
    }
}