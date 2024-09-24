using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DlssUpdater.ViewModels.Windows;

public class SplashscreenViewModel : INotifyPropertyChanged
{
    private string _infoText = "";

    public string InfoText
    {
        get => _infoText;
        set
        {
            _infoText = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    // Create the OnPropertyChanged method to raise the event
    // The calling member's name will be used as the parameter.
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}