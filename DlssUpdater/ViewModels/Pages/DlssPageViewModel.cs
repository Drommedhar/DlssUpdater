using DlssUpdater.Defines;
using DlssUpdater.Singletons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static DlssUpdater.Defines.DlssTypes;

namespace DLSSUpdater.ViewModels.Pages
{
    public partial class DlssPageViewModel : ObservableObject
    {
        private DllType? _dllType;

        private readonly DllUpdater _dllUpdater;

        [ObservableProperty] private IEnumerable<OnlinePackage>? _versions;

        public DlssPageViewModel(DllUpdater updater)
        {
            _dllUpdater = updater;
        }

        public void SetDllType(DllType dllType)
        {
            // Only if a different value is set we update the view
            if (dllType == _dllType)
            {
                return;
            }

            _dllType = dllType;
            Reload();
        }

        private void Reload()
        {
            if(_dllType is null)
            {
                return;
            }

            Versions = _dllUpdater.OnlinePackages[_dllType ?? DllType.Dlss];
        }
    }
}
