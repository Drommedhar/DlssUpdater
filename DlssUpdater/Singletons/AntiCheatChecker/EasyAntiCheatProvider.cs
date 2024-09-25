using System.IO;

namespace DlssUpdater.Singletons.AntiCheatChecker;

public class EasyAntiCheatProvider : IAntiCheatProvider
{
    public AntiCheatProvider ProviderType => AntiCheatProvider.EasyAntiCheat;

    private readonly NLog.Logger _logger;
    
    public EasyAntiCheatProvider(NLog.Logger logger)
    {
        _logger = logger;
    }

    public bool Check(string directory)
    {
        var allFiles = Directory.GetFiles(directory, "*EasyAntiCheat*", SearchOption.AllDirectories);
        _logger.Debug($"Checked EAC for '{directory}' and found {allFiles.Length} files");
        return allFiles.Length > 0;
    }
}