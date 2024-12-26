using System.Collections.ObjectModel;
using System.IO;
using NLog;

namespace DLSSUpdater.ViewModels.Pages;

public partial class AboutPageViewModel : ObservableObject
{
    private readonly Logger _logger;

    public AboutPageViewModel(Logger logger)
    {
        _logger = logger;
    }

    public async void Init()
    {
        
    }
}