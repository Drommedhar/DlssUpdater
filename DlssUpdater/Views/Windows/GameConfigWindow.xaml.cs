using AdonisUI.Controls;
using DlssUpdater.Defines;
using DlssUpdater.GameLibrary;
using DlssUpdater.Singletons;
using DLSSUpdater.Singletons;
using DLSSUpdater.ViewModels.Windows;
using Microsoft.Win32;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Input;
using static DlssUpdater.Defines.DlssTypes;

namespace DLSSUpdater.Views.Windows
{
    /// <summary>
    /// Interaction logic for GameConfigWindow.xaml
    /// </summary>
    public partial class GameConfigWindow : AdonisWindow
    {
        public GameConfigWindowViewModel ViewModel { get; set; }

        private readonly GameContainer _gameContainer;
        private readonly AsyncFileWatcher _fileWatcher;
        private readonly DllUpdater _updater;
        private readonly bool _newGame;
        private readonly GameInfo? _originalGameInfo;

        public GameConfigWindow(GameConfigWindowViewModel viewModel, GameContainer gameContainer, AsyncFileWatcher watcher, 
                                DllUpdater updater, GameInfo? gameInfo)
        {
            ViewModel = viewModel;
            _newGame = true;
            if(gameInfo is not null)
            {
                watcher.RemoveFile(gameInfo);
                ViewModel.GameInfo = new(gameInfo);
                _originalGameInfo = gameInfo;
                _newGame = false;
            }

            _gameContainer = gameContainer;
            _fileWatcher = watcher;
            _updater = updater;

            SizeChanged += (s, e) => CenterOwner(this);
            InitializeComponent();

            DataContext = this;
            _updater = updater;

            updateUi();
        }

        private void CenterOwner(Window w)
        {
            // TODO: Does not work if maximized
            if (w.Owner != null)
            {
                double top = w.Owner.Top + ((w.Owner.Height - w.ActualHeight) / 2);
                double left = w.Owner.Left + ((w.Owner.Width - w.ActualWidth) / 2);

                w.Top = top < 0 ? 0 : top;
                w.Left = left < 0 ? 0 : left;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Button_MouseEnter(object sender, MouseEventArgs e)
        {
            ViewModel.EditTextVisible = Visibility.Visible;
        }

        private void Button_MouseLeave(object sender, MouseEventArgs e)
        {
            ViewModel.EditTextVisible= Visibility.Collapsed;
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private async void btnApply_Click(object sender, RoutedEventArgs e)
        {
            if (_newGame)
            {
                _gameContainer.Games.Add(ViewModel.GameInfo);
            }
            else
            {
                var index = _gameContainer.Games.IndexOf(_originalGameInfo!);
                if (index != -1)
                {
                    _gameContainer.Games.RemoveAt(index);
                    _gameContainer.Games.Add(ViewModel.GameInfo);
                }
            }

            ViewModel.GameInfo.Update();
            _fileWatcher.AddFile(ViewModel.GameInfo);

            ViewModel.GameInfo.InstalledDlls[DllType.Dlss].Version = ViewModel.DlssItem ?? "";
            ViewModel.GameInfo.InstalledDlls[DllType.DlssD].Version = ViewModel.DlssDItem ?? "";
            ViewModel.GameInfo.InstalledDlls[DllType.DlssG].Version = ViewModel.DlssGItem ?? "";

            var updateResult = _updater.UpdateGameDlls(ViewModel.GameInfo, ViewModel.SaveAsDefault);
            _gameContainer.SaveGames();

            await ViewModel.GameInfo.GatherInstalledVersions();
            _gameContainer.DoUpdate();
            Close();
        }

        private async void btnPath_Click(object sender, RoutedEventArgs e)
        {
            OpenFolderDialog dlg = new()
            {
                Multiselect = false
            };
            if (dlg.ShowDialog() == true)
            {
                ViewModel.GameInfo.GamePath = dlg.FolderName;
                if (string.IsNullOrEmpty(ViewModel.GameInfo.GameName))
                {
                    // Set folder name as name
                    ViewModel.GameInfo.GameName = Path.GetFileName(dlg.FolderName)!;
                }
                await ViewModel.GameInfo.GatherInstalledVersions();
                updateUi();
            }
        }

        private void btnImage_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new();
            dlg.CheckFileExists = true;
            dlg.Multiselect = false;
            dlg.Filter = "";

            var codecs = ImageCodecInfo.GetImageEncoders();
            var sep = string.Empty;

            foreach (var c in codecs.Reverse())
            {
                var codecName = c.CodecName!.Substring(8).Replace("Codec", "Files").Trim();
                dlg.Filter = string.Format("{0}{1}{2} ({3})|{3}", dlg.Filter, sep, codecName, c.FilenameExtension);
                sep = "|";
            }

            if (dlg.ShowDialog() == true)
            {
                ViewModel.GameInfo.SetGameImageUri(dlg.FileName);
                updateUi();
            }
        }

        private void updateUi()
        {
            ViewModel.IsManualGame = ViewModel.GameInfo.LibraryType == LibraryType.Manual;
            ViewModel.UpdateUi();
        }
    }
}
