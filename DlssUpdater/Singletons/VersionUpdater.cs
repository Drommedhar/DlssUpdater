using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Text.Json;
using DlssUpdater.Helpers;

namespace DLSSUpdater.Singletons;

public class ApplicationVersion
{
    public string? version { get; set; }
    public Version Get => new(version ?? "0.0.0.0");
}

public class VersionUpdater
{
    public async Task<bool> CheckForUpdate()
    {
        try
        {
            var updateUrl = "https://github.com/Drommedhar/DlssUpdater/releases/latest/download/version.json";
            var dataOnline = await WebHelper.HttpGet(updateUrl);
            var versionOnline = JsonSerializer.Deserialize<ApplicationVersion>(dataOnline, new JsonSerializerOptions());
            var data = await File.ReadAllTextAsync("version.json");
            var versionLocal = JsonSerializer.Deserialize<ApplicationVersion>(data, new JsonSerializerOptions());
            return versionOnline!.Get > versionLocal!.Get;
        }
        catch (Exception ex) when (ex is WebException || ex is JsonException)
        {
            return false;
        }
    }

    public async Task DoUpdate(string updateFile, string installPath, int processId)
    {
        try
        {
            // Wait for application to stop
            try
            {
                var proc = Process.GetProcessById(processId);
                if (proc is not null)
                {
                    await proc.WaitForExitAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }

            await clearFiles(installPath);

            ZipFile.ExtractToDirectory(updateFile, installPath, true);
            File.Delete(updateFile);

            var path = Path.Combine(installPath, "DLSSUpdater.exe");
            if (File.Exists(path))
            {
                Process.Start(path);
            }

            Application.Current.Shutdown();
        }
        catch (Exception)
        {
        }
    }

    private static async Task clearFiles(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        var files = Directory.GetFiles(path, "*.*", SearchOption.TopDirectoryOnly);
        foreach (var file in files)
        {
            if (file.Contains("update.zip") || file.Contains("ApplicationUpdater"))
            {
                continue;
            }

            await forceDelete(file);
        }
    }

    private static async Task forceDelete(string file)
    {
        try
        {
            File.Delete(file);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            await Task.Delay(100);
            await forceDelete(file);
        }
    }
}