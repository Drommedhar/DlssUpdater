using System.IO;
using DlssUpdater.GameLibrary.Steam;
using DlssUpdater.Helpers;
using Microsoft.Win32;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.GameLibrary;

public class UbisoftConnectLibrary : ILibrary
{
    private string? _installPath;

    internal UbisoftConnectLibrary()
    {
        getInstallationDirectory();
    }

    public async Task<List<GameInfo>> GatherGamesAsync()
    {
        if (_installPath is null) return [];

        return await getGames();
    }

    private void getInstallationDirectory()
    {
        // We are getting the steam installation path from the user registry (if it exists)
        using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
        using var ubiRegKey = hklm.OpenSubKey(@"SOFTWARE\Ubisoft\Launcher");
        var installPath = ubiRegKey?.GetValue("InstallDir") as string;
        if (!string.IsNullOrEmpty(installPath)) _installPath = installPath;
    }

    private async Task<List<GameInfo>> getGames()
    {
        List<GameInfo> ret = [];
        if(string.IsNullOrEmpty(_installPath))
        {
            return ret;
        }

        var configPath = Path.Combine(_installPath, "cache", "configuration", "configurations");
        if(!File.Exists(configPath))
        {
            return ret;
        }

        var data = await File.ReadAllTextAsync(configPath);
        var entries = data.Split("root:", StringSplitOptions.TrimEntries);
        foreach (var entry in entries)
        {
            var result = entry
             .Split("\n")
             .Select(x => x.Split(':'))
             .SafeToDictionary(x => x[0].Trim(), x => x.Length >= 2 ? x[1].Trim() : "");

            if (!result.TryGetValue("name", out var name))
            {
                continue;
            }
            if (!result.TryGetValue("register", out var registerKey))
            {
                continue;
            }
            if (!result.TryGetValue("thumb_image", out var thumbImage))
            {
                continue;
            }

            using var hklm = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
            using var gameRegKey = hklm.OpenSubKey(registerKey.Replace("HKEY_LOCAL_MACHINE\\","")
                                   .Replace("\\InstallDir", ""));
            var gamePath = gameRegKey?.GetValue("InstallDir") as string;
            if (string.IsNullOrEmpty(gamePath))
            {
                continue;
            }

            var info = new GameInfo(name, gamePath, LibraryType.Ubisoft);
            await info.GatherInstalledVersions();
            if (info.HasInstalledDlls())
            {
                var imageUri = getGameImage(thumbImage);
                if (imageUri != null)
                {
                    info.SetGameImageUri(imageUri);
                    info.TextVisible = Visibility.Hidden;
                }
                ret.Add(info);
            }
            
        }

        return ret;
    }

    private string? getGameImage(string thumbImage)
    {
        var cachedImage = Path.Combine(_installPath!, "cache", "assets", thumbImage);
        if (File.Exists(cachedImage)) return new string(cachedImage);

        var url = $"https://ubistatic3-a.akamaihd.net/orbit/uplay_launcher_3_0/assets/{thumbImage}";
        var valid = WebHelper.UrlIsValid(url);

        if (valid) return url;
        return null;
    }
}