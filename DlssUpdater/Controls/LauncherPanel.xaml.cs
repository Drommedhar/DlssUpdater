using DlssUpdater.Defines;
using DlssUpdater.Singletons;
using DLSSUpdater.Defines;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DlssUpdater.Controls
{
    /// <summary>
    /// Interaction logic for LauncherPanel.xaml
    /// </summary>
    public partial class LauncherPanel : UserControl
    {
        public static readonly DependencyProperty LibraryConfigProperty =
        DependencyProperty.Register("LibraryConfig", typeof(LibraryConfig), typeof(LauncherPanel));

        public LibraryConfig LibraryConfig
        {
            get => (LibraryConfig)GetValue(LibraryConfigProperty);
            set => SetValue(LibraryConfigProperty, value);
        }

        private readonly Settings _settings;
        private readonly GameContainer _gameContainer;
        private readonly NLog.Logger _logger;

        public LauncherPanel()
        {
            InitializeComponent();

            _settings = App.GetService<Settings>()!;
            _gameContainer = App.GetService<GameContainer>()!;
            _logger = App.GetService<NLog.Logger>()!;

            GridExpand.Visibility = Visibility.Visible;
        }

        private async void ToggleSwitch_Click(object sender, RoutedEventArgs e)
        {
            _settings.Save();
            _logger.Debug($"Switched library '{LibraryConfig.LibraryName}' to {LibraryConfig.IsChecked}");
            _gameContainer.UpdateLibraries();
            await _gameContainer.ReloadLibraryGames(LibraryConfig.LibraryType);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var libRef = _gameContainer.Libraries.FirstOrDefault(l => l.GetLibraryType() == LibraryConfig.LibraryType);
            libRef?.GetInstallationDirectory();
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dlg = new()
            {
                Multiselect = false
            };
            if (dlg.ShowDialog() == true)
            {
                LibraryConfig.InstallPath = dlg.FolderName;
                _settings.Save();
                _gameContainer.UpdateLibraries();
                await _gameContainer.LoadGamesAsync();
            }
        }
    }
}
