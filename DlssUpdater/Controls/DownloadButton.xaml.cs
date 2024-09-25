using System.Windows.Controls;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons;
using Wpf.Ui;
using Wpf.Ui.Controls;
using static DlssUpdater.Defines.DlssTypes;

namespace DlssUpdater.Controls;

/// <summary>
///     Interaction logic for DownloadButton.xaml
/// </summary>
public partial class DownloadButton : UserControl
{
    public static readonly DependencyProperty DllTypeProperty =
        DependencyProperty.Register("DllType", typeof(DllType), typeof(DownloadButton));

    public static readonly DependencyProperty VersionTextProperty =
        DependencyProperty.Register("VersionText", typeof(string), typeof(DownloadButton));

    public static readonly DependencyProperty ButtonIconProperty =
        DependencyProperty.Register("ButtonIcon", typeof(SymbolIcon), typeof(DownloadButton));

    private readonly SymbolRegular _addIcon = SymbolRegular.Add12;
    private readonly SymbolRegular _removeIcon = SymbolRegular.Prohibited48;
    private readonly ISnackbarService _snackbar;
    private readonly DllUpdater _updater;

    public DownloadButton()
    {
        _updater = App.GetService<DllUpdater>()!;
        _snackbar = App.GetService<ISnackbarService>()!;

        InitializeComponent();

        prgDownload.Visibility = Visibility.Hidden;
    }

    public DllType DllType
    {
        get => (DllType)GetValue(DllTypeProperty);
        set => SetValue(DllTypeProperty, value);
    }

    public string VersionText
    {
        get => (string)GetValue(VersionTextProperty);
        set => SetValue(VersionTextProperty, value);
    }

    public SymbolIcon ButtonIcon
    {
        get => (SymbolIcon)GetValue(ButtonIconProperty);
        set => SetValue(ButtonIconProperty, value);
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (isInstalled())
            doRemove();
        else
            doInstall();
    }

    private async void doInstall()
    {
        btnAction.Visibility = Visibility.Hidden;
        prgDownload.Value = 0;
        prgDownload.Visibility = Visibility.Visible;
        _updater.ProgressChanged += (totalFileSize, totalBytesDownloaded, progressPercentage) =>
        {
            prgDownload.Value = (int)progressPercentage!;
        };
        var success = await _updater.DownloadDll(DllType, VersionText);
        prgDownload.Visibility = Visibility.Hidden;

        if (success)
            _snackbar.ShowEx("Download", "Download was successful.", ControlAppearance.Success);
        else
            _snackbar.ShowEx("Download", "Download has failed. Please try again after some seconds.",
                ControlAppearance.Danger);

        isInstalled();
        btnAction.Visibility = Visibility.Visible;
    }

    private void doRemove()
    {
        btnAction.Visibility = Visibility.Hidden;
        _updater.RemoveInstalledDll(DllType, VersionText);
        isInstalled();
        btnAction.Visibility = Visibility.Visible;
    }

    private void downloadButton_Loaded(object sender, RoutedEventArgs e)
    {
        isInstalled();
    }

    private bool isInstalled()
    {
        if (_updater.IsInstalled(DllType, VersionText))
        {
            ButtonIcon = new SymbolIcon(_removeIcon);
            return true;
        }

        ButtonIcon = new SymbolIcon(_addIcon);
        return false;
    }
}