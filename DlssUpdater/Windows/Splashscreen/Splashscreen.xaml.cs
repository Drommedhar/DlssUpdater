using System.Diagnostics;
using System.IO;
using System.Net.Http;
using AdonisUI.Controls;
using DlssUpdater.GameLibrary;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons;
using DLSSUpdater.Singletons;
using DlssUpdater.Singletons.AntiCheatChecker;
using DlssUpdater.ViewModels.Windows;
using DlssUpdater.Views.Windows;
using NLog;
using MessageBox = AdonisUI.Controls.MessageBox;
using MessageBoxImage = AdonisUI.Controls.MessageBoxImage;
using MessageBoxResult = AdonisUI.Controls.MessageBoxResult;
using Windows.Foundation;
using Windows.Services.Store;

namespace DlssUpdater.Windows.Splashscreen;

/// <summary>
///     Interaction logic for Splashscreen.xaml
/// </summary>
public partial class Splashscreen : Window
{
    private readonly GameContainer _gameContainer;
    private readonly Logger _logger;
    private readonly MainWindow _mainWindow;
    private readonly Settings _settings;
    private readonly DllUpdater _updater;
    private readonly VersionUpdater _versionUpdater;

    private bool _runInit = true;

    public Splashscreen(MainWindow mainWindow, DllUpdater updater, GameContainer container, Settings settings,
        AntiCheatChecker cheatChecker, VersionUpdater versionUpdater, Logger logger)
    {
        _updater = updater;
        _gameContainer = container;
        _gameContainer.LoadingProgress += _gameContainer_LoadingProgress;
        _gameContainer.InfoMessage += _gameContainer_InfoMessage;
        _versionUpdater = versionUpdater;
        _logger = logger;
        _logger.Debug("### STARTUP ###");

        settings.Load();
        _gameContainer.UpdateLibraries();
        _settings = settings;
        cheatChecker.Init();

        InitializeComponent();

        _mainWindow = mainWindow;
        DataContext = SplashscreenViewModel;
    }

    public SplashscreenViewModel SplashscreenViewModel { get; set; } = new();

    private void _gameContainer_InfoMessage(object? sender, string e)
    {
        SplashscreenViewModel.InfoText = e;
    }

    private void _gameContainer_LoadingProgress(object? sender, Tuple<int, int, LibraryType> e)
    {
        var (current, amount, libType) = e;
        SplashscreenViewModel.InfoText = $"Gathering installed games ({ILibrary.GetName(libType)}: {current}/{amount})";
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        if (!_runInit)
        {
            return;
        }

        _runInit = false;

        SplashscreenViewModel.InfoText = "Checking for update...";
        if (await DownloadUpdatesAsync())
        {
            Application.Current.Shutdown();
            return;
        }

        SplashscreenViewModel.InfoText = "Gathering installed versions...";
        _updater.GatherInstalledVersions();
        _updater.OnInfo += Updater_OnInfo;
        try
        {
            await _updater.UpdateDlssVersionsAsync();
        }
        catch (HttpRequestException ex)
        {
            _updater.OnInfo -= Updater_OnInfo;
            _logger.Error($"Request for UpdateDlssVersions failed: {ex}");
            SplashscreenViewModel.InfoText = $"ERROR: {ex}";
            await Task.Delay(5000);
            Application.Current.Shutdown();
            return;
        }

        _updater.OnInfo -= Updater_OnInfo;

        SplashscreenViewModel.InfoText = "Gathering installed games...";
        await _gameContainer.LoadGamesAsync();

        Visibility = Visibility.Hidden;
        _mainWindow.ShowDialog();
        Close();
    }

    private void Updater_OnInfo(object? sender, string e)
    {
        SplashscreenViewModel.InfoText = e;
    }

    private async Task<bool> DownloadUpdatesAsync()
    {
        StoreContext context = StoreContext.GetDefault();
        // Obtain window handle by passing in pointer to the window object
        var wih = new System.Windows.Interop.WindowInteropHelper(this);
        // Initialize the dialog using wrapper function for IInitializeWithWindow
        WinRT.Interop.InitializeWithWindow.Initialize(context, wih.Handle);
        IReadOnlyList<StorePackageUpdate> updates = await context.GetAppAndOptionalStorePackageUpdatesAsync();

        if (updates.Count > 0)
        {
            IAsyncOperationWithProgress<StorePackageUpdateResult, StorePackageUpdateStatus> downloadOperation =
                context.RequestDownloadStorePackageUpdatesAsync(updates);

            downloadOperation.Progress = async (asyncInfo, progress) =>
            {
                SplashscreenViewModel.InfoText = $"Downloading update ( {progress.PackageDownloadProgress}% )...";
            };

            StorePackageUpdateResult result = await downloadOperation.AsTask();
            if (result.OverallState == StorePackageUpdateState.Completed)
            {
                return await InstallUpdatesAsync(context);
            }
        }

        return false;
    }

    private async Task<bool> InstallUpdatesAsync(StoreContext context)
    {
        IReadOnlyList<StorePackageUpdate> updates = await context.GetAppAndOptionalStorePackageUpdatesAsync();
        IAsyncOperationWithProgress<StorePackageUpdateResult, StorePackageUpdateStatus> installOperation =
            context.RequestDownloadAndInstallStorePackageUpdatesAsync(updates);

        StorePackageUpdateResult result = await installOperation.AsTask();

        // Under normal circumstances, app will terminate here
        return result.OverallState == StorePackageUpdateState.Completed;

        // Handle error cases here using StorePackageUpdateResult from above
    }
}