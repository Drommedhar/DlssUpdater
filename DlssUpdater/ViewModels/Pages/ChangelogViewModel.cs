using DlssUpdater.Defines;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons;
using DlssUpdater.Singletons.AntiCheatChecker;
using Microsoft.VisualBasic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace DlssUpdater.ViewModels.Pages;

public partial class ChangelogViewModel : ObservableObject, INavigationAware
{
    private bool _isInitialized = false;
    private Settings _settings;

    public ChangelogViewModel(Settings settings)
    {
        _settings = settings;
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
        _isInitialized = true;
    }
}