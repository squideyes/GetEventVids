namespace GetEventVids;

public class RunInfo
{
    public RunInfo(string text, bool bold = false, bool talent = false)
    {
        Text = text;
        Bold = bold;
        Talent = talent;
    }
    public RunInfo(Uri? uri = null, bool bold = false, bool talent = false)
    {
        Uri = uri;
        Bold = bold;
        Talent = talent;
    }

    public string? Text { get; }
    public Uri? Uri { get; }
    public bool Bold { get; }
    public bool Talent { get; }
}
