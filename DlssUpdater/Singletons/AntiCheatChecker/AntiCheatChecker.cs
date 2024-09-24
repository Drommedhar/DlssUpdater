using System.IO;

namespace DlssUpdater.Singletons.AntiCheatChecker;

public class AntiCheatChecker
{
    private Settings _settings;
    private readonly List<IAntiCheatProvider> _providers = [];

    public AntiCheatChecker(Settings settings)
    {
        _settings = settings;
    }

    public bool Check(string directory)
    {
        if (!Directory.Exists(directory)) return false;

        return _providers.Any(provider => provider.Check(directory));
    }

    public void Init()
    {
        _providers.Clear();
        if (_settings.AntiCheatSettings.ActiveAntiCheatChecks.HasFlag(AntiCheatProvider.EasyAntiCheat))
        {
            _providers.Add(new EasyAntiCheatProvider());
        }
        if (_settings.AntiCheatSettings.ActiveAntiCheatChecks.HasFlag(AntiCheatProvider.BattlEye))
        {
            _providers.Add(new BattlEyeProvider());
        }
    }
}