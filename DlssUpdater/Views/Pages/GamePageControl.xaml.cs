using DlssUpdater.Defines;
using DLSSUpdater.ViewModels.Pages;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace DLSSUpdater.Views.Pages
{
    /// <summary>
    /// Interaction logic for GamePageControl.xaml
    /// </summary>
    public partial class GamePageControl : UserControl
    {
        public enum Filter
        {
            All,
            Hidden
        }

        public GamePageViewModel ViewModel { get; }

        private Filter _filter = Filter.All;

        public GamePageControl(GamePageViewModel viewModel)
        {
            ViewModel = viewModel;

            InitializeComponent();

            DataContext = this;
            ViewModel.FilterVisible = Visibility.Collapsed;
            FilterImage.Opacity = 0.5;
        }

        public void UpdateFilter()
        {
            ICollectionView collectionView = CollectionViewSource.GetDefaultView(listItems.ItemsSource);
            collectionView.Filter = FilterGames;
        }

        private bool FilterGames(object game)
        {
            if (game is GameInfo realGame)
            {
                if(!string.IsNullOrEmpty(ViewModel.SearchText) && !realGame.GameName.ToLower().Contains(ViewModel.SearchText.ToLower()))
                {
                    return false;
                }

                if (ViewModel.FilterActive)
                {
                    bool libActive = realGame.LibraryType switch
                    {
                        DlssUpdater.GameLibrary.LibraryType.Manual => ViewModel.FilterLibManual,
                        DlssUpdater.GameLibrary.LibraryType.Steam => ViewModel.FilterLibSteam,
                        DlssUpdater.GameLibrary.LibraryType.Ubisoft => ViewModel.FilterLibUbisoft,
                        DlssUpdater.GameLibrary.LibraryType.EpicGames => ViewModel.FilterLibEpic,
                        DlssUpdater.GameLibrary.LibraryType.GOG => ViewModel.FilterLibGOG,
                        DlssUpdater.GameLibrary.LibraryType.Xbox => ViewModel.FilterLibXbox,
                        _ => true,
                    };
                    if (!libActive)
                    {
                        return false;
                    }
                }

                if(ViewModel.FilterVisACOnly && realGame.AntiCheat != DlssUpdater.Singletons.AntiCheatChecker.AntiCheatProvider.None)
                {
                    return false;
                }
                if(ViewModel.FilterVisUpdateOnly && realGame.UpdateVisible != Visibility.Visible)
                {
                    return false;
                }

                if (_filter == Filter.Hidden)
                {
                    return realGame.IsHidden;
                }
                else
                {
                    return !realGame.IsHidden;
                }
            }

            return false;
        }

        private void DllsPage_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateFilter();
        }

        private void btnFilter_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.FilterVisible == Visibility.Visible)
            {
                ViewModel.FilterVisible = Visibility.Collapsed;
            }
            else
            {
                ViewModel.FilterVisible = Visibility.Visible;
            }

            UpdateFilterActiveState();
        }

        private void UpdateFilterActiveState()
        {
            FilterImage.Opacity = ViewModel.FilterActive ? 0.9 : 0.5;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateFilter();
        }

        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FilterActive = false;
            UpdateFilterActiveState();

            _filter = Filter.All;
            ViewModel.FilterVisHiddenOnly = false;
            ViewModel.FilterVisACOnly = false;
            ViewModel.FilterVisUpdateOnly = false;

            ViewModel.FilterVisible = Visibility.Collapsed;
            ViewModel.SetDefaultFilterValues();
            UpdateFilter();
        }

        private void btnApply_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.FilterActive = true;
            UpdateFilterActiveState();

            _filter = ViewModel.FilterVisHiddenOnly ? Filter.Hidden : Filter.All;

            ViewModel.FilterVisible = Visibility.Collapsed;
            UpdateFilter();
        }
    }
}
