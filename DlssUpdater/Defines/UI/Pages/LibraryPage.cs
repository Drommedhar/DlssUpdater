using System.Collections.ObjectModel;
using System.Windows.Controls;
using DlssUpdater;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons;
using DLSSUpdater.Singletons;
using DLSSUpdater.ViewModels.Windows;
using DLSSUpdater.Views.Pages;
using DlssUpdater.Views.Windows;
using DLSSUpdater.Views.Windows;
using NLog;

namespace DLSSUpdater.Defines.UI.Pages;

public partial class LibraryPage : ObservableObject, IContentPage
{
    private readonly AsyncFileWatcher _fileWatcher;

    private readonly GameContainer _gameContainer;
    private readonly Logger _logger;
    private readonly DllUpdater _updater;

    [ObservableProperty] private GamePageControl _pageControl;

    public MainWindowViewModel? MainWindowViewModel;

    public LibraryPage(GameContainer gameContainer, AsyncFileWatcher watcher, DllUpdater updater, Logger logger)
    {
        PageControl = App.GetService<GamePageControl>()!;
        _logger = logger;
        _gameContainer = gameContainer;
        _gameContainer.GamesChanged += _gameContainer_GamesChanged;
        _fileWatcher = watcher;
        watcher.FilesChanged += Watcher_FilesChanged;
        _updater = updater;
    }

    public UserControl GetPageControl()
    {
        return PageControl;
    }

    ObservableCollection<NavigationButton> IContentPage.GetNavigationButtons()
    {
        return
        [
            new NavigationButton("Add game", () => { ShowGameConfig(); }, true)
        ];
    }

    public HorizontalAlignment GetAlignment()
    {
        return HorizontalAlignment.Right;
    }

    private void _gameContainer_GamesChanged(object? sender, EventArgs e)
    {
        UpdateNotificationInfo();
    }

    private void Watcher_FilesChanged(object? sender, EventArgs e)
    {
        Application.Current.Dispatcher.Invoke(() => { UpdateNotificationInfo(); });
    }

    public void UpdateNotificationInfo()
    {
        var hasUpdate = _gameContainer.IsUpdateAvailable();
        MainWindowViewModel?.UpdateLibraryNotification(hasUpdate);
    }

    private void ShowGameConfig()
    {
        var wndMain = App.GetService<MainWindow>();
        wndMain?.SetEffect(true);
        var wndConfig = new GameConfigWindow(App.GetService<GameConfigWindowViewModel>()!, _gameContainer, _fileWatcher,
            _updater, _logger, null)
        {
            Width = 0,
            Height = 0,
            Owner = wndMain
        };
        WindowPositionHelper.CenterWindowToParent(wndConfig, wndMain!);
        wndConfig.ShowDialog();
        wndMain?.SetEffect(false);
    }
}