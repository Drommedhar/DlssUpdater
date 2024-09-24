using System.Collections.ObjectModel;
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
            Content = "Settings",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 },
            TargetPageType = typeof(SettingsPage)
        }
    };

    [ObservableProperty] private ObservableCollection<object> _menuItems = new()
    {
        new NavigationViewItem
        {
            Content = "Games",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Games48 },
            TargetPageType = typeof(GamesPage)
        },
        new NavigationViewItem
        {
            Content = "DLSS",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Library28 },
            TargetPageType = typeof(DLSSPage),
            InfoBadge = new InfoBadge
            {
                Severity = InfoBadgeSeverity.Attention,
                // TODO: How to update visibility?!?
                Visibility =
                    Visibility
                        .Hidden //DlssUpdater.Get().IsNewerVersionAvailable() ? Visibility.Visible : Visibility.Collapsed,
            }
        }
    };

    [ObservableProperty] private ObservableCollection<MenuItem> _trayMenuItems = new()
    {
        new MenuItem { Header = "Home", Tag = "tray_home" }
    };
}