using System.Collections.ObjectModel;
using DlssUpdater.Defines;
using DlssUpdater.Singletons;
using DlssUpdater.Views.Pages;
using DLSSUpdater.Singletons;
using Wpf.Ui.Controls;

namespace DlssUpdater.ViewModels.Pages;

public partial class GamesViewModel : ObservableObject, INavigationAware
{
    private readonly GameContainer _gameContainer;
    private readonly string _notInstalled = "N/A";

    private readonly DllUpdater _updater;
    [ObservableProperty] private IEnumerable<InstalledPackage>? _dlss;

    [ObservableProperty] private IEnumerable<InstalledPackage>? _dlssD;

    [ObservableProperty] private string? _dlssDItem;

    [ObservableProperty] private bool _dlssDItemEnabled;

    [ObservableProperty] private string? _dlssDItemInstalled;

    [ObservableProperty] private IEnumerable<InstalledPackage>? _dlssG;

    [ObservableProperty] private string? _dlssGItem;

    [ObservableProperty] private bool _dlssGItemEnabled;

    [ObservableProperty] private string? _dlssGItemInstalled;

    [ObservableProperty] private string? _dlssItem;

    [ObservableProperty] private bool _dlssItemEnabled;

    [ObservableProperty] private string? _dlssItemInstalled;

    [ObservableProperty] private ObservableCollection<GameInfo>? _games;

    private bool _isInitialized;
    private GameInfo? _lastGame;

    [ObservableProperty] private GameInfo? _selectedGame;

    public GamesViewModel(DllUpdater updater, GameContainer gameContainer, AsyncFileWatcher watcher)
    {
        _updater = updater;
        _gameContainer = gameContainer;
        watcher.FilesChanged += Watcher_FilesChanged;
    }

    private void Watcher_FilesChanged(object? sender, EventArgs e)
    {
        Update();
    }

    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom()
    {
    }

    public void Update()
    {
        if (SelectedGame is null)
        {
            DlssItemInstalled = _notInstalled;
            DlssDItemInstalled = _notInstalled;
            DlssGItemInstalled = _notInstalled;
        }
        else
        {
            DlssItemInstalled = SelectedGame.InstalledDlls[DlssTypes.DllType.Dlss].Version ?? _notInstalled;
            DlssDItemInstalled = SelectedGame.InstalledDlls[DlssTypes.DllType.DlssD].Version ?? _notInstalled;
            DlssGItemInstalled = SelectedGame.InstalledDlls[DlssTypes.DllType.DlssG].Version ?? _notInstalled;
        }

        DlssItemEnabled = DlssItemInstalled != _notInstalled;
        DlssDItemEnabled = DlssDItemInstalled != _notInstalled;
        DlssGItemEnabled = DlssGItemInstalled != _notInstalled;

        if (_lastGame != SelectedGame)
        {
            DlssItem = string.Empty;
            DlssDItem = string.Empty;
            DlssGItem = string.Empty;
        }

        _lastGame = SelectedGame;
    }

    public void ApplyFilter(GamesPage.Filter filter)
    {
        foreach(var game in Games!)
        {
            if(filter == GamesPage.Filter.All)
            {
                game.Visible = game.IsHidden ? Visibility.Collapsed : Visibility.Visible;
            }
            else
            {
                game.Visible = game.IsHidden ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }

    private void InitializeViewModel()
    {
        Dlss = _updater.InstalledPackages[DlssTypes.DllType.Dlss];
        DlssD = _updater.InstalledPackages[DlssTypes.DllType.DlssD];
        DlssG = _updater.InstalledPackages[DlssTypes.DllType.DlssG];
        Games = _gameContainer.Games;

        _isInitialized = true;
    }
}