using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Xml.Linq;
using NLog;

namespace DLSSUpdater.ViewModels.Pages;

public partial class AboutPageViewModel : ObservableObject
{
    private readonly Logger _logger;

    [ObservableProperty]
    private string _assemblyVersion = string.Empty;

    public AboutPageViewModel(Logger logger)
    {
        _logger = logger;
    }

    public async void Init()
    {
        AssemblyVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();
    }
}