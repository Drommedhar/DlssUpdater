using System.IO;
using DlssUpdater.GameLibrary.Steam;
using DlssUpdater.Helpers;
using DLSSUpdater.Defines;
using DLSSUpdater.Helpers;
using Microsoft.Win32;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.GameLibrary;

public class UbisoftConnectLibrary : ILibrary
{
    private readonly LibraryConfig _config;
    private readonly NLog.Logger _logger;

    internal UbisoftConnectLibrary(LibraryConfig config, NLog.Logger logger)
    {
        _config = config;
        _logger = logger;

        if(string.IsNullOrEmpty(config.InstallPath) )
        {
            GetInstallationDirectory();
        }
    }

    public LibraryType GetLibraryType()
    {
        return _config.LibraryType;
    }

    public async Task<List<GameInfo>> GatherGamesAsync()
    {
        if (string.IsNullOrEmpty(_config.InstallPath)) return [];

        return await getGames();
    }

    public void GetInstallationDirectory()
    {
        // We are getting the ubisoft installation path from the user registry (if it exists)
        var installPath = RegistryHelper.GetRegistryValue(@"SOFTWARE\Ubisoft\Launcher", "InstallDir") as string;

        _logger.Debug($"Ubisoft Connect install directory: {installPath ?? "N/A"}");

        if (!string.IsNullOrEmpty(installPath)) _config.InstallPath = installPath;
    }

    private async Task<List<GameInfo>> getGames()
    {
        List<GameInfo> ret = [];
        if(string.IsNullOrEmpty(_config.InstallPath))
        {
            return ret;
        }

        var configPath = Path.Combine(_config.InstallPath, "cache", "configuration", "configurations");
        if(!File.Exists(configPath))
        {
            _logger.Warn($"Ubisoft connect: Could not find configurations file at {configPath}");
            return ret;
        }

        var data = await File.ReadAllTextAsync(configPath);
        var entries = data.Split("root:", StringSplitOptions.TrimEntries);
        List<Task> tasks = [];
        var throttler = new SemaphoreSlim(initialCount: 10);
        foreach (var entry in entries)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    // do an async wait until we can schedule again
                    await throttler.WaitAsync();

                    var result = entry
                     .Split("\n")
                     .Select(x => x.Split(':'))
                     .SafeToDictionary(x => x[0].Trim(), x => x.Length >= 2 ? x[1].Trim() : "");

                    if (!result.TryGetValue("name", out var name))
                    {
                        return;
                    }
                    if (!result.TryGetValue("register", out var registerKey))
                    {
                        return;
                    }
                    if (!result.TryGetValue("thumb_image", out var thumbImage))
                    {
                        return;
                    }
                    if (!result.TryGetValue("app_id", out var appId))
                    {
                        return;
                    }

                    var gamePath = RegistryHelper.GetRegistryValue(registerKey.Replace("HKEY_LOCAL_MACHINE\\", "")
                                                 .Replace("\\InstallDir", ""), "InstallDir") as string;
                    if (string.IsNullOrEmpty(gamePath))
                    {
                        _logger.Warn($"Ubisoft connect: Could not find regkey for {registerKey}");
                        return;
                    }

                    var info = new GameInfo(name, gamePath, LibraryType.Ubisoft)
                    {
                        UniqueId = "ubi_" + appId
                    };
                    await info.GatherInstalledVersions();
                    if (info.HasInstalledDlls())
                    {
                        var imageUri = getGameImage(thumbImage);
                        if (imageUri != null)
                        {
                            info.SetGameImageUri(imageUri);
                        }
                        ret.Add(info);
                    }
                    else
                    {
                        _logger.Debug($"Ubisoft connect: '{info.GameName}' does not have any DLSS dll and is being ignored.");
                    }
                }
                finally
                {
                    throttler.Release();
                }
            });
            tasks.Add(task);
        }

        await Task.WhenAll(tasks);

        return ret;
    }

    private string? getGameImage(string thumbImage)
    {
        var cachedImage = Path.Combine(_config.InstallPath!, "cache", "assets", thumbImage);
        if (File.Exists(cachedImage)) return new string(cachedImage);

        var url = $"https://ubistatic3-a.akamaihd.net/orbit/uplay_launcher_3_0/assets/{thumbImage}";
        var valid = WebHelper.UrlIsValid(url);

        if (valid) return url;
        return null;
    }
}