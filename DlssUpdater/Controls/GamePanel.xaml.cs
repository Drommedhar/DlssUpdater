using System.Windows.Controls;
using DlssUpdater;
using DlssUpdater.Defines;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons;
using DLSSUpdater.Singletons;
using DLSSUpdater.ViewModels.Windows;
using DLSSUpdater.Views.Pages;
using DlssUpdater.Views.Windows;
using DLSSUpdater.Views.Windows;
using NLog;

namespace DLSSUpdater.Controls;

/// <summary>
///     Interaction logic for GamePanel.xaml
/// </summary>
public partial class GamePanel : UserControl
{
    public static readonly DependencyProperty GameInfoProperty =
        DependencyProperty.Register("GameInfo", typeof(GameInfo), typeof(GamePanel));

    private readonly AsyncFileWatcher _fileWatcher;
    private readonly GameContainer _gameContainer;

    private readonly GamePageControl _gamesPage;
    private readonly Logger _logger;
    private readonly DllUpdater _updater;

    public GamePanel()
    {
        _gamesPage = App.GetService<GamePageControl>()!;
        _gameContainer = App.GetService<GameContainer>()!;
        _updater = App.GetService<DllUpdater>()!;
        _fileWatcher = App.GetService<AsyncFileWatcher>()!;
        _logger = App.GetService<Logger>()!;

        InitializeComponent();
    }

    public GameInfo GameInfo
    {
        get => (GameInfo)GetValue(GameInfoProperty);
        set => SetValue(GameInfoProperty, value);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        GameInfo.IsHidden = !GameInfo.IsHidden;
        _gamesPage.UpdateFilter();
        _gameContainer.SaveGames();
        _gameContainer.DoUpdate();
    }

    private void Button_Click_1(object sender, RoutedEventArgs e)
    {
        var wndMain = App.GetService<MainWindow>();
        wndMain?.SetEffect(true);
        var wndConfig = new GameConfigWindow(App.GetService<GameConfigWindowViewModel>()!, _gameContainer, _fileWatcher,
            _updater, _logger, GameInfo)
        {
            Width = 0,
            Height = 0,
            Owner = wndMain
        };
        WindowPositionHelper.CenterWindowToParent(wndConfig, wndMain!);
        wndConfig.ShowDialog();
        wndMain?.SetEffect(false);
    }

    private void btnRemove_Click(object sender, RoutedEventArgs e)
    {
        _gameContainer.RemoveGame(GameInfo);
        _gameContainer.SaveGames();
    }
}