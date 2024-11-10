using System.Windows.Media;
using DLSSUpdater.Controls;

namespace DLSSUpdater.Defines.UI;

public partial class NavigationButton : ObservableObject
{
    private readonly Action _action;
    private readonly bool _allowMultiClick;
    [ObservableProperty] private SolidColorBrush _foreground;
    [ObservableProperty] private NavigationButton _instance;

    [ObservableProperty] private string _title;
    public bool IsClicked;

    public NavigationButton(string title, Action action, bool allowMultiClick)
    {
        Instance = this;
        Title = title;
        _action = action;
        _allowMultiClick = allowMultiClick;
        Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
    }

    public NavButton? Control { get; set; }

    public async void OnClick()
    {
        if (!_allowMultiClick && IsClicked)
        {
            return;
        }

        // TODO: Thats "just a bit" ugly. I should rather change the call chain.
        //       But I will do it like this for now! :D
        while (Control is null)
        {
            await Task.Delay(10);
        }

        IsClicked = true;
        _action();
    }
}