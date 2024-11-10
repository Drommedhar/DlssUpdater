using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace DLSSUpdater.Defines.UI.Pages;

public interface IContentPage
{
    public UserControl GetPageControl();
    public ObservableCollection<NavigationButton> GetNavigationButtons();
    public HorizontalAlignment GetAlignment();
}