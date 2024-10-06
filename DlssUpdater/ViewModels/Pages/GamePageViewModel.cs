using DlssUpdater.Defines;
using DlssUpdater.Helpers;
using DlssUpdater.Singletons;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLSSUpdater.ViewModels.Pages
{
    public partial class GamePageViewModel : ObservableObject
    {
        [ObservableProperty]
        private SortableObservableCollection<GameInfo> _games;
        [ObservableProperty]
        private string _searchText = "";
        [ObservableProperty]
        private Visibility _filterVisible = Visibility.Collapsed;
        [ObservableProperty]
        private bool _filterActive = false;
        [ObservableProperty]
        private bool _filterLibSteam = true;
        [ObservableProperty]
        private bool _filterLibEpic = true;
        [ObservableProperty]
        private bool _filterLibGOG = true;
        [ObservableProperty]
        private bool _filterLibUbisoft = true;
        [ObservableProperty]
        private bool _filterLibXbox = true;
        [ObservableProperty]
        private bool _filterLibManual = true;
        [ObservableProperty]
        private bool _filterVisHiddenOnly = false;
        [ObservableProperty]
        private bool _filterVisACOnly = false;
        [ObservableProperty]
        private bool _filterVisUpdateOnly = false;

        private readonly GameContainer _container;

        public GamePageViewModel(GameContainer container)
        {
            _container = container;
            _container.GamesChanged += _container_GamesChanged;
            Games = container.Games;
            SetDefaultFilterValues();
        }

        public void SetDefaultFilterValues()
        {
            FilterActive = false;
            FilterLibSteam = true;
            FilterLibEpic = true;
            FilterLibGOG = true;
            FilterLibUbisoft = true;
            FilterLibXbox = true;
            FilterLibManual = true;
            FilterVisHiddenOnly = false;
        }

        private void _container_GamesChanged(object? sender, EventArgs e)
        {
            Games = _container.Games;
        }
    }
}
