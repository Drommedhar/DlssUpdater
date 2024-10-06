using DLSSUpdater.Defines.UI;
using System.Windows.Controls;
using System.Windows.Media;
using static DlssUpdater.Defines.DlssTypes;

namespace DLSSUpdater.Controls
{
    /// <summary>
    /// Interaction logic for NavButton.xaml
    /// </summary>
    public partial class NavButton : UserControl
    {
        public static readonly DependencyProperty ButtonForegroundProperty =
        DependencyProperty.Register("ButtonForeground", typeof(SolidColorBrush), typeof(NavButton));

        public SolidColorBrush ButtonForeground
        {
            get => (SolidColorBrush)GetValue(ButtonForegroundProperty);
            set => SetValue(ButtonForegroundProperty, value);
        }

        public static readonly DependencyProperty NotificationColorProperty =
        DependencyProperty.Register("NotificationColor", typeof(Color), typeof(NavButton));

        public Color NotificationColor
        {
            get => (Color)GetValue(NotificationColorProperty);
            set => SetValue(NotificationColorProperty, value);
        }

        public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register("Title", typeof(string), typeof(NavButton));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty NotificationCountProperty =
        DependencyProperty.Register("NotificationCount", typeof(string), typeof(NavButton));

        public string NotificationCount
        {
            get => (string)GetValue(NotificationCountProperty);
            set => SetValue(NotificationCountProperty, value);
        }

        public static readonly DependencyProperty NotificationVisibilityProperty =
        DependencyProperty.Register("NotificationVisibility", typeof(Visibility), typeof(NavButton));

        public Visibility NotificationVisibility
        {
            get => (Visibility)GetValue(NotificationVisibilityProperty);
            set => SetValue(NotificationVisibilityProperty, value);
        }

        public static readonly DependencyProperty NavigationButtonProperty =
        DependencyProperty.Register("NavigationButton", typeof(NavigationButton), typeof(NavButton));

        public NavigationButton NavigationButton
        {
            get => (NavigationButton)GetValue(NavigationButtonProperty);
            set => SetValue(NavigationButtonProperty, value);
        }

        public event EventHandler<RoutedEventArgs>? Click;

        public NavButton()
        {
            InitializeComponent();

            NotificationVisibility = Visibility.Collapsed;
            NotificationColor = Color.FromArgb(255, 125, 125, 125);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Click?.Invoke(sender, e);
        }

        private void ButtonMain_Loaded(object sender, RoutedEventArgs e)
        {
            NavigationButton.Control = this;
        }
    }
}
