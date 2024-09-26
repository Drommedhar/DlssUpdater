using DlssUpdater.Defines;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons;
using DlssUpdater.Singletons.AntiCheatChecker;
using DLSSUpdater.Defines;
using Microsoft.VisualBasic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DlssUpdater.ViewModels.Pages;

public partial class SettingsViewModel : ObservableObject, INavigationAware
{
    [ObservableProperty] private Visibility _antiCheatDisclaimer = Visibility.Visible;
    [ObservableProperty] private bool _antiCheatEnabled = false;
    [ObservableProperty] private bool _antiCheatEACEnabled = false;
    [ObservableProperty] private bool _antiCheatBattlEyeEnabled = false;
    [ObservableProperty] string? _installPath;
    [ObservableProperty] string? _downloadPath;
    [ObservableProperty] string? _settingsPath;

    [ObservableProperty] private ObservableCollection<LibraryConfig>? _libraries;

    private bool _isInitialized = false;
    private Settings _settings;
    private GameContainer _gameContainer;

    public SettingsViewModel(Settings settings, GameContainer gameContainer)
    {
        _settings = settings;
        _gameContainer = gameContainer;

        InstallPath = settings.Directories.InstallPath;
        DownloadPath = settings.Directories.DownloadPath;
    }        

    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom()
    {
    }

    private void InitializeViewModel()
    {
        _isInitialized = true;

        UpdateAntiCheat(_settings.AntiCheatSettings.DisclaimerAccepted);
        AntiCheatEACEnabled = _settings.AntiCheatSettings.ActiveAntiCheatChecks.HasFlag(AntiCheatProvider.EasyAntiCheat);
        AntiCheatBattlEyeEnabled = _settings.AntiCheatSettings.ActiveAntiCheatChecks.HasFlag(AntiCheatProvider.BattlEye);
        Libraries = _settings.Libraries;
    }

    public void UpdateAntiCheat(bool enabled)
    {
        _settings.AntiCheatSettings.DisclaimerAccepted = enabled;
        AntiCheatEnabled = enabled;
        AntiCheatDisclaimer = enabled ? Visibility.Collapsed : Visibility.Visible;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if(e.PropertyName == nameof(AntiCheatEACEnabled))
        {
            setAntiCheat(AntiCheatProvider.EasyAntiCheat, AntiCheatEACEnabled);
            _gameContainer.RescanAntiCheat();
        }

        if (e.PropertyName == nameof(AntiCheatBattlEyeEnabled))
        {
            setAntiCheat(AntiCheatProvider.BattlEye, AntiCheatBattlEyeEnabled);
            _gameContainer.RescanAntiCheat();
        }

        _settings.Directories.InstallPath = InstallPath ?? _settings.Directories.InstallPath;
        _settings.Directories.DownloadPath = DownloadPath ?? _settings.Directories.DownloadPath;

        _settings.Save();
    }

    private void setAntiCheat(AntiCheatProvider antiCheat, bool set)
    {
        _settings.AntiCheatSettings.ActiveAntiCheatChecks =
            _settings.AntiCheatSettings.ActiveAntiCheatChecks.SetFlag(antiCheat, set);
    }
}