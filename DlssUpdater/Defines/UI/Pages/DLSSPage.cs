using DlssUpdater.Singletons;
using DlssUpdater;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static DlssUpdater.Defines.DlssTypes;
using DLSSUpdater.Views.Pages;
using System.Runtime.CompilerServices;
using System.Windows.Documents;
using DLSSUpdater.ViewModels.Windows;

namespace DLSSUpdater.Defines.UI.Pages
{
    public partial class DLSSPage : ObservableObject, IContentPage
    {
        [ObservableProperty]
        private DlssPageControl _pageControl;

        private readonly Dictionary<NavigationButton, DllType> _navigationButtonsByType = [];
        private readonly ObservableCollection<NavigationButton> _navigationButtons = [];
        private readonly DllUpdater _dllUpdater;
        private readonly Settings _settings;
        public MainWindowViewModel? MainWindowViewModel;

        public DLSSPage(DllUpdater updater, Settings settings)
        {
            _dllUpdater = updater;
            _settings = settings;
            _dllUpdater.DlssFilesChanged += _dllUpdater_DlssFilesChanged;

            foreach (DllType dllType in Enum.GetValues(typeof(DllType)))
            {
                NavigationButton navButton = new(GetUIName(dllType), () => { ShowSubPage(dllType); }, false);
                _navigationButtonsByType.Add(navButton, dllType);
                _navigationButtons.Add(navButton);
            }

            PageControl = App.GetService<DlssPageControl>()!;
        }

        private void _dllUpdater_DlssFilesChanged(object? sender, EventArgs e)
        {
            UpdateNotificationInfo();
        }

        public UserControl GetPageControl()
        {
            return PageControl;
        }

        ObservableCollection<NavigationButton> IContentPage.GetNavigationButtons()
        {
            foreach (var button in _navigationButtons)
            {
                button.Control = null;
            }

            return _navigationButtons;
        }

        public HorizontalAlignment GetAlignment()
        {
            return HorizontalAlignment.Left;
        }

        public void UpdateNotificationInfo()
        {
            int count = 0;
            foreach (var button in _navigationButtons)
            {
                var dllType = _navigationButtonsByType[button];
                var hasUpdate = _dllUpdater.IsNewerVersionAvailable(dllType);
                if(hasUpdate)
                {
                    count++;
                }

                var navigationButton = button.Control;
                if (navigationButton is null)
                {
                    continue;
                }

                navigationButton.NotificationCount = hasUpdate ? "" : "0";
                if (_settings.ShowNotifications)
                {
                    navigationButton.NotificationVisibility = hasUpdate ? Visibility.Visible : Visibility.Collapsed;
                }
                else
                {
                    navigationButton.NotificationVisibility = Visibility.Collapsed;
                }

            }

            MainWindowViewModel?.UpdateDllsNotification(count);
        }

        private void ShowSubPage(DllType dllType)
        {
            PageControl.SetDllType(dllType);

            UpdateNotificationInfo();
        }
    }
}
