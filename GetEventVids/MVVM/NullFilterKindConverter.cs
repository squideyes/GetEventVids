namespace GetEventVids;

public class NullFilterKindConverter : NullEnumConverter
{
    public NullFilterKindConverter()
        : base(typeof(FilterKind), NullText)
    {
    }

    public const string NullText = "(No Filter)";
}
