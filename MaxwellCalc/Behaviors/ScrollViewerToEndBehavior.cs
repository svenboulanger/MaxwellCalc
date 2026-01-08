using Avalonia;
using Avalonia.Controls;
using System.Collections.Specialized;

namespace MaxwellCalc.Behaviors;

internal class ScrollViewerBehavior
{
    public static readonly AttachedProperty<bool> AutoScrollToEndProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, bool>("AutoScrollToEnd", typeof(ScrollViewerBehavior));

    public static void SetAutoScrollToEnd(AvaloniaObject element, bool value) =>
        element.SetValue(AutoScrollToEndProperty, value);

    public static bool GetAutoScrollToEnd(AvaloniaObject element) =>
        element.GetValue(AutoScrollToEndProperty);

    static ScrollViewerBehavior()
    {
        AutoScrollToEndProperty.Changed.AddClassHandler<ScrollViewer>((scrollViewer, e) =>
        {
            if ((bool)(e?.NewValue ?? false))
            {
                scrollViewer.AttachedToVisualTree += ScrollViewerOnAttachedToVisualTree;
            }
        });
    }

    private static void ScrollViewerOnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer)
            return;
        
        scrollViewer.AttachedToVisualTree -= ScrollViewerOnAttachedToVisualTree;
        
        if (scrollViewer.Content is ItemsControl itemsControl &&
            itemsControl.Items is INotifyCollectionChanged notifyCollection)
        {
            notifyCollection.CollectionChanged += (sender, args) =>
            {
                // Scroll to the end if we were already scrolled to the end before
                if (scrollViewer.Offset.Y >= scrollViewer.Extent.Height - scrollViewer.Viewport.Height - 50.0)
                    scrollViewer.ScrollToEnd();
            };
        }
    }
}
