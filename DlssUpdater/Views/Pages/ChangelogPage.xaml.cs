using DlssUpdater.ViewModels.Pages;
using Wpf.Ui.Controls;

namespace DlssUpdater.Views.Pages;

public partial class ChangelogPage : INavigableView<ChangelogViewModel>
{
    public ChangelogPage(ChangelogViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;

        InitializeComponent();

        viewLog.Markdown = System.IO.File.ReadAllText("changelog.md");
    }

    public ChangelogViewModel ViewModel { get; }
}