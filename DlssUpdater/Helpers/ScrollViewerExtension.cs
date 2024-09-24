using System.Windows.Controls;
using System.Windows.Media;

namespace DlssUpdater.Helpers;

public class ScrollViewerExtension : DependencyObject
{
    // Using a DependencyProperty as the backing store for DisableParentScrollViewer.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty DisableParentScrollViewerProperty =
        DependencyProperty.RegisterAttached("DisableParentScrollViewer", typeof(bool), typeof(ScrollViewerExtension),
            new PropertyMetadata(false, OnDisableParentScrollViewerChanged));

    public static bool GetDisableParentScrollViewer(DependencyObject obj)
    {
        return (bool)obj.GetValue(DisableParentScrollViewerProperty);
    }

    public static void SetDisableParentScrollViewer(DependencyObject obj, bool value)
    {
        obj.SetValue(DisableParentScrollViewerProperty, value);
    }

    private static void OnDisableParentScrollViewerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is FrameworkElement)
            (d as FrameworkElement).Loaded += (_, __) =>
            {
                var scrollViewer = FindAncestor(d as Visual, typeof(ScrollViewer)) as ScrollViewer;
                if (scrollViewer != null && (bool)e.NewValue)
                    scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Disabled;
            };
    }

    public static Visual FindAncestor(Visual startingFrom, Type typeAncestor)
    {
        if (startingFrom != null)
        {
            var parent = VisualTreeHelper.GetParent(startingFrom);

            while (parent != null && !typeAncestor.IsInstanceOfType(parent))
                parent = VisualTreeHelper.GetParent(parent);

            return parent as Visual;
        }

        return null;
    }
}