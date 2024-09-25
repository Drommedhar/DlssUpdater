using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using DlssUpdater.Singletons;
using DlssUpdater.Views.Pages;
using Wpf.Ui.Controls;

namespace DlssUpdater.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _applicationTitle = "DLSS Updater";

    [ObservableProperty] private ObservableCollection<object> _footerMenuItems = new()
    {
        new NavigationViewItem
        {
            Content = "Changelog",
            Icon = new SymbolIcon { Symbol = SymbolRegular.DocumentOnePage20 },
            TargetPageType = typeof(ChangelogPage)
        },
        new NavigationViewItem
        {
            Content = "Settings",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
            TargetPageType = typeof(SettingsPage)
        }
    };

    [ObservableProperty] private ObservableCollection<object> _menuItems = new()
    {
        
    };

    [ObservableProperty] private Visibility _dlssUpdateAvailable;
    [ObservableProperty] private Visibility _gameUpdateAvailable;

    [ObservableProperty] private ObservableCollection<Wpf.Ui.Controls.MenuItem> _trayMenuItems = new()
    {
        new Wpf.Ui.Controls.MenuItem { Header = "Home", Tag = "tray_home" }
    };

    [ObservableProperty] private WindowState _windowState;

    private readonly DllUpdater _updater;
    private readonly GameContainer _gameContainer;

    private NavigationViewItem GamesItem = new NavigationViewItem
    {
        Content = "Games",
        Icon = new SymbolIcon { Symbol = SymbolRegular.Games48 },
        TargetPageType = typeof(GamesPage),
        InfoBadge = new InfoBadge
        {
            Severity = InfoBadgeSeverity.Caution,
            Visibility = Visibility.Hidden,
            Opacity = 0.5
        }
    };

    private NavigationViewItem DlssItem = new NavigationViewItem
    {
        Content = "DLSS",
        Icon = new SymbolIcon { Symbol = SymbolRegular.Library28 },
        TargetPageType = typeof(DLSSPage),
        InfoBadge = new InfoBadge
        {
            Severity = InfoBadgeSeverity.Caution,
            Visibility = Visibility.Hidden,
            Opacity = 0.5
        }
    };

    public MainWindowViewModel(DllUpdater updater, GameContainer gameContainer)
    {
        _updater = updater;
        _gameContainer = gameContainer;
        MenuItems.Add(GamesItem);
        MenuItems.Add(DlssItem);
        updater.DlssFilesChanged += Updater_DlssFilesChanged;
        gameContainer.GamesChanged += GameContainer_GamesChanged;

        Binding notificationBinding = new()
        {
            Source = this,
            Path = new PropertyPath("DlssUpdateAvailable"),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        BindingOperations.SetBinding(DlssItem.InfoBadge, InfoBadge.VisibilityProperty, notificationBinding);

        Binding gameNotificationBinding = new()
        {
            Source = this,
            Path = new PropertyPath("GameUpdateAvailable"),
            Mode = BindingMode.TwoWay,
            UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        };
        BindingOperations.SetBinding(GamesItem.InfoBadge, InfoBadge.VisibilityProperty, gameNotificationBinding);
    }

    private void GameContainer_GamesChanged(object? sender, EventArgs e)
    {
        GameUpdateAvailable = _gameContainer.IsUpdateAvailable() ? Visibility.Visible : Visibility.Hidden;
    }

    private void Updater_DlssFilesChanged(object? sender, EventArgs e)
    {
        DlssUpdateAvailable = _updater.IsNewerVersionAvailable() ? Visibility.Visible : Visibility.Hidden;
    }
}