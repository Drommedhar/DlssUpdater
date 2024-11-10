using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Media;
using DlssUpdater;
using DLSSUpdater.Defines.UI;
using DLSSUpdater.Defines.UI.Pages;
using DlssUpdater.Views.Windows;

namespace DLSSUpdater.ViewModels.Windows;

public partial class MainWindowViewModel : ObservableObject
{
    private readonly Settings _settings;
    [ObservableProperty] private ObservableCollection<NavigationButton> _navigationButtons = [];
    [ObservableProperty] private HorizontalAlignment _subNavigationAlignment = HorizontalAlignment.Left;
    [ObservableProperty] private ObservableCollection<NavigationButton> _subNavigationButtons = [];

    [ObservableProperty] private WindowState _windowState;

    public MainWindowViewModel(LibraryPage libraryPage, DLSSPage dlssPage, SettingsPage settingsPage,
        ChangelogPage changelogPage, Settings settings)
    {
        _settings = settings;
        libraryPage.MainWindowViewModel = this;
        dlssPage.MainWindowViewModel = this;

        NavigationButtons.Add(new NavigationButton("Library", () => ShowPage(libraryPage), false));
        NavigationButtons.Add(new NavigationButton("DLSS", () => ShowPage(dlssPage), false));
        NavigationButtons.Add(new NavigationButton("Settings", () => ShowPage(settingsPage), false));
        NavigationButtons.Add(new NavigationButton("Changelog", () => ShowPage(changelogPage), false));
    }

    public MainWindow? Window { get; set; }

    public void UpdateNavigation(object sender)
    {
        var navigationButton = ((Button)sender)?.DataContext as NavigationButton;
        SelectNavigationButton(navigationButton, NavigationButtons);
        navigationButton?.OnClick();
    }

    public void UpdateSubNavigation(object sender)
    {
        var navigationButton = ((Button)sender)?.DataContext as NavigationButton;
        if (SubNavigationAlignment != HorizontalAlignment.Right)
        {
            SelectNavigationButton(navigationButton, SubNavigationButtons);
        }
        else
        {
            navigationButton?.OnClick();
        }
    }

    public void SelectNavigationButton(NavigationButton? navigationButton,
        ObservableCollection<NavigationButton> buttonList)
    {
        if (navigationButton == null)
        {
            return;
        }

        foreach (var button in buttonList)
        {
            if (button == navigationButton && !button.IsClicked)
            {
                button.Foreground = new SolidColorBrush(Color.FromArgb(255, 255, 255, 255));
                navigationButton?.OnClick();
            }
            else if (button != navigationButton)
            {
                button.Foreground = new SolidColorBrush(Color.FromArgb(125, 255, 255, 255));
                button.IsClicked = false;
            }
        }
    }

    public void UpdateLibraryNotification(bool bUpdate)
    {
        // TODO: Well, maybe we should not directly use the index... But hey
        var btn = NavigationButtons[0];
        if (btn is null || btn.Control is null)
        {
            return;
        }

        btn.Control.NotificationCount = "";
        if (_settings.ShowNotifications)
        {
            btn.Control.NotificationVisibility = bUpdate ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            btn.Control.NotificationVisibility = Visibility.Collapsed;
        }
    }

    public void UpdateDllsNotification(int count)
    {
        // TODO: Well, maybe we should not directly use the index... But hey
        var btn = NavigationButtons[1];
        if (btn is null || btn.Control is null)
        {
            return;
        }

        btn.Control.NotificationCount = count.ToString();
        if (_settings.ShowNotifications)
        {
            btn.Control.NotificationVisibility = count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }
        else
        {
            btn.Control.NotificationVisibility = Visibility.Collapsed;
        }
    }

    public void ReinitPageControl(IContentPage page)
    {
        if (Window is null)
        {
            return;
        }

        Window.GridContent.Children.Clear();
        Window.GridContent.Children.Add(page.GetPageControl());
    }

    private void ShowPage(IContentPage page)
    {
        SubNavigationButtons = page.GetNavigationButtons();
        SubNavigationAlignment = page.GetAlignment();
        ReinitPageControl(page);

        if (SubNavigationAlignment != HorizontalAlignment.Right)
        {
            SelectNavigationButton(SubNavigationButtons[0], SubNavigationButtons);
        }
    }
}