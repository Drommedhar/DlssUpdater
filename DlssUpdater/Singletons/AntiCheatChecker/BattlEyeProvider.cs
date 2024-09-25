using System.IO;

namespace DlssUpdater.Singletons.AntiCheatChecker;

public class BattlEyeProvider : IAntiCheatProvider
{
    public AntiCheatProvider ProviderType => AntiCheatProvider.BattlEye;

    private readonly NLog.Logger _logger;

    public BattlEyeProvider(NLog.Logger logger)
    {
        _logger = logger;
    }

    public bool Check(string directory)
    {
        var allFiles = Directory.GetFiles(directory, "*BEService*", SearchOption.AllDirectories);
        _logger.Debug($"Checked BattlEye for '{directory}' and found {allFiles.Length} files");
        return allFiles.Length > 0;
    }
}