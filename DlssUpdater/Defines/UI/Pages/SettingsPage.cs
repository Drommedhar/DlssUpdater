using System.Collections.ObjectModel;
using System.Windows.Controls;
using DlssUpdater;
using DLSSUpdater.ViewModels.Pages;
using DLSSUpdater.ViewModels.Windows;
using DLSSUpdater.Views.Pages;

namespace DLSSUpdater.Defines.UI.Pages;

public partial class SettingsPage : ObservableObject, IContentPage
{
    private readonly SettingsCommonPageViewModel _viewModel;

    [ObservableProperty] private UserControl _pageControl;

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
            new NavigationButton("Common", () =>
            {
                PageControl = new SettingsCommonPageControl(_viewModel);
                App.GetService<MainWindowViewModel>()!.ReinitPageControl(this);
            }, false)
        ];
    }

    public HorizontalAlignment GetAlignment()
    {
        return HorizontalAlignment.Left;
    }
}