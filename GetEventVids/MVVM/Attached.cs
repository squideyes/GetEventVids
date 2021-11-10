using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace GetEventVids;

public class Attached
{
    public static IEnumerable<Inline> GetInlines(DependencyObject d) =>
        (IEnumerable<Inline>)d.GetValue(InlinesProperty);

    public static void SetInlines(DependencyObject d, IEnumerable<Inline> value) =>
        d.SetValue(InlinesProperty, value);

    public static readonly DependencyProperty InlinesProperty =
        DependencyProperty.RegisterAttached(
            "Inlines", typeof(IEnumerable<Inline>), typeof(Attached),
            new FrameworkPropertyMetadata(OnInlinesPropertyChanged));

    private static void OnInlinesPropertyChanged(
        DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock textBlock)
            return;

        var inlinesCollection = textBlock.Inlines;

        inlinesCollection.Clear();

        if (e.NewValue != null)
            inlinesCollection.AddRange((IEnumerable<Inline>)e.NewValue);
    }
}
