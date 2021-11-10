using System.Globalization;
using System.Windows.Data;

namespace GetEventVids;

public abstract class NullEnumConverter : IValueConverter
{
    private readonly Type enumType;
    private readonly string nullText;

    public NullEnumConverter(Type enumType, string nullText)
    {
        this.enumType = enumType ??
            throw new ArgumentNullException(nameof(enumType));

        this.nullText = nullText ??
            throw new ArgumentNullException(nameof(enumType));
    }

    public object Convert(object value,
        Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null)
            return nullText;

        return value;
    }

    public object? ConvertBack(object value, Type targetType,
           object parameter, CultureInfo culture)
    {
        if (value.ToString()!.Equals(nullText))
            return null;

        var rawEnum = Enum.Parse(enumType, value.ToString()!);

        return System.Convert.ChangeType(rawEnum, enumType);
    }
}
