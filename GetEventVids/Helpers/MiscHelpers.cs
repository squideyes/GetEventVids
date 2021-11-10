using NodaTime;
using System.ComponentModel;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static System.Environment;

namespace GetEventVids;

internal static class MiscHelpers
{
    private static readonly MD5 md5 = MD5.Create();

    public static R Funcify<T, R>(this T value, Func<T, R> getResult) => getResult(value);

    public static string GetFolder(Event @event) => Path.Combine(
        GetFolderPath(SpecialFolder.MyDocuments), nameof(GetEventVids), @event.ToString());

    public static string GetDescription(this Enum value)
    {
        var fi = value.GetType().GetField(value.ToString())!;

        if (fi.GetCustomAttributes(typeof(DescriptionAttribute), false)
            is DescriptionAttribute[] attributes && attributes.Any())
        {
            return attributes.First().Description;
        }

        return value.ToString();
    }

    private static readonly DateTimeZone pacificZone =
        DateTimeZoneProviders.Tzdb.GetZoneOrNull("US/Pacific")!;

    public static DateTime ToPacificFromUtc(this DateTime value)
    {
        if (value.Kind != DateTimeKind.Utc)
            throw new ArgumentOutOfRangeException(nameof(value));

        return Instant.FromDateTimeUtc(value)
            .InZone(pacificZone).ToDateTimeUnspecified();
    }

    public static List<string> ToLines(this string value)
    {
        var reader = new StringReader(value);

        var lines = new List<string>();

        string? line;

        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();

            if (string.IsNullOrWhiteSpace(line))
                continue;

            lines.Add(line);
        }

        return lines;
    }

    public static string ToSingleLine(this string value)
    {
        var lines = value.ToLines();

        if (lines.Count == 0)
            return string.Empty;

        return string.Join("; ", lines);
    }

    public static string ToMd5Hash(this string value) =>
        Convert.ToBase64String(md5.ComputeHash(Encoding.UTF8.GetBytes(value)));
}
