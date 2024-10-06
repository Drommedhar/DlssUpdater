using DLSSUpdater.Controls;
using System.Windows.Controls;
using System.Windows.Media;

namespace DLSSUpdater.Defines.UI
{
    public partial class NavigationButton : ObservableObject
    {
        public NavButton? Control { get; set; }
        public bool IsClicked = false;

        [ObservableProperty] private string _title;
        [ObservableProperty] private SolidColorBrush _foreground;
        [ObservableProperty] private NavigationButton _instance;
        private readonly Action _action;
        private bool _allowMultiClick;

        public NavigationButton(string title, Action action, bool allowMultiClick) 
        { 
            Instance = this;
            Title = title;
            _action = action;
            _allowMultiClick = allowMultiClick;
            Foreground = new SolidColorBrush(Color.FromRgb(255, 255, 255));
        }

        public async void OnClick()
        {
            if(!_allowMultiClick && IsClicked)
            {
                return;
            }

            // TODO: Thats "just a bit" ugly. I should rather change the call chain.
            //       But I will do it like this for now! :D
            while(Control is null)
            {
                await Task.Delay(10);
            }

            IsClicked = true;
            _action();
        }
    }
}
