using DlssUpdater;
using DlssUpdater.Defines;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons;
using DlssUpdater.Views.Windows;
using DLSSUpdater.Singletons;
using DLSSUpdater.ViewModels.Windows;
using DLSSUpdater.Views.Pages;
using DLSSUpdater.Views.Windows;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DLSSUpdater.Defines.UI.Pages
{
    public partial class LibraryPage : ObservableObject, IContentPage
    {
        public MainWindowViewModel? MainWindowViewModel;

        [ObservableProperty]
        private GamePageControl _pageControl;

        private readonly GameContainer _gameContainer;
        private readonly AsyncFileWatcher _fileWatcher;
        private readonly DllUpdater _updater;

        public LibraryPage(GameContainer gameContainer, AsyncFileWatcher watcher, DllUpdater updater)
        {
            PageControl = App.GetService<GamePageControl>()!;
            _gameContainer = gameContainer;
            _gameContainer.GamesChanged += _gameContainer_GamesChanged;
            _fileWatcher = watcher;
            watcher.FilesChanged += Watcher_FilesChanged;
            _updater = updater;
        }

        private void _gameContainer_GamesChanged(object? sender, EventArgs e)
        {
            UpdateNotificationInfo();
        }

        private void Watcher_FilesChanged(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() => { UpdateNotificationInfo(); }));
        }

        public UserControl GetPageControl()
        {
            return PageControl;
        }

        ObservableCollection<NavigationButton> IContentPage.GetNavigationButtons()
        {
            return
            [
                new("Add game", () => { ShowGameConfig(); }, true),
            ];
        }

        public HorizontalAlignment GetAlignment()
        {
            return HorizontalAlignment.Right;
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
            var wndConfig = new GameConfigWindow(App.GetService<GameConfigWindowViewModel>()!, _gameContainer, _fileWatcher, _updater, null)
            {
                Width = 0,
                Height = 0,
                Owner = wndMain
            };
            wndConfig.ShowDialog();
            wndMain?.SetEffect(false);
        }
    }
}
