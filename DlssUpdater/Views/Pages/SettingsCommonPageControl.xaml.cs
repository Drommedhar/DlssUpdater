using System.Windows.Controls;
using DlssUpdater.GameLibrary;
using DLSSUpdater.ViewModels.Pages;

namespace DLSSUpdater.Views.Pages;

/// <summary>
///     Interaction logic for SettingsCommonPageControl.xaml
/// </summary>
public partial class SettingsCommonPageControl : UserControl
{
    public SettingsCommonPageControl(SettingsCommonPageViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        DataContext = this;
    }

    public SettingsCommonPageViewModel ViewModel { get; }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Init();
    }

    private async void btnPathUbi_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateLibraryPath(LibraryType.Ubisoft);
    }

    private async void btnPathSteam_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateLibraryPath(LibraryType.Steam);
    }

    private async void btnPathEpic_Click(object sender, RoutedEventArgs e)
    {
        ViewModel.UpdateLibraryPath(LibraryType.EpicGames);
    }
}