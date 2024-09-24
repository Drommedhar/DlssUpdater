using System.IO;

namespace DlssUpdater.Singletons.AntiCheatChecker;

public class EasyAntiCheatProvider : IAntiCheatProvider
{
    public AntiCheatProvider ProviderType => AntiCheatProvider.EasyAntiCheat;

    public bool Check(string directory)
    {
        var allFiles = Directory.GetFiles(directory, "*EasyAntiCheat*", SearchOption.AllDirectories);

        return allFiles.Length > 0;
    }
}