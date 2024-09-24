using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Net.Http;
using System.Reflection;
using System.Runtime;
using System.Security.Policy;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons;
using DlssUpdater.Singletons.AntiCheatChecker;
using DlssUpdater.ViewModels.Windows;
using DlssUpdater.Views.Windows;
using DLSSUpdater.Singletons;
using MessageBox = Wpf.Ui.Controls.MessageBox;
using MessageBoxResult = Wpf.Ui.Controls.MessageBoxResult;

namespace DlssUpdater.Windows.Splashscreen;

/// <summary>
///     Interaction logic for Splashscreen.xaml
/// </summary>
public partial class Splashscreen : Window
{
    private readonly GameContainer _gameContainer;
    private readonly MainWindow _mainWindow;
    private readonly DllUpdater _updater;
    private readonly VersionUpdater _versionUpdater;
    private readonly Settings _settings;

    private bool _runInit = true;

    public Splashscreen(MainWindow mainWindow, DllUpdater updater, GameContainer container, Settings settings,
                        AntiCheatChecker cheatChecker, VersionUpdater versionUpdater)
    {
        _updater = updater;
        _gameContainer = container;
        _versionUpdater = versionUpdater;
        settings.Load();
        _settings = settings;
        cheatChecker.Init();

        InitializeComponent();

        _mainWindow = mainWindow;
        DataContext = SplashscreenViewModel;
    }

    public SplashscreenViewModel SplashscreenViewModel { get; set; } = new();

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        if (!_runInit) return;

        _runInit = false;

        if(File.Exists(Path.Combine(AppContext.BaseDirectory, "update.txt")))
        {
            SplashscreenViewModel.InfoText = "Updating...";
            var data = await File.ReadAllTextAsync(Path.Combine(AppContext.BaseDirectory, "update.txt"));
            var args = data.Split(" ", StringSplitOptions.RemoveEmptyEntries);
            if(args.Length == 3)
            {
                // We are in update mode
                await _versionUpdater.DoUpdate(args[0], args[1], int.Parse(args[2]));
                return;
            }
        }

        SplashscreenViewModel.InfoText = "Checking for update...";

        // Clear download folder
        if(Directory.Exists(_settings.Directories.DownloadPath))
        {
            Directory.Delete(_settings.Directories.DownloadPath, true);
        }

        var updateAvailable = await _versionUpdater.CheckForUpdate();
        bool installUpdate = false;
        if(updateAvailable)
        {
            var uiMessageBox = new MessageBox
            {
                Title = "Update available",
                Content = "An update for Dlss Updater is available.\nDo you want to download the update?",
                SecondaryButtonText = "Yes",
                CloseButtonText = "No",
                IsPrimaryButtonEnabled = false,
                IsSecondaryButtonEnabled = true
            };

            installUpdate = await uiMessageBox.ShowDialogAsync() == MessageBoxResult.Secondary;
        }

        if(installUpdate)
        {
            _settings.ShowChangelogOnStartup = true;
            _settings.Save();

            // Download the update and start the ApplicationUpdater
            var outputPath = Path.Combine(AppContext.BaseDirectory, _settings.Directories.DownloadPath, "update.zip");
            var updateUrl = "http://github.com/Drommedhar/DlssUpdater/releases/latest/download/dlssupdater.zip";
            DirectoryHelper.EnsureDirectoryExists(_settings.Directories.DownloadPath);
            using var client = new HttpClientDownloadWithProgress(updateUrl, outputPath);
            client.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
            {
                SplashscreenViewModel.InfoText = $"Downloading update ({progressPercentage}%)...";
            };
            
            await client.StartDownload();

            // Copy over the application updater files
            var files = Directory.GetFiles(AppContext.BaseDirectory, "*.*", SearchOption.TopDirectoryOnly);
            foreach ( var file in files)
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(_settings.Directories.DownloadPath, fileName), true);
                File.SetAttributes(file, FileAttributes.Normal);
            }

            // Now let's finally start the updater
            var updateArgs = $"{outputPath} {AppContext.BaseDirectory} {Environment.ProcessId}";
            await File.WriteAllTextAsync(Path.Combine(_settings.Directories.DownloadPath, "update.txt"), updateArgs);
            Process.Start(Path.Combine(_settings.Directories.DownloadPath, "DlssUpdater.exe"));
            Application.Current.Shutdown();
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
            SplashscreenViewModel.InfoText = $"ERROR: {ex}";
            await Task.Delay(5000);
            Application.Current.Shutdown();
            return;
        }

        _updater.OnInfo -= Updater_OnInfo;

        SplashscreenViewModel.InfoText = "Gathering installed games...";
        await _gameContainer.LoadGamesAsync();

        Visibility = Visibility.Hidden;
        _mainWindow.ShowWindow();
        Close();
    }

    private void Updater_OnInfo(object? sender, string e)
    {
        SplashscreenViewModel.InfoText = e;
    }
}