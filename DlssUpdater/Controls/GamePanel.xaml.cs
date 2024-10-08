using DlssUpdater;
using DlssUpdater.Defines;
using DlssUpdater.Singletons;
using DLSSUpdater.ViewModels.Windows;
using DlssUpdater.Views.Windows;
using DLSSUpdater.Views.Pages;
using DLSSUpdater.Views.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DLSSUpdater.Singletons;
using DlssUpdater.Helpers;

namespace DLSSUpdater.Controls
{
    /// <summary>
    /// Interaction logic for GamePanel.xaml
    /// </summary>
    public partial class GamePanel : UserControl
    {
        public static readonly DependencyProperty GameInfoProperty =
        DependencyProperty.Register("GameInfo", typeof(GameInfo), typeof(GamePanel));

        public GameInfo GameInfo
        {
            get => (GameInfo)GetValue(GameInfoProperty);
            set => SetValue(GameInfoProperty, value);
        }

        private readonly GamePageControl _gamesPage;
        private readonly GameContainer _gameContainer;
        private readonly DllUpdater _updater;
        private readonly AsyncFileWatcher _fileWatcher;
        private readonly NLog.Logger _logger;

        public GamePanel()
        {
            _gamesPage = App.GetService<GamePageControl>()!;
            _gameContainer = App.GetService<GameContainer>()!;
            _updater = App.GetService<DllUpdater>()!;
            _fileWatcher = App.GetService<AsyncFileWatcher>()!;
            _logger = App.GetService<NLog.Logger>()!;

            InitializeComponent();
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
            var wndConfig = new GameConfigWindow(App.GetService<GameConfigWindowViewModel>()!, _gameContainer, _fileWatcher, _updater, _logger, GameInfo)
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
}
