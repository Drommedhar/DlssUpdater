using System.IO;
using System.Text.Json;
using DlssUpdater.GameLibrary;
using DlssUpdater.Helpers;
using DLSSUpdater.Defines;
using Microsoft.Win32;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DLSSUpdater.GameLibrary;

public class GOGLibrary : ILibrary
{
    private readonly NLog.Logger _logger;
    private readonly LibraryConfig _config;

    internal GOGLibrary(LibraryConfig config, NLog.Logger logger)
    {
        _config = config;
        _logger = logger;

        _config.NeedsInstallPath = false;
        _config.InstallPath = "None needed";
    }

    public LibraryType GetLibraryType()
    {
        return _config.LibraryType;
    }

    public async Task<List<GameInfo>> GatherGamesAsync()
    {
        return await getGames();
    }

    public void GetInstallationDirectory()
    {

    }

    private async Task<List<GameInfo>> getGames()
    {
        List<GameInfo> ret = [];

        // We are getting the gog games paths from the user registry (if it exists)
        using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        using var gogRegistryKey = hklm.OpenSubKey(@"SOFTWARE\GOG.com\Games");
        var subKeys = gogRegistryKey?.GetSubKeyNames();
        if (subKeys == null || subKeys.Length == 0)
        {
            return ret;
        }

        foreach (var subKey in subKeys)
        {
            var gameKey = hklm.OpenSubKey(Path.Combine(@"SOFTWARE\GOG.com\Games", subKey));
            if (gameKey == null)
            {
                continue;
            }

            var dependsOn = gameKey.GetValue("dependsOn") as string;
            if (!string.IsNullOrEmpty(dependsOn))
            {
                continue;
            }

            var name = gameKey.GetValue("gameName") as string;
            var path = gameKey.GetValue("path") as string;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(path))
            {
                continue;
            }

            var info = new GameInfo(name, path, GetLibraryType())
            {
                UniqueId = "gog_" + subKey
            };
            var gameImage = await getGameImage(subKey) ?? string.Empty;
            if (!string.IsNullOrEmpty(gameImage))
            {
                info.SetGameImageUri(gameImage);
            }

            await info.GatherInstalledVersions();
            if (info.HasInstalledDlls())
            {
                ret.Add(info);
            }
            else
            {
                _logger.Debug($"GOG: '{info.GameName}' does not have any DLSS dll and is being ignored.");
            }
        }
        return ret;
    }

    private async Task<string?> getGameImage(string appId)
    {
        // NOTE: For ease of use we just use the online image here, as we need an online connection anyway
        var url = $"https://api.gog.com/v2/games/{appId}";
        var json = await WebHelper.HttpGet(url);
        var yourObject = JsonDocument.Parse(json);

        try
        {
            var image = yourObject.RootElement
                          .GetProperty("_links");
            image = image.GetProperty("boxArtImage");
            image = image.GetProperty("href");
            return image.GetString();
        }
        catch (Exception ex)
        {
            _logger.Warn($"GOGLibrary getGameImage exception: {ex}");
            return "";
        }
    }
}