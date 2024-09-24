using System.IO;
using System.Reflection;
using System.Windows.Threading;
using DlssUpdater.Services;
using DlssUpdater.Singletons;
using DlssUpdater.Singletons.AntiCheatChecker;
using DlssUpdater.ViewModels.Pages;
using DlssUpdater.ViewModels.Windows;
using DlssUpdater.Views.Pages;
using DlssUpdater.Views.Windows;
using DlssUpdater.Windows.Splashscreen;
using DLSSUpdater.Singletons;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Wpf.Ui;
using WpfBindingErrors;

namespace DlssUpdater;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
    // https://docs.microsoft.com/dotnet/core/extensions/generic-host
    // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
    // https://docs.microsoft.com/dotnet/core/extensions/configuration
    // https://docs.microsoft.com/dotnet/core/extensions/logging
    private static readonly IHost _host = Host
        .CreateDefaultBuilder()
        .ConfigureAppConfiguration(
            c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)!); })
        .ConfigureServices((context, services) =>
        {
            services.AddHostedService<ApplicationHostService>();

            // Page resolver service
            services.AddSingleton<IPageService, PageService>();

            // Theme manipulation
            services.AddSingleton<IThemeService, ThemeService>();

            // TaskBar manipulation
            services.AddSingleton<ITaskBarService, TaskBarService>();


            // Service containing navigation, same as INavigationWindow... but without window
            services.AddSingleton<INavigationService, NavigationService>();

            // Main window with navigation
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<MainWindow>();
            services.AddSingleton<Splashscreen>();
            services.AddSingleton<GamesPage>();
            services.AddSingleton<GamesViewModel>();
            services.AddSingleton<DLSSPage>();
            services.AddSingleton<DLSSViewModel>();
            services.AddSingleton<SettingsPage>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<ChangelogPage>();
            services.AddSingleton<ChangelogViewModel>();

            services.AddSingleton<Settings>();
            services.AddSingleton<DllUpdater>();
            services.AddSingleton<GameContainer>();
            services.AddSingleton<AntiCheatChecker>();
            services.AddSingleton<VersionUpdater>();

            services.AddSingleton<ISnackbarService, SnackbarService>();
        }).Build();

    /// <summary>
    ///     Gets registered service.
    /// </summary>
    /// <typeparam name="T">Type of the service to get.</typeparam>
    /// <returns>Instance of the service or <see langword="null" />.</returns>
    public static T GetService<T>()
        where T : class
    {
        return _host.Services.GetService(typeof(T)) as T;
    }

    /// <summary>
    ///     Occurs when the application is loading.
    /// </summary>
    private void OnStartup(object sender, StartupEventArgs e)
    {
        _host.Start();

        // Start listening for WPF binding error.
        // After that line, a BindingException will be thrown each time
        // a binding error occurs.
        BindingExceptionThrower.Attach();
    }

    /// <summary>
    ///     Occurs when the application is closing.
    /// </summary>
    private async void OnExit(object sender, ExitEventArgs e)
    {
        await _host.StopAsync();

        _host.Dispose();
    }

    /// <summary>
    ///     Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
    }
}