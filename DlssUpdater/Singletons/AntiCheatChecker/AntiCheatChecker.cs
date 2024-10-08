using System.IO;

namespace DlssUpdater.Singletons.AntiCheatChecker;

public class AntiCheatChecker
{
    private Settings _settings;
    private readonly NLog.Logger _logger;
    private readonly List<IAntiCheatProvider> _providers = [];

    public AntiCheatChecker(Settings settings, NLog.Logger logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public AntiCheatProvider Check(string directory)
    {
        if (!Directory.Exists(directory)) return AntiCheatProvider.None;

        try
        {
	        return _providers.FirstOrDefault(provider => provider.Check(directory))?.ProviderType ?? AntiCheatProvider.None;
		}
        catch (UnauthorizedAccessException ex)
        {
	        _logger.Error($"AntiCheatChecker: {ex}");
	        return AntiCheatProvider.None;
        }
        
    }

    public void Init()
    {
        _providers.Clear();
        if (_settings.AntiCheatSettings.ActiveAntiCheatChecks.HasFlag(AntiCheatProvider.EasyAntiCheat))
        {
            _logger.Debug($"Add 'EasyAntiCheat' provider");
            _providers.Add(new EasyAntiCheatProvider(_logger));
        }
        if (_settings.AntiCheatSettings.ActiveAntiCheatChecks.HasFlag(AntiCheatProvider.BattlEye))
        {
            _logger.Debug($"Add 'BattlEye' provider");
            _providers.Add(new BattlEyeProvider(_logger));
        }
    }
}