using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using DlssUpdater.Defines;
using DlssUpdater.Helpers;
using Microsoft.Security.Extensions;
using NLog;
using static DlssUpdater.Defines.DlssTypes;
using static DlssUpdater.Helpers.HttpClientDownloadWithProgress;
using static DlssUpdater.Settings;

namespace DlssUpdater.Singletons;

public class DllUpdater
{
    public enum UpdateResult
    {
        NothingDone,
        Success,
        Failure
    }

    public static string DefaultVersion = "Restore default";

    [JsonIgnore] private readonly Logger _logger;

    [JsonIgnore] private readonly Settings _settings;

    public DllUpdater()
    {
    }

    public DllUpdater(Settings settings, Logger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public DateTime LastUpdate { get; set; } = DateTime.MinValue;

    public Dictionary<DllType, ObservableCollection<OnlinePackage>> OnlinePackages { get; set; } = [];

    [JsonIgnore] public Dictionary<DllType, ObservableCollection<InstalledPackage>> InstalledPackages { get; } = [];

    public event EventHandler? DlssFilesChanged;

    public event EventHandler<string>? OnInfo;
    public event ProgressChangedHandler? ProgressChanged;

    public bool IsNewerVersionAvailable()
    {
        foreach (DllType dllType in Enum.GetValues(typeof(DllType)))
        {
            if (IsNewerVersionAvailable(dllType))
            {
                return true;
            }
        }

        return false;
    }

    public bool IsNewerVersionAvailable(DllType type)
    {
        if (InstalledPackages.TryGetValue(type, out var highestInstalled) &&
            OnlinePackages.TryGetValue(type, out var highestOnline) && highestInstalled.FirstOrDefault() != null
            && highestOnline.FirstOrDefault() != null)
        {
            return new Version(highestInstalled.FirstOrDefault()!.Version) <
                   new Version(highestOnline.FirstOrDefault()!.Version);
        }

        return false;
    }

    public bool IsNewerVersionAvailable(DllType type, InstalledPackage installed)
    {
        if (InstalledPackages.TryGetValue(type, out var highestInstalled)
            && highestInstalled.FirstOrDefault(p => p.Version != DefaultVersion) != null)
        {
            return new Version(installed.Version) <
                   new Version(highestInstalled.FirstOrDefault(p => p.Version != DefaultVersion)!.Version);
        }

        return true;
    }

    public async Task UpdateDlssVersionsAsync()
    {
        Load();
        var nextCheck = LastUpdate.Add(Constants.CacheTime);
        if (DateTime.UtcNow > nextCheck)
        {
            _logger.Debug("Updating online DLSS libs.");
            OnlinePackages.Clear();
            foreach (DllType dllType in Enum.GetValues(typeof(DllType)))
            {
                await updateDlssVersionAsync(dllType);
            }

            DlssFilesChanged?.Invoke(this, EventArgs.Empty);
            LastUpdate = DateTime.UtcNow;
        }

        Save();
    }

    public async Task<Tuple<bool, string?>> DownloadDll(DllType dllType, string version)
    {
        try
        {
            var package = OnlinePackages[dllType].FirstOrDefault(p => p.Version == version);
            if (package is null)
            {
                return new Tuple<bool, string?>(false, "Online package not found.");
            }

            var url = GetUrl(package.DllType);
            if (url == null)
            {
                return new Tuple<bool, string?>(false, "Dll url not found.");
            }

            url += $"id={package.DownloadId}";
            KeyValuePair<string, string>[] formData = [new("id", package.DownloadId)];
            var data = await WebHelper.HttpPost(url, formData);
            // Data will contain servers now (if nothing went wrong)
            var searchString = "<button type=\"submit\" name=\"server_id\" value=\"";
            var startIndex = data.IndexOf(searchString) + searchString.Length;
            var endIndex = data.IndexOf('>', startIndex) - 1;
            var serverId = data.Substring(startIndex, endIndex - startIndex).Trim();

            KeyValuePair<string, string>[] formDataDownload =
            [
                new("id", package.DownloadId),
                new("server_id", serverId)
            ];
            var outputPath = Path.Combine(_settings.Directories.DownloadPath, GetName(package.DllType));
            DirectoryHelper.EnsureDirectoryExists(outputPath);
            outputPath = Path.Combine(outputPath, package.Version + ".zip");
            using (var client = new HttpClientDownloadWithProgress(url, outputPath))
            {
                client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
                {
                    ProgressChanged?.Invoke(totalFileSize, totalBytesDownloaded, progressPercentage);
                };

                await client.StartDownload(formDataDownload);
            }

            // Something failed
            if (!File.Exists(outputPath))
            {
                _logger.Warn($"DllUpdater: Could not download file to {outputPath}");
                return new Tuple<bool, string?>(false, $"Could not download file to {outputPath}.");
            }

            using (var sha256 = SHA256.Create())
            {
                using var stream = File.OpenRead(outputPath);
                var onlineHashArray = sha256.ComputeHash(stream);
                var onlineHash = BitConverter.ToString(onlineHashArray).Replace("-", "").ToLowerInvariant();
                if (package.SHA256.ToLower() != onlineHash)
                {
                    _logger.Error($"DllUpdater: SHA256 check failed for {outputPath}");
                    return new Tuple<bool, string?>(false, $"SHA256 check failed for {outputPath}.");
                }
            }

            var dllTargetPath = Path.Combine(_settings.Directories.InstallPath, GetName(package.DllType),
                package.Version.Replace(' ', '_'));
            ZipFile.ExtractToDirectory(outputPath, dllTargetPath, true);
            File.Delete(outputPath);

            var dllPath = Path.Combine(dllTargetPath, GetDllName(dllType));
            using var fileStream = new FileStream(dllPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileSignatureInfo = FileSignatureInfo.GetFromFileStream(fileStream);
            if (fileSignatureInfo.State != SignatureState.SignedAndTrusted)
            {
                File.Delete(dllPath);
                _logger.Warn($"Could not verify signature of '{dllPath}'");
                return new Tuple<bool, string?>(false, $"Could not verify signature of '{dllPath}'.");
            }

            var fileInfo = FileVersionInfo.GetVersionInfo(dllPath);
            var versionString = fileInfo.FileVersion!.Replace(',', '.');
            var versionParsed = Version.Parse(versionString);
            if (versionParsed.Revision == -1)
            {
                versionParsed = new Version(versionParsed.Major, versionParsed.Minor, versionParsed.Build, 0);
            }

            if (!InstalledPackages.TryGetValue(dllType, out var value))
            {
                value = new ObservableCollection<InstalledPackage>();
                InstalledPackages.Add(dllType, value);
            }

            var folder = Path.GetFileName(dllTargetPath)!;
            var installedPackage = new InstalledPackage
            {
                Path = Path.Combine(dllTargetPath, GetDllName(dllType)),
                Version = versionParsed.ToString(),
                VersionDetailed = folder
            };
            value.Add(installedPackage);
            var installed = value.ToList();
            value.Clear();
            installed.Sort((first, second) => second.VersionDetailed.CompareTo(first.VersionDetailed));
            foreach (var ver in installed)
            {
                value.Add(ver);
            }

            DlssFilesChanged?.Invoke(this, EventArgs.Empty);
            return new Tuple<bool, string?>(true, string.Empty);
        }
        catch (HttpRequestException ex)
        {
            return new Tuple<bool, string?>(false, $"Download failed: {ex}");
        }
    }

    public void RemoveInstalledDll(DllType dllType, string version)
    {
        if (InstalledPackages.TryGetValue(dllType, out var installedPackages))
        {
            var package = installedPackages.FirstOrDefault(p => p.VersionDetailed == version);
            if (package is null)
            {
                return;
            }

            File.Delete(package.Path);
            Directory.Delete(Path.GetDirectoryName(package.Path)!);
            installedPackages.Remove(package);
            DlssFilesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool HasDefaultDll(DllType dllType, GameInfo gameInfo)
    {
        if (!gameInfo.DefaultDlls.TryGetValue(dllType, out var defaultDll))
        {
            var defaultPath = GetGameDefaultDllPath(gameInfo);

            // Check if the target already exists
            return File.Exists(Path.Combine(defaultPath, GetDllName(dllType)));
        }

        return true;
    }

    public UpdateResult UpdateGameDlls(GameInfo gameInfo, bool saveAsDefault)
    {
        var result = UpdateResult.NothingDone;
        foreach (var (dll, info) in gameInfo.InstalledDlls)
        {
            if (string.IsNullOrEmpty(info.Version))
            {
                continue;
            }

            if (info.Version == DefaultVersion)
            {
                RestoreDefaultDll(dll, gameInfo);
                continue;
            }

            var package = InstalledPackages[dll].FirstOrDefault(p => p.VersionDetailed == info.Version);
            if (package is null)
            {
                continue;
            }

            // Now detect if we need to save this as a default
            if (saveAsDefault)
            {
                // We need to save this, so we do that
                var defaultPath = GetGameDefaultDllPath(gameInfo);
                DirectoryHelper.EnsureDirectoryExists(defaultPath);
                try
                {
                    File.Copy(info.Path, Path.Combine(defaultPath, GetDllName(dll)), true);
                    gameInfo.DefaultDlls.Add(dll, true);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Default copy failed: {ex}");
                }

                if (!gameInfo.DefaultDlls.TryAdd(dll, true))
                {
                    gameInfo.DefaultDlls[dll] = true;
                }
            }

            try
            {
                File.Copy(package.Path, info.Path, true);
                result = UpdateResult.Success;
            }
            catch (Exception ex)
            {
                _logger.Error($"DllUpdater: Exception on UpdateGameDlls: {ex}");
                result = UpdateResult.Failure;
                return result;
            }
        }

        return result;
    }

    public string GetGameDefaultDllPath(GameInfo gameInfo)
    {
        return Path.Combine(_settings.Directories.InstallPath, "Games", gameInfo.UniqueId);
    }

    public void RestoreDefaultDll(DllType dllType, GameInfo gameInfo)
    {
        var defaultPath = GetGameDefaultDllPath(gameInfo);
        if (gameInfo.InstalledDlls.TryGetValue(dllType, out var package))
        {
            if (package is null || string.IsNullOrEmpty(package.Path))
            {
                return;
            }

            var defaultDll = Path.Combine(defaultPath, GetDllName(dllType));
            if (!File.Exists(defaultDll))
            {
                return;
            }

            File.Copy(defaultDll, package.Path, true);
            _logger.Debug($"Restored default for '{gameInfo.GameName}' -> {GetDllName(dllType)}");
        }
    }

    public void Load()
    {
        var cachePath = Path.Combine(_settings.Directories.SettingsPath, Constants.CacheFile);

        if (File.Exists(cachePath))
        {
            var jsonData = File.ReadAllText(cachePath);
            var data = JsonSerializer.Deserialize<DllUpdater>(jsonData, new JsonSerializerOptions())!;
            LastUpdate = data.LastUpdate;
            OnlinePackages = data.OnlinePackages;
            DlssFilesChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public void Save()
    {
        DirectoryHelper.EnsureDirectoryExists(_settings.Directories.SettingsPath);
        var cachePath = Path.Combine(_settings.Directories.SettingsPath, Constants.CacheFile);
        var json = JsonSerializer.Serialize(this, new JsonSerializerOptions());
        File.WriteAllText(cachePath, json);
    }

    private async Task updateDlssVersionAsync(DllType dllType)
    {
        var onlinePackages = await checkOnlineVersionAsync(dllType);
        OnlinePackages.Add(dllType, onlinePackages);
    }

    private async Task<ObservableCollection<OnlinePackage>> checkOnlineVersionAsync(DllType dllType)
    {
        OnInfo?.Invoke(this, $"Checking available version for '{GetDllName(dllType)}'");
        var data = await WebHelper.HttpGet(GetUrl(dllType)!);

        // Gather list of all DLSS files
        var lines = data.Split("\n");
        var parseFiles = false;
        OnlinePackage? curPackage = null;
        ObservableCollection<OnlinePackage> onlinePackages = new();
        var dllName = GetDllName(dllType).Replace(".dll", "") + "_";
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];

            if (line.Contains("<ul class=\"files\">"))
            {
                parseFiles = true;
                continue;
            }

            if (!parseFiles)
            {
                continue;
            }

            // Parse filename
            if (line.Contains("class=\"filename\""))
            {
                curPackage = new OnlinePackage();
                //<div class="filename" title="File Name">nvngx_dlss_3.7.10.zip</div>
                var startIndex = line.IndexOf('>') + 1;
                var endIndex = line.LastIndexOf("<");
                if (startIndex == -1 && endIndex == -1)
                {
                    continue;
                }

                curPackage.Version = line.Substring(startIndex, endIndex - startIndex).Replace(dllName, "")
                    .Replace(".zip", "");
            }

            // Parse SHA256
            if (line.Contains("class=\"hash-name\">SHA256:"))
            {
                line = lines[i + 1];
                // Next line will contain the hash value
                //<div class="hash-value">83ECD0801FB010F9249B5F82C05AF4DF7532F71143336BF9BC7064376A619FBD</div>
                var startToken = "hash-value\">";
                var startIndex = line.LastIndexOf(startToken) + 1;
                var endIndex = line.LastIndexOf("</div>");
                if (startIndex == -1 && endIndex == -1)
                {
                    continue;
                }

                curPackage!.SHA256 = line.Substring(startIndex + startToken.Length - 1,
                    endIndex - startIndex - startToken.Length + 1);
            }

            // Parse download id
            if (line.Contains("<input type=\"hidden\" name=\"id\""))
            {
                //<input type="hidden" name="id" value="2638" />
                var startIndex = line.IndexOf("value=") + 7;
                var endIndex = line.LastIndexOf('"');
                if (startIndex == -1 && endIndex == -1)
                {
                    continue;
                }

                curPackage!.DownloadId = line.Substring(startIndex, endIndex - startIndex);
                curPackage!.DllType = dllType;
                if (string.IsNullOrEmpty(curPackage?.SHA256))
                {
                    _logger.Error($"Could not determine SHA256 for {GetDllName(dllType)} -> {curPackage!.Version}");
                }
                else
                {
                    onlinePackages.Add(curPackage);
                }

                curPackage = null;
            }
        }

        return onlinePackages;
    }

    public void GatherInstalledVersions()
    {
        foreach (DllType dllType in Enum.GetValues(typeof(DllType)))
        {
            List<InstalledPackage> installedPackages = new();
            if (!InstalledPackages.TryGetValue(dllType, out var value))
            {
                value = new ObservableCollection<InstalledPackage>();
                InstalledPackages.Add(dllType, value);
            }

            var path = getInstallPath(dllType);
            DirectoryHelper.EnsureDirectoryExists(path);
            var allFiles = Directory.GetFiles(path, GetDllName(dllType), SearchOption.AllDirectories);
            foreach (var file in allFiles)
            {
                var folder = Path.GetFileName(Path.GetDirectoryName(file))!;
                var fileInfo = FileVersionInfo.GetVersionInfo(file);
                var versionString = fileInfo.FileVersion!.Replace(',', '.');
                var package = new InstalledPackage
                {
                    Path = file,
                    Version = versionString,
                    VersionDetailed = folder
                };
                installedPackages.Add(package);
            }

            installedPackages.Sort((first, second) => second.VersionDetailed.CompareTo(first.VersionDetailed));
            foreach (var package in installedPackages)
            {
                value.Add(package);
            }
        }

        DlssFilesChanged?.Invoke(this, EventArgs.Empty);
    }

    private string getInstallPath(DllType dllType)
    {
        return Path.Combine(_settings.Directories.InstallPath, GetName(dllType));
    }

    internal bool IsInstalled(DllType dllType, string versionText)
    {
        if (InstalledPackages.TryGetValue(dllType, out var installedPackages))
        {
            return installedPackages.Any(p => p.VersionDetailed == versionText);
        }

        return false;
    }
}