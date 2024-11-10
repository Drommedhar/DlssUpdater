using System.ComponentModel;
using DlssUpdater;
using DLSSUpdater.Defines.UI.Pages;
using DlssUpdater.GameLibrary;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons;
using DlssUpdater.Singletons.AntiCheatChecker;
using Microsoft.Win32;
using NLog;

namespace DLSSUpdater.ViewModels.Pages;

public partial class SettingsCommonPageViewModel : ObservableObject
{
    private readonly GameContainer _gameContainer;
    private readonly Logger _logger;
    private readonly Settings _settings;

    private bool _allowSave;

    [ObservableProperty] private bool _battlEye;

    [ObservableProperty] private bool _easyAntiCheat;

    [ObservableProperty] private bool _epic;

    [ObservableProperty] private string _epicPath;

    [ObservableProperty] private bool _gog;

    [ObservableProperty] private bool _showNotifications;

    [ObservableProperty] private bool _steam;

    [ObservableProperty] private string _steamPath;

    [ObservableProperty] private bool _ubi;

    [ObservableProperty] private string _ubiPath;

    [ObservableProperty] private bool _xbox;

    public SettingsCommonPageViewModel(Settings settings, GameContainer gameContainer, Logger logger)
    {
        _settings = settings;
        _gameContainer = gameContainer;
        _logger = logger;
    }

    public void Init()
    {
        ShowNotifications = _settings.ShowNotifications;
        EasyAntiCheat = _settings.AntiCheatSettings.ActiveAntiCheatChecks.HasFlag(AntiCheatProvider.EasyAntiCheat);
        BattlEye = _settings.AntiCheatSettings.ActiveAntiCheatChecks.HasFlag(AntiCheatProvider.BattlEye);

        (Steam, SteamPath) = GetLibraryData(LibraryType.Steam);
        (Ubi, UbiPath) = GetLibraryData(LibraryType.Ubisoft);
        (Epic, EpicPath) = GetLibraryData(LibraryType.EpicGames);
        (Gog, _) = GetLibraryData(LibraryType.GOG);
        (Xbox, _) = GetLibraryData(LibraryType.Xbox);

        _allowSave = true;
    }

    private async void SetLibraryData(LibraryType libraryType, bool enabled, string? path)
    {
        var lib = _settings.Libraries.FirstOrDefault(l => l.LibraryType == libraryType);
        if (lib is null)
        {
            return;
        }

        if (lib.IsChecked == enabled)
        {
            return;
        }

        _logger.Debug($"Switched library '{lib.LibraryName}' to {lib.IsChecked}");

        lib.IsChecked = enabled;
        if (lib.NeedsInstallPath && path is not null)
        {
            lib.InstallPath = path;
        }

        await _gameContainer.ReloadLibraryGames(libraryType);
    }

    private Tuple<bool, string> GetLibraryData(LibraryType libraryType)
    {
        var bEnabled = false;
        var path = string.Empty;
        var lib = _settings.Libraries.FirstOrDefault(l => l.LibraryType == libraryType);
        if (lib != null)
        {
            bEnabled = lib.IsChecked;
            path = lib.InstallPath;
        }

        return Tuple.Create(bEnabled, path);
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (!_allowSave)
        {
            return;
        }

        if (e.PropertyName == nameof(ShowNotifications))
        {
            _settings.ShowNotifications = ShowNotifications;
            App.GetService<LibraryPage>()!.UpdateNotificationInfo();
            App.GetService<DLSSPage>()!.UpdateNotificationInfo();
        }

        if (e.PropertyName == nameof(EasyAntiCheat))
        {
            setAntiCheat(AntiCheatProvider.EasyAntiCheat, EasyAntiCheat);
            _gameContainer.RescanAntiCheat();
        }

        if (e.PropertyName == nameof(BattlEye))
        {
            setAntiCheat(AntiCheatProvider.BattlEye, BattlEye);
            _gameContainer.RescanAntiCheat();
        }

        SetLibraryData(LibraryType.Steam, Steam, SteamPath);
        SetLibraryData(LibraryType.Ubisoft, Ubi, UbiPath);
        SetLibraryData(LibraryType.EpicGames, Epic, EpicPath);
        SetLibraryData(LibraryType.GOG, Gog, null);
        SetLibraryData(LibraryType.Xbox, Xbox, null);

        _settings.Save();
    }

    public async void UpdateLibraryPath(LibraryType libraryType)
    {
        var lib = _settings.Libraries.FirstOrDefault(l => l.LibraryType == libraryType);
        if (lib is null)
        {
            return;
        }

        OpenFolderDialog dlg = new()
        {
            Multiselect = false
        };
        if (dlg.ShowDialog() == true)
        {
            lib.InstallPath = dlg.FolderName;
            _settings.Save();
            await _gameContainer.ReloadLibraryGames(lib.LibraryType);
        }
    }

    private void setAntiCheat(AntiCheatProvider antiCheat, bool set)
    {
        _settings.AntiCheatSettings.ActiveAntiCheatChecks =
            _settings.AntiCheatSettings.ActiveAntiCheatChecks.SetFlag(antiCheat, set);
    }
}