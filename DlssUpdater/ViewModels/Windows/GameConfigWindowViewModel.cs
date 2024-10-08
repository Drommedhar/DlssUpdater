using DlssUpdater.Defines;
using DlssUpdater.Singletons;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DlssUpdater.Defines.DlssTypes;

namespace DLSSUpdater.ViewModels.Windows
{
    public partial class GameConfigWindowViewModel : ObservableObject
    {
        [ObservableProperty]
        private Visibility _editTextVisible = Visibility.Collapsed;
        [ObservableProperty]
        private GameInfo _gameInfo = new("", "", DlssUpdater.GameLibrary.LibraryType.Manual);
        [ObservableProperty]
        private bool _isManualGame = true;
        [ObservableProperty]
        private ObservableCollection<InstalledPackage> _installedVersionsDlss = [];
        [ObservableProperty]
        private ObservableCollection<InstalledPackage> _installedVersionsDlssD = [];
        [ObservableProperty]
        private ObservableCollection<InstalledPackage> _installedVersionsDlssG = [];
        [ObservableProperty]
        private string _dlssItem = "";
        [ObservableProperty]
        private string _dlssDItem = "";
        [ObservableProperty]
        private string _dlssGItem = "";
        [ObservableProperty]
        private bool _dlssEnabled = true;
        [ObservableProperty]
        private bool _dlssDEnabled = true;
        [ObservableProperty]
        private bool _dlssGEnabled = true;
        [ObservableProperty]
        private string _pathText = "";
        [ObservableProperty]
        private bool _saveAsDefault = false;

        private readonly DllUpdater _updater;

        public GameConfigWindowViewModel(DllUpdater updater)
        {
            _updater = updater;

            UpdateUi();
        }

        public void UpdateUi()
        {
            DlssItem = "";
            DlssDItem = "";
            DlssGItem = "";
            SaveAsDefault = false;

            InstalledVersionsDlss = copyPackages(DllType.Dlss);
            InstalledVersionsDlssD = copyPackages(DllType.DlssD);
            InstalledVersionsDlssG = copyPackages(DllType.DlssG);

            PathText = IsManualGame ? "..." : "Can't be set manually";
            DlssEnabled = !string.IsNullOrEmpty(GameInfo.InstalledDlls[DllType.Dlss].Version);
            DlssDEnabled = !string.IsNullOrEmpty(GameInfo.InstalledDlls[DllType.DlssD].Version);
            DlssGEnabled = !string.IsNullOrEmpty(GameInfo.InstalledDlls[DllType.DlssG].Version);

            if(DlssEnabled && _updater.HasDefaultDll(DllType.Dlss, GameInfo))
            {
                InstalledVersionsDlss.Insert(0, new() { Version = DllUpdater.DefaultVersion, VersionDetailed = DllUpdater.DefaultVersion });
            }
            if (DlssDEnabled && _updater.HasDefaultDll(DllType.DlssD, GameInfo))
            {
                InstalledVersionsDlssD.Insert(0, new() { Version = DllUpdater.DefaultVersion, VersionDetailed = DllUpdater.DefaultVersion });
            }
            if (DlssGEnabled && _updater.HasDefaultDll(DllType.DlssG, GameInfo))
            {
                InstalledVersionsDlssG.Insert(0, new() { Version = DllUpdater.DefaultVersion, VersionDetailed = DllUpdater.DefaultVersion });
            }
        }

        private ObservableCollection<InstalledPackage> copyPackages(DllType dllType)
        {
            ObservableCollection<InstalledPackage> ret = [];

            foreach (var p in _updater.InstalledPackages[dllType])
            {
                ret.Add(p);
            }

            return ret;
        }
    }
}
