using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using DlssUpdater.Defines;
using DlssUpdater.Helpers;
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

    [JsonIgnore]
    private readonly Settings _settings;
    [JsonIgnore]
    private readonly NLog.Logger _logger;

    public DateTime LastUpdate { get; set; } = DateTime.MinValue;

    public DllUpdater()
    {
    }

    public DllUpdater(Settings settings, NLog.Logger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public Dictionary<DllType, ObservableCollection<OnlinePackage>> OnlinePackages { get; set; } = [];
    [JsonIgnore]
    public Dictionary<DllType, ObservableCollection<InstalledPackage>> InstalledPackages { get; } = [];

    public event EventHandler<string>? OnInfo;
    public event ProgressChangedHandler? ProgressChanged;

    public bool IsNewerVersionAvailable()
    {
        // TODO: Implement
        return true;
    }

    public async Task UpdateDlssVersionsAsync()
    {
        Load();
        var nextCheck = LastUpdate.Add(Constants.CacheTime);
        if(DateTime.UtcNow > nextCheck)
        {
            _logger.Debug($"Updating online DLSS libs.");
            OnlinePackages.Clear();
            foreach (DllType dllType in Enum.GetValues(typeof(DllType)))
            {
                await updateDlssVersionAsync(dllType);
            }

            LastUpdate = DateTime.UtcNow;
        }
        Save();
    }

    public async Task<bool> DownloadDll(DllType dllType, string version)
    {
        try
        {
            var package = OnlinePackages[dllType].FirstOrDefault(p => p.Version == version);
            if (package is null) return false;

            var url = GetUrl(package.DllType);
            if (url == null) return false;

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
                return false;
            }

            var dllTargetPath = Path.Combine(_settings.Directories.InstallPath, GetName(package.DllType),
                package.Version.Replace(' ', '_'));
            ZipFile.ExtractToDirectory(outputPath, dllTargetPath, true);
            File.Delete(outputPath);

            var fileInfo = FileVersionInfo.GetVersionInfo(Path.Combine(dllTargetPath, GetDllName(dllType)));
            var versionString = fileInfo.FileVersion!.Replace(',', '.');
            var versionParsed = Version.Parse(versionString);
            if (versionParsed.Revision == -1)
                versionParsed = new Version(versionParsed.Major, versionParsed.Minor, versionParsed.Build, 0);

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
            return true;
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }

    public void RemoveInstalledDll(DllType dllType, string version)
    {
        if (InstalledPackages.TryGetValue(dllType, out var installedPackages))
        {
            var package = installedPackages.FirstOrDefault(p => p.VersionDetailed == version);
            if (package is null) return;

            File.Delete(package.Path);
            Directory.Delete(Path.GetDirectoryName(package.Path)!);
            installedPackages.Remove(package);
        }
    }

    public UpdateResult UpdateGameDlls(GameInfo gameInfo)
    {
        var result = UpdateResult.NothingDone;
        foreach (var (dll, info) in gameInfo.InstalledDlls)
        {
            if (string.IsNullOrEmpty(info.Version)) continue;

            var package = InstalledPackages[dll].FirstOrDefault(p => p.VersionDetailed == info.Version);
            if (package is null) continue;

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

    public void Load()
    {
        var cachePath = Path.Combine(_settings.Directories.SettingsPath, Settings.Constants.CacheFile);

        if (File.Exists(cachePath))
        {
            var jsonData = File.ReadAllText(cachePath);
            var data = JsonSerializer.Deserialize<DllUpdater>(jsonData, new JsonSerializerOptions())!;
            LastUpdate = data.LastUpdate;
            OnlinePackages = data.OnlinePackages;
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

            if (!parseFiles) continue;

            // Parse filename
            if (line.Contains("class=\"filename\""))
            {
                curPackage = new OnlinePackage();
                //<div class="filename" title="File Name">nvngx_dlss_3.7.10.zip</div>
                var startIndex = line.IndexOf('>') + 1;
                var endIndex = line.LastIndexOf("<");
                if (startIndex == -1 && endIndex == -1)
                    continue;

                curPackage.Version = line.Substring(startIndex, endIndex - startIndex).Replace(dllName, "")
                    .Replace(".zip", "");
            }

            // Parse MD5
            if (line.Contains("class=\"hash-name\">MD5:"))
            {
                line = lines[i + 1];
                // Next line will contain the hash value
                //<div class="hash-value">078F738799876F99778DFC51012F65F3</div>
                var startIndex = line.IndexOf('>') + 1;
                var endIndex = line.LastIndexOf("<");
                if (startIndex == -1 && endIndex == -1)
                    continue;

                curPackage!.MD5 = line.Substring(startIndex, endIndex - startIndex);
            }

            // Parse download id
            if (line.Contains("<input type=\"hidden\" name=\"id\""))
            {
                //<input type="hidden" name="id" value="2638" />
                var startIndex = line.IndexOf("value=") + 7;
                var endIndex = line.LastIndexOf('"');
                if (startIndex == -1 && endIndex == -1)
                    continue;

                curPackage!.DownloadId = line.Substring(startIndex, endIndex - startIndex);
                curPackage!.DllType = dllType;
                onlinePackages.Add(curPackage);
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
            foreach (var package in installedPackages) value.Add(package);
        }
    }

    private string getInstallPath(DllType dllType)
    {
        return Path.Combine(_settings.Directories.InstallPath, GetName(dllType));
    }

    internal bool IsInstalled(DllType dllType, string versionText)
    {
        if (InstalledPackages.TryGetValue(dllType, out var installedPackages))
            return installedPackages.Any(p => p.VersionDetailed == versionText);
        return false;
    }
}