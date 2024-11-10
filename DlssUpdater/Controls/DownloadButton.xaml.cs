using System.Windows.Controls;
using DlssUpdater;
using DlssUpdater.Singletons;
using static DlssUpdater.Defines.DlssTypes;

namespace DLSSUpdater.Controls;

/// <summary>
///     Interaction logic for DownloadButton.xaml
/// </summary>
public partial class DownloadButton : UserControl
{
    public static readonly DependencyProperty DllTypeProperty =
        DependencyProperty.Register("DllType", typeof(DllType), typeof(DownloadButton));

    public static readonly DependencyProperty VersionTextProperty =
        DependencyProperty.Register("VersionText", typeof(string), typeof(DownloadButton));

    private readonly DllUpdater _updater;

    public DownloadButton()
    {
        _updater = App.GetService<DllUpdater>()!;

        InitializeComponent();

        downloadIcon.Visibility = Visibility.Hidden;
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

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (isInstalled())
        {
            doRemove();
        }
        else
        {
            doInstall();
        }
    }

    private async void doInstall()
    {
        lblAction.Visibility = Visibility.Hidden;
        downloadIcon.Visibility = Visibility.Visible;
        var (success, errorString) = await _updater.DownloadDll(DllType, VersionText);
        downloadIcon.Visibility = Visibility.Hidden;

        //if (success)
        //    _snackbar.ShowEx("Download", "Download was successful.", ControlAppearance.Success);
        //else
        //    _snackbar.ShowEx("Download", $"Download has failed. {errorString}",
        //        ControlAppearance.Danger);

        isInstalled();
        lblAction.Visibility = Visibility.Visible;
    }

    private void doRemove()
    {
        lblAction.Visibility = Visibility.Hidden;
        btnAction.Visibility = Visibility.Hidden;
        _updater.RemoveInstalledDll(DllType, VersionText);
        isInstalled();
        lblAction.Visibility = Visibility.Visible;
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
            lblAction.Content = "Remove";
            return true;
        }

        lblAction.Content = "Install";
        return false;
    }
}