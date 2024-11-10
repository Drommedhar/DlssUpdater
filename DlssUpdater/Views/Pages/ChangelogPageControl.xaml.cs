using System.Windows.Controls;
using DLSSUpdater.ViewModels.Pages;

namespace DLSSUpdater.Views.Pages;

/// <summary>
///     Interaction logic for ChangelogPageControl.xaml
/// </summary>
public partial class ChangelogPageControl : UserControl
{
    public ChangelogPageControl(ChangelogPageViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        DataContext = this;
    }

    public ChangelogPageViewModel ViewModel { get; }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        ViewModel.Init();
    }
}