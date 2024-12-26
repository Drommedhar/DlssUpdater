using System.Windows.Controls;
using DLSSUpdater.ViewModels.Pages;

namespace DLSSUpdater.Views.Pages;

/// <summary>
///     Interaction logic for AboutPageControl.xaml
/// </summary>
public partial class AboutPageControl : UserControl
{
    public AboutPageControl(AboutPageViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        DataContext = this;
    }

    public AboutPageViewModel ViewModel { get; }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Init();
    }

    private void Hyperlink_RequestNavigate(object sender,
                                       System.Windows.Navigation.RequestNavigateEventArgs e)
    {
        System.Diagnostics.Process.Start("explorer.exe", e.Uri.AbsoluteUri);
    }
}