using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;
using DlssUpdater;
using DlssUpdater.GameLibrary;
using DlssUpdater.Helpers;
using DLSSUpdater.Defines;
using DLSSUpdater.Helpers;
using Microsoft.Win32;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DLSSUpdater.GameLibrary.EpicGames;

public class EpicGamesLibrary : ILibrary
{
    private readonly NLog.Logger _logger;
    private readonly LibraryConfig _config;

    internal EpicGamesLibrary(LibraryConfig config, NLog.Logger logger)
    {
        _config = config;
        _logger = logger;

        if (string.IsNullOrEmpty(config.InstallPath))
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
        // We are getting the steam installation path from the user registry (if it exists)
        var installPath = RegistryHelper.GetRegistryValue(@"SOFTWARE\Epic Games\EpicGamesLauncher", "AppDataPath") as string;

        _logger.Debug($"Epic Games install directory: {installPath ?? "N/A"}");

        if (!string.IsNullOrEmpty(installPath)) _config.InstallPath = installPath;
    }

    private async Task<List<GameInfo>> getGames()
    {
        List<GameInfo> ret = [];
        if (string.IsNullOrEmpty(_config.InstallPath))
        {
            return ret;
        }

        var path = Path.Combine(_config.InstallPath, "Manifests");
        if (!Directory.Exists(path))
        {
            _logger.Warn("EpicGames Could not find Manifests folder");
            return ret;
        }

        var cacheFile = Path.Combine(_config.InstallPath, "Catalog", "catcache.bin");
        if (!File.Exists(cacheFile))
        {
            _logger.Warn("EpicGames Could not find catcache.bin");
            return ret;
        }

        // Start by loading the cache file
        List<CatalogCache> cachedGames = [];
        using (var cacheFileStream = File.OpenRead(cacheFile))
        {
            using var memoryStream = new MemoryStream((int)cacheFileStream.Length);
            await cacheFileStream.CopyToAsync(memoryStream);
            var cacheBase64 = Encoding.UTF8.GetString(memoryStream.ToArray());
            var cacheJson = Convert.FromBase64String(cacheBase64);
            try
            {
                cachedGames = JsonSerializer.Deserialize<List<CatalogCache>>(cacheJson)!;
            }
            catch (JsonException ex)
            {
                _logger.Error($"EpicGames could not parse catcache.bin: {ex}");
            }
        }

        List<Task> tasks = [];
        var throttler = new SemaphoreSlim(initialCount: Settings.Constants.CoreCount);
        var files = Directory.GetFiles(path);
        foreach (var file in files)
        {
            var task = Task.Run(async () =>
            {
                // do an async wait until we can schedule again
                await throttler.WaitAsync();

                var data = await File.ReadAllBytesAsync(file);
                var yourObject = JsonDocument.Parse(data);

                try
                {
                    var isApp = yourObject.RootElement.GetProperty("bIsApplication").GetBoolean();
                    if (!isApp)
                    {
                        return;
                    }

                    var displayName = yourObject.RootElement.GetProperty("DisplayName").GetString();
                    var location = yourObject.RootElement.GetProperty("InstallLocation").GetString()?.Replace('/', '\\');
                    var catalogId = yourObject.RootElement.GetProperty("CatalogItemId").GetString();

                    var cachedData = cachedGames.FirstOrDefault(g => g.Id == catalogId);
                    if (cachedData is null || string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(location))
                    {
                        return;
                    }

                    var imageObj = cachedData.KeyImages.FirstOrDefault(i => i.Type == "DieselGameBoxTall");

                    var info = new GameInfo(displayName, location, GetLibraryType())
                    {
                        UniqueId = "epic_" + catalogId!
                    };
                    if (imageObj is not null && !string.IsNullOrEmpty(imageObj.Url))
                    {
                        info.SetGameImageUri(imageObj.Url);
                    }

                    await info.GatherInstalledVersions();
                    if (info.HasInstalledDlls())
                    {
                        ret.Add(info);
                    }
                    else
                    {
                        _logger.Debug($"EpicGames: '{info.GameName}' does not have any DLSS dll and is being ignored.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn($"EpicGames Parsing manifest failed with:  {ex}");
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
            _logger.Warn($"EpicGamesLibrary getGameImage exception: {ex}");
            return "";
        }
    }
}