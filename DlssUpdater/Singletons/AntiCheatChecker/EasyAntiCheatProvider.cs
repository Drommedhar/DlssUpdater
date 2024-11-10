using System.IO;
using NLog;

namespace DlssUpdater.Singletons.AntiCheatChecker;

public class EasyAntiCheatProvider : IAntiCheatProvider
{
    private readonly Logger _logger;

    public EasyAntiCheatProvider(Logger logger)
    {
        _logger = logger;
    }

    public AntiCheatProvider ProviderType => AntiCheatProvider.EasyAntiCheat;

    public bool Check(string directory)
    {
        var allFiles = Directory.GetFiles(directory, "*EasyAntiCheat*", SearchOption.AllDirectories);
        _logger.Debug($"Checked EAC for '{directory}' and found {allFiles.Length} files");
        return allFiles.Length > 0;
    }
}