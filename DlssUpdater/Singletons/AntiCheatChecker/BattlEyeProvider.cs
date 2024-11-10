using System.IO;
using NLog;

namespace DlssUpdater.Singletons.AntiCheatChecker;

public class BattlEyeProvider : IAntiCheatProvider
{
    private readonly Logger _logger;

    public BattlEyeProvider(Logger logger)
    {
        _logger = logger;
    }

    public AntiCheatProvider ProviderType => AntiCheatProvider.BattlEye;

    public bool Check(string directory)
    {
        var allFiles = Directory.GetFiles(directory, "*BEService*", SearchOption.AllDirectories);
        _logger.Debug($"Checked BattlEye for '{directory}' and found {allFiles.Length} files");
        return allFiles.Length > 0;
    }
}