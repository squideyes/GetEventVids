using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace GetEventVids;

public class RunInfosToInlinesConverter : IValueConverter
{
    private static readonly Brush blackBrush = new SolidColorBrush(Colors.Black);
    private static readonly Brush talentBrush = new SolidColorBrush(Colors.IndianRed);
    private static readonly Brush linkBrush = new SolidColorBrush(Colors.Blue);

    public RunInfosToInlinesConverter()
    {
    }

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not Session session)
            return null;

        var runInfos = new List<RunInfo>
            {
                new RunInfo(session.Title!, true),
                new RunInfo(" ("),
                new RunInfo(session.Talent, true, true),
                new RunInfo(") "),
                new RunInfo(session.Synopsis!)
            };

        var inlines = new List<Inline>();

        foreach (var runInfo in runInfos)
        {
            if (runInfo.Uri == null)
            {
                inlines.Add(new Run()
                {
                    Text = runInfo.Text,
                    Foreground = runInfo.Talent ? talentBrush : blackBrush,
                    FontWeight = runInfo.Bold ? FontWeights.Bold : FontWeights.Normal
                });
            }
            else
            {
                var hyperLink = new Hyperlink();

                hyperLink.SetBinding(Hyperlink.CommandProperty, "GoToSessionCommand");

                hyperLink.SetBinding(Hyperlink.CommandParameterProperty, new Binding()
                {
                    ElementName = "SessionsGrid",
                    Path = new PropertyPath("SelectedItem")
                });

                hyperLink.Inlines.Add(new Run()
                {
                    Text = runInfo.Uri.AbsoluteUri,
                    Foreground = linkBrush,
                    FontWeight = runInfo.Bold ? FontWeights.Bold : FontWeights.Normal
                });

                inlines.Add(hyperLink);
            }
        }

        return inlines;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        throw new NotImplementedException();
}
