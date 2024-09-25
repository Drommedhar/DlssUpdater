using DlssUpdater.Defines;
using DlssUpdater.Singletons;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace DlssUpdater.ViewModels.Pages;

public partial class DLSSViewModel : ObservableObject, INavigationAware
{
    private readonly DllUpdater _updater;
    [ObservableProperty] private IEnumerable<OnlinePackage>? _dlss;

    [ObservableProperty] private IEnumerable<OnlinePackage>? _dlssD;

    [ObservableProperty] private IEnumerable<OnlinePackage>? _dlssG;

    private bool _isInitialized;

    public DLSSViewModel(DllUpdater updater)
    {
        _updater = updater;
    }

    public void OnNavigatedTo()
    {
        if (!_isInitialized)
            InitializeViewModel();
    }

    public void OnNavigatedFrom()
    {
    }

    private void InitializeViewModel()
    {
        Dlss = _updater.OnlinePackages[DlssTypes.DllType.Dlss];
        DlssD = _updater.OnlinePackages[DlssTypes.DllType.DlssD];
        DlssG = _updater.OnlinePackages[DlssTypes.DllType.DlssG];

        _isInitialized = true;
    }
}