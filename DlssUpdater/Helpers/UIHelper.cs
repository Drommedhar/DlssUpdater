using System.Windows.Media.Animation;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace DlssUpdater.Helpers;

public static class UIHelper
{
    public static async void ToggleControlFade(this FrameworkElement control, int speed, double opacity)
    {
        if ((opacity == 0.0 && control.Visibility == Visibility.Collapsed)
            || (opacity == 1.0 && control.Visibility == Visibility.Visible))
            return;

        if (opacity >= 0.0 && control.Visibility != Visibility.Visible) control.Visibility = Visibility.Visible;

        var storyboard = new Storyboard();
        var duration = new TimeSpan(0, 0, 0, 0, speed);

        var animation = new DoubleAnimation { From = 1.0, To = 0.0, Duration = new Duration(duration) };
        if (control.Opacity == 0.0)
            animation = new DoubleAnimation { From = 0.0, To = 1.0, Duration = new Duration(duration) };

        Storyboard.SetTargetName(animation, control.Name);
        Storyboard.SetTargetProperty(animation, new PropertyPath("Opacity", opacity));
        storyboard.Children.Add(animation);

        storyboard.Begin(control);
        await Task.Delay(speed);
        if (opacity == 0.0)
            control.Visibility = Visibility.Collapsed;
        else
            control.Visibility = Visibility.Visible;
    }

    public static void ShowEx(this ISnackbarService snackbar, string title, string message,
        ControlAppearance controlAppearance)
    {
        snackbar.Show(title, message, controlAppearance, null, TimeSpan.FromSeconds(5));
    }
}