using System.IO;
using DLSSUpdater.Defines;
using DlssUpdater.Helpers;
using DLSSUpdater.Helpers;
using NLog;
using GameInfo = DlssUpdater.Defines.GameInfo;

namespace DlssUpdater.GameLibrary;

public class XboxLibrary : ILibrary
{
    private readonly LibraryConfig _config;
    private readonly Logger _logger;

    internal XboxLibrary(LibraryConfig config, Logger logger)
    {
        _config = config;
        _logger = logger;

        _config.NeedsInstallPath = false;
        _config.InstallPath = "None needed";
    }

    public event EventHandler<Tuple<int, int, LibraryType>> LoadingProgress;

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
        List<Task> tasks = [];
        var throttler = new SemaphoreSlim(1);
        List<GameInfo> ret = [];

        var amount = 0;
        var current = 0;

        List<ApplicationInfo> windowsStoreApps = WindowsAppHelper.GetInstalledAppsFromWindowsStore();

        var drive = DriveInfo.GetDrives();
        foreach (var driveItem in drive)
        {
            var task = Task.Run(async () =>
            {
                try
                {
                    // do an async wait until we can schedule again
                    await throttler.WaitAsync();

                    if (driveItem.DriveType != DriveType.Fixed)
                    {
                        return;
                    }

                    var rootFile = Path.Combine(driveItem.RootDirectory.FullName, ".GamingRoot");
                    if (!File.Exists(rootFile))
                    {
                        return;
                    }

                    var fileData = await File.ReadAllTextAsync(rootFile);
                    fileData = fileData.Replace("\0", string.Empty);
                    fileData = fileData.Replace("\u0001", string.Empty);
                    if (!fileData.StartsWith("RGBX"))
                    {
                        return;
                    }

                    fileData = fileData.Replace("RGBX", string.Empty);

                    var gamePath = Path.Combine(driveItem.RootDirectory.FullName, fileData);
                    if (!Path.Exists(gamePath))
                    {
                        return;
                    }
                    
                    var gameDirs = Directory.GetDirectories(gamePath);
                    amount += gameDirs.Length;
                    foreach (var gameDir in gameDirs)
                    {
                        current += 1;
                        LoadingProgress?.Invoke(this,
                            new Tuple<int, int, LibraryType>(current, amount, GetLibraryType()));

                        string? identityName = null;
                        string? displayName = null;
                        var xmlDoc = WindowsAppHelper.InitXDocumentFromManifest(Path.Combine(gameDir, "Content", "appxmanifest.xml"), out var uap);
                        if (xmlDoc != null)
                        {
                            identityName = WindowsAppHelper.GetIdentityNameFromXml(xmlDoc, uap!);
                        }                                             
                        else
                        {
                            continue;
                        }

                        if (identityName != null)
                        {
                            // Query the SplashScreen element and get the Image attribute
                            var splashScreenImage = xmlDoc
                                .Descendants(uap! + "SplashScreen")
                                .FirstOrDefault()?.Attribute("Image")?.Value;
                            
                            ApplicationInfo? windowsStoreApp = windowsStoreApps.Find(f => f.identityName.Equals(identityName));

                            displayName = windowsStoreApp?.displayName ?? WindowsAppHelper.GetDisplayNameFromXml(xmlDoc, uap!);

                            // Extract the namespace (if any is necessary)
                            var ns = xmlDoc.Root!.Name.Namespace;

                            // Query all Application elements and get their Id attributes
                            var id = xmlDoc
                                .Descendants(ns + "Application") // Include the namespace for the Application element
                                .Select(app => app.Attribute("Id")?.Value).FirstOrDefault();

                            var imagePathFinal = "";
                            if (splashScreenImage != null)
                            {
                                imagePathFinal = Path.Combine(gameDir, "Content", splashScreenImage);
                            }

                            if (!string.IsNullOrEmpty(displayName))
                            {
                                var info = new GameInfo(displayName, gameDir, GetLibraryType())
                                {
                                    UniqueId = "xbox_" + id
                                };
                                if (!string.IsNullOrEmpty(imagePathFinal))
                                {
                                    info.SetGameImageUri(imagePathFinal);
                                }

                                await info.GatherInstalledVersions();
                                if (info.HasInstalledDlls())
                                {
                                    _logger.Debug($"Xbox: '{info.GamePath}' has DLSS dll and is being added.");
                                    ret.Add(info);
                                }
                                else
                                {
                                    _logger.Debug(
                                        $"Xbox: '{info.GamePath}' does not have any DLSS dll and is being ignored.");
                                }
                            }
                        }
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
        if (File.Exists(cachedImage))
        {
            return new string(cachedImage);
        }

        var url = $"https://ubistatic3-a.akamaihd.net/orbit/uplay_launcher_3_0/assets/{thumbImage}";
        var valid = WebHelper.UrlIsValid(url);

        if (valid)
        {
            return url;
        }

        return null;
    }
}