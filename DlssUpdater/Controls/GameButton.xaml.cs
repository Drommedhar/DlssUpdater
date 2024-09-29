using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using DlssUpdater.Defines;
using DlssUpdater.Singletons;
using DlssUpdater.Views.Pages;

namespace DlssUpdater.Controls;

/// <summary>
///     Interaction logic for GameButton.xaml
/// </summary>
public partial class GameButton : UserControl
{
    public static readonly DependencyProperty GameInfoProperty =
        DependencyProperty.Register("GameInfo", typeof(GameInfo), typeof(GameButton));

    private readonly GameContainer _gameContainer;
    private readonly GamesPage _gamesPage;

    public GameButton()
    {
        InitializeComponent();
        _gameContainer = App.GetService<GameContainer>()!;
        _gamesPage = App.GetService<GamesPage>()!;
        gridAntiCheat.Visibility = Visibility.Hidden;
        selectionBox.Visibility = Visibility.Hidden;
    }

    public GameInfo GameInfo
    {
        get => (GameInfo)GetValue(GameInfoProperty);
        set => SetValue(GameInfoProperty, value);
    }

    private void gameButton_Loaded(object sender, RoutedEventArgs e)
    {
        gridAntiCheat.Visibility = GameInfo.HasAntiCheat ? Visibility.Visible : Visibility.Collapsed;
    }

    private void gameButton_MouseEnter(object sender, MouseEventArgs e)
    {
        if(GameInfo.LibraryType == GameLibrary.LibraryType.Manual)
        {
            btnAction.Visibility = Visibility.Visible;
        }

        selectionBox.Visibility = Visibility.Visible;
    }

    private void gameButton_MouseLeave(object sender, MouseEventArgs e)
    {
        if (GameInfo.LibraryType == GameLibrary.LibraryType.Manual)
        {
            btnAction.Visibility = Visibility.Hidden;
        }

        selectionBox.Visibility = Visibility.Hidden;
    }

    private void btnAction_Click(object sender, RoutedEventArgs e)
    {
        _gameContainer.RemoveGame(GameInfo);
        _gameContainer.SaveGames();
    }

    private void MenuItem_Click(object sender, RoutedEventArgs e)
    {
        GameInfo.IsHidden = !GameInfo.IsHidden;
        _gamesPage.UpdateFilter();
        _gameContainer.SaveGames();
        _gameContainer.DoUpdate();
    }

    private void gameButton_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (FindResource("gameContextMenu") is not ContextMenu cm)
        {
            return;
        }
        cm!.PlacementTarget = sender as Button;
        cm!.IsOpen = true;
    }
}