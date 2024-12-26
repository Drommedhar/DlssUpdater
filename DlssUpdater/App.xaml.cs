using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using AdonisUI.Controls;
using DLSSUpdater.Defines.UI.Pages;
using DlssUpdater.Helpers;
using DlssUpdater.Services;
using DlssUpdater.Singletons;
using DLSSUpdater.Singletons;
using DlssUpdater.Singletons.AntiCheatChecker;
using DLSSUpdater.ViewModels.Pages;
using DLSSUpdater.ViewModels.Windows;
using DLSSUpdater.Views.Pages;
using DlssUpdater.Views.Windows;
using DlssUpdater.Windows.Splashscreen;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NLog;
using WpfBindingErrors;
using MessageBox = AdonisUI.Controls.MessageBox;

namespace DlssUpdater;

/// <summary>
///     Interaction logic for App.xaml
/// </summary>
public partial class App
{
    private const string ISSUE_BUTTON_ID = "GithubIssue";

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

            // Windows
            services.AddSingleton<Splashscreen>();
            services.AddSingleton<MainWindow>();

            // ViewModel
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<DlssPageViewModel>();
            services.AddSingleton<GamePageViewModel>();
            services.AddSingleton<GameConfigWindowViewModel>();
            services.AddSingleton<SettingsCommonPageViewModel>();
            services.AddSingleton<AboutPageViewModel>();

            services.AddSingleton(LogManager.GetCurrentClassLogger());

            services.AddSingleton<Settings>();
            services.AddSingleton<DllUpdater>();
            services.AddSingleton<GameContainer>();
            services.AddSingleton<AntiCheatChecker>();
            services.AddSingleton<VersionUpdater>();
            services.AddSingleton<AsyncFileWatcher>();

            // Pages
            services.AddSingleton<LibraryPage>();
            services.AddSingleton<DLSSPage>();
            services.AddSingleton<SettingsPage>();
            services.AddSingleton<AboutPage>();
            services.AddSingleton<DlssPageControl>();
            services.AddSingleton<GamePageControl>();
        }).Build();

    /// <summary>
    ///     Gets registered service.
    /// </summary>
    /// <typeparam name="T">Type of the service to get.</typeparam>
    /// <returns>Instance of the service or <see langword="null" />.</returns>
    public static T? GetService<T>()
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
        var messageBox = new MessageBoxModel
        {
            Caption = "Fatal error",
            Text =
                $"Application has crashed unexpectedly.\n" +
                $"To open a new issue click the corresponding button below.",
            Buttons =
            [
                MessageBoxButtons.Ok(),
                MessageBoxButtons.Custom("Open github issue", ISSUE_BUTTON_ID)
            ]
        };
        
        _ = MessageBox.Show(messageBox);
        if (messageBox.ButtonPressed?.Id as string == ISSUE_BUTTON_ID)
        {
            var body = $"&body={Uri.EscapeDataString($"Encountered an unhandled exception: \n ```{e.Exception}```")}";
            var labels = "&labels=exception";
            var title = $"&title=Unhandled%20Exception - '{e.Exception.Message}'";
            var url =
                $"https://github.com/Drommedhar/DlssUpdater/issues/new?assignees=&labels=bug&projects=&template=bug_report.md" +
                $"{title}{labels}{body}";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        
        e.Handled = true;
        Application.Current?.Shutdown();
    }
}