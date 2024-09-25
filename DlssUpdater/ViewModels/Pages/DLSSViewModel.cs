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

    [ObservableProperty]
    private Visibility _dlssUpdate;
    [ObservableProperty]
    private Visibility _dlssDUpdate;
    [ObservableProperty]
    private Visibility _dlssGUpdate;

    private bool _isInitialized;

    public DLSSViewModel(DllUpdater updater)
    {
        _updater = updater;
        _updater.DlssFilesChanged += _updater_DlssFilesChanged;
    }

    private void _updater_DlssFilesChanged(object? sender, EventArgs e)
    {
        DlssUpdate = _updater.IsNewerVersionAvailable(DlssTypes.DllType.Dlss) ? Visibility.Visible : Visibility.Hidden;
        DlssDUpdate = _updater.IsNewerVersionAvailable(DlssTypes.DllType.DlssD) ? Visibility.Visible : Visibility.Hidden;
        DlssGUpdate = _updater.IsNewerVersionAvailable(DlssTypes.DllType.DlssG) ? Visibility.Visible : Visibility.Hidden;
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

        _updater_DlssFilesChanged(null, EventArgs.Empty);
        _isInitialized = true;
    }
}