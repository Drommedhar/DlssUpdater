using DlssUpdater;
using DLSSUpdater.ViewModels.Pages;
using DLSSUpdater.ViewModels.Windows;
using DLSSUpdater.Views.Pages;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DLSSUpdater.Defines.UI.Pages
{
    public partial class SettingsPage : ObservableObject, IContentPage
    {
        [ObservableProperty]
        private UserControl _pageControl;

        private readonly SettingsCommonPageViewModel _viewModel;

        public SettingsPage(SettingsCommonPageViewModel viewModel)
        {
            PageControl = new UserControl();
            _viewModel = viewModel;
        }

        public UserControl GetPageControl()
        {
            return PageControl;
        }

        ObservableCollection<NavigationButton> IContentPage.GetNavigationButtons()
        {
            return
            [
                new("Common", () => { PageControl = new SettingsCommonPageControl(_viewModel); App.GetService<MainWindowViewModel>()!.ReinitPageControl(this); }, false),
            ];
        }

        public HorizontalAlignment GetAlignment()
        {
            return HorizontalAlignment.Left;
        }
    }
}
