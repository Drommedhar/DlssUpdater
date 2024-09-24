using DlssUpdater.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace DlssUpdater.Views.Pages;

public partial class DLSSPage : INavigableView<DLSSViewModel>
{
    public DLSSPage(DLSSViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();
    }

    public DLSSViewModel ViewModel { get; }
}