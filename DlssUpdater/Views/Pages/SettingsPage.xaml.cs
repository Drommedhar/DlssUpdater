using DlssUpdater.ViewModels.Pages;
using Microsoft.Win32;
using Wpf.Ui.Controls;

namespace DlssUpdater.Views.Pages;

public partial class SettingsPage : INavigableView<SettingsViewModel>
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public SettingsViewModel ViewModel { get; }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateAntiCheat(true);
    }

    private void ButtonPathInstall_Click(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog dlg = new()
        {
            Multiselect = false
        };
        if (dlg.ShowDialog() == true)
        {
            ViewModel.InstallPath = dlg.FolderName;
        }
    }

    private void ButtonPathDownload_Click(object sender, RoutedEventArgs e)
    {
        OpenFolderDialog dlg = new()
        {
            Multiselect = false
        };
        if (dlg.ShowDialog() == true)
        {
            ViewModel.DownloadPath = dlg.FolderName;
        }
    }
}