using DlssUpdater;
using DlssUpdater.GameLibrary;
using DLSSUpdater.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DLSSUpdater.Views.Pages
{
    /// <summary>
    /// Interaction logic for ChangelogPageControl.xaml
    /// </summary>
    public partial class ChangelogPageControl : UserControl
    {
        public ChangelogPageViewModel ViewModel { get; }

        public ChangelogPageControl(ChangelogPageViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            DataContext = this;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Init();
        }
    }
}
