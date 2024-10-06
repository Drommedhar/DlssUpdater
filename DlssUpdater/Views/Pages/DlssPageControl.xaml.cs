using DlssUpdater.Defines;
using DLSSUpdater.Controls;
using DLSSUpdater.ViewModels.Pages;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media;
using static DlssUpdater.Defines.DlssTypes;

namespace DLSSUpdater.Views.Pages
{
    /// <summary>
    /// Interaction logic for DlssPageControl.xaml
    /// </summary>
    public partial class DlssPageControl : UserControl
    {
        public DlssPageViewModel ViewModel { get; }

        public DlssPageControl(DlssPageViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            DataContext = this;
        }

        public void SetDllType(DllType dllType)
        {
            ViewModel.SetDllType(dllType);
            scrollGrid.ScrollToTop();
        }
    }
}
