using System.IO;

namespace DlssUpdater.Singletons.AntiCheatChecker;

public class BattlEyeProvider : IAntiCheatProvider
{
    public AntiCheatProvider ProviderType => AntiCheatProvider.BattlEye;

    public bool Check(string directory)
    {
        var allFiles = Directory.GetFiles(directory, "*BEService*", SearchOption.AllDirectories);

        return allFiles.Length > 0;
    }
}