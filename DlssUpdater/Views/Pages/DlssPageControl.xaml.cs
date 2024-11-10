using System.Windows.Controls;
using DLSSUpdater.ViewModels.Pages;
using static DlssUpdater.Defines.DlssTypes;

namespace DLSSUpdater.Views.Pages;

/// <summary>
///     Interaction logic for DlssPageControl.xaml
/// </summary>
public partial class DlssPageControl : UserControl
{
    public DlssPageControl(DlssPageViewModel viewModel)
    {
        ViewModel = viewModel;

        InitializeComponent();

        DataContext = this;
    }

    public DlssPageViewModel ViewModel { get; }

    public void SetDllType(DllType dllType)
    {
        ViewModel.SetDllType(dllType);
        scrollGrid.ScrollToTop();
    }
}