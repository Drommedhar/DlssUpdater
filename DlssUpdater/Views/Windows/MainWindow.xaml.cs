using System.Windows.Media.Effects;
using AdonisUI.Controls;
using DLSSUpdater.Defines.UI.Pages;
using DLSSUpdater.Singletons;
using DLSSUpdater.ViewModels.Windows;

namespace DlssUpdater.Views.Windows;

public partial class MainWindow : AdonisWindow
{
    private readonly AsyncFileWatcher _fileWatcher;
    private readonly Settings _settings;

    private bool _isInitialized;

    public MainWindow(MainWindowViewModel viewModel, Settings settings, AsyncFileWatcher watcher)
    {
        ViewModel = viewModel;
        ViewModel.Window = this;
        _settings = settings;
        _fileWatcher = watcher;

        InitializeComponent();
        DataContext = this;
    }

    public MainWindowViewModel ViewModel { get; }

    public void SetEffect(bool active)
    {
        if (active)
        {
            Effect = new BlurEffect
            {
                KernelType = KernelType.Gaussian,
                Radius = 10,
                RenderingBias = RenderingBias.Performance
            };
        }
        else
        {
            Effect = null;
        }
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

    private void FluentWindow_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (!_isInitialized)
        {
            return;
        }

        _settings.WindowState = WindowState;
        _settings.Save();
    }

    private void NavigationButton_Clicked(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateNavigation(sender);
    }

    private void SubNavigationButton_Clicked(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateSubNavigation(sender);
    }

    private async void AdonisWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // We delay all Loaded calls until the main navigation buttons are actually present
        var navButtons = ViewModel.NavigationButtons.ToList();
        while (navButtons.Any(b => b.Control is null))
        {
            await Task.Delay(10);
        }

        ViewModel.WindowState = _settings.WindowState;
        _fileWatcher.Start();

        if (_settings.ShowChangelogOnStartup)
        {
            _settings.ShowChangelogOnStartup = false;
            _settings.Save();
            ViewModel.SelectNavigationButton(ViewModel.NavigationButtons[3], ViewModel.NavigationButtons);
        }
        else
        {
            ViewModel.SelectNavigationButton(ViewModel.NavigationButtons[0], ViewModel.NavigationButtons);
        }

        App.GetService<LibraryPage>()?.UpdateNotificationInfo();
        App.GetService<DLSSPage>()?.UpdateNotificationInfo();

        _isInitialized = true;
    }
}