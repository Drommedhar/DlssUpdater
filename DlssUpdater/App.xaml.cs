using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Windows.Threading;
using AdonisUI.Controls;
using DlssUpdater.Helpers;
using DlssUpdater.Services;
using DlssUpdater.Singletons;
using DlssUpdater.Singletons.AntiCheatChecker;
using DlssUpdater.Views.Windows;
using DlssUpdater.Windows.Splashscreen;
using DLSSUpdater.Defines.UI.Pages;
using DLSSUpdater.Singletons;
using DLSSUpdater.ViewModels.Pages;
using DLSSUpdater.ViewModels.Windows;
using DLSSUpdater.Views.Pages;
using DLSSUpdater.Views.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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

            // Windows
            services.AddSingleton<Splashscreen>();
            services.AddSingleton<MainWindow>();

            // ViewModel
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<DlssPageViewModel>();
            services.AddSingleton<GamePageViewModel>();
            services.AddSingleton<GameConfigWindowViewModel>();
            services.AddSingleton<SettingsCommonPageViewModel>();
            services.AddSingleton<ChangelogPageViewModel>();

            services.AddSingleton(NLog.LogManager.GetCurrentClassLogger());

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
            services.AddSingleton<ChangelogPage>();
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

    const string ISSUE_BUTTON_ID = "GithubIssue";
    /// <summary>
    ///     Occurs when an exception is thrown by an application but not handled.
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        var time = DateTime.UtcNow.ToString("s").Replace(":", ".");
        var file = $"Dumps/{time}.dmp";
        CreateMiniDump(file);

        var messageBox = new MessageBoxModel
        {
            Caption = "Fatal error",
            Text = $"Application has crashed unexpectedly. A dump file was created at '{file}'. Please provide this in your github issue (as a download link).\n" +
            $"To open a new issue click the corresponding button below.",
            Buttons = [
                MessageBoxButtons.Ok(),
                MessageBoxButtons.Custom("Open github issue", ISSUE_BUTTON_ID),
            ]
        };

        _ = AdonisUI.Controls.MessageBox.Show(messageBox);
        if((string)messageBox.ButtonPressed.Id == ISSUE_BUTTON_ID)
        {
            var body = $"&body={Uri.EscapeDataString($"Encountered an unhandled exception: \n ```{e.Exception}```")}";
            var labels = "&labels=exception";
            var title = $"&title=Unhandled%20Exception - '{e.Exception.Message}'";
            var url = $"https://github.com/Drommedhar/DlssUpdater/issues/new?assignees=&labels=bug&projects=&template=bug_report.md" +
                $"{title}{labels}{body}";
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }

        e.Handled = true;
    }

    public static class MINIDUMP_TYPE
    {
        public const int MiniDumpNormal = 0x00000000;
        public const int MiniDumpWithDataSegs = 0x00000001;
        public const int MiniDumpWithFullMemory = 0x00000002;
        public const int MiniDumpWithHandleData = 0x00000004;
        public const int MiniDumpFilterMemory = 0x00000008;
        public const int MiniDumpScanMemory = 0x00000010;
        public const int MiniDumpWithUnloadedModules = 0x00000020;
        public const int MiniDumpWithIndirectlyReferencedMemory = 0x00000040;
        public const int MiniDumpFilterModulePaths = 0x00000080;
        public const int MiniDumpWithProcessThreadData = 0x00000100;
        public const int MiniDumpWithPrivateReadWriteMemory = 0x00000200;
        public const int MiniDumpWithoutOptionalData = 0x00000400;
        public const int MiniDumpWithFullMemoryInfo = 0x00000800;
        public const int MiniDumpWithThreadInfo = 0x00001000;
        public const int MiniDumpWithCodeSegs = 0x00002000;
    }

    [DllImport("dbghelp.dll")]
    public static extern bool MiniDumpWriteDump(IntPtr hProcess,
                                                Int32 ProcessId,
                                                IntPtr hFile,
                                                int DumpType,
                                                IntPtr ExceptionParam,
                                                IntPtr UserStreamParam,
                                                IntPtr CallackParam);

    private static void CreateMiniDump(string file)
    {
        DirectoryHelper.EnsureDirectoryExists("Dumps");
        using FileStream fs = new(file, FileMode.Create);
        using Process process = Process.GetCurrentProcess();
        MiniDumpWriteDump(process.Handle,
                                         process.Id,
                                         fs.SafeFileHandle.DangerousGetHandle(),
                                         MINIDUMP_TYPE.MiniDumpWithFullMemory,
                                         IntPtr.Zero,
                                         IntPtr.Zero,
                                         IntPtr.Zero);
    }
}