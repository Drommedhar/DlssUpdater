using DlssUpdater.ViewModels.Windows;
using DlssUpdater.Views.Pages;
using DLSSUpdater.Singletons;
using System.Runtime;
using Wpf.Ui;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DlssUpdater.Views.Windows;

public partial class MainWindow : INavigationWindow
{
    public MainWindowViewModel ViewModel { get; }

    private readonly Settings _settings;

    public MainWindow(MainWindowViewModel viewModel, IPageService pageService, INavigationService navigationService,
        ISnackbarService snackbarService, Settings settings, AsyncFileWatcher watcher)
    {
        ViewModel = viewModel;
        _settings = settings;
        DataContext = this;

        InitializeComponent();
        SetPageService(pageService);

        snackbarService.SetSnackbarPresenter(SnackbarPresenter);
        navigationService.SetNavigationControl(RootNavigation);

        watcher.Start();
    }

    INavigationView INavigationWindow.GetNavigation()
    {
        throw new NotImplementedException();
    }

    public void SetServiceProvider(IServiceProvider serviceProvider)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    ///     Raises the closed event.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Make sure that closing this window will begin the process of closing the application.
        Application.Current.Shutdown();
    }

    #region INavigationWindow methods

    public INavigationView GetNavigation()
    {
        return RootNavigation;
    }

    public bool Navigate(Type pageType)
    {
        return RootNavigation.Navigate(pageType);
    }

    public void SetPageService(IPageService pageService)
    {
        RootNavigation.SetPageService(pageService);
    }

    public void ShowWindow()
    {
        ViewModel.WindowState = _settings.WindowState;

        Show();
        if (_settings.ShowChangelogOnStartup)
        {
            Navigate(typeof(ChangelogPage));
            _settings.ShowChangelogOnStartup = false;
            _settings.Save();
        }
        else
        {
            Navigate(typeof(GamesPage));
        }
    }

    public void CloseWindow()
    {
        Close();
    }

    #endregion INavigationWindow methods

    private void FluentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        _settings.WindowState = WindowState;
        _settings.Save();
    }
}