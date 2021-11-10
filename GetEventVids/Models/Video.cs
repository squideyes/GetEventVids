namespace GetEventVids;

public class Video
{
    public Uri? Uri { get; init; }
    public int Duration { get; init; }
    public long FileSize { get; init; }
    public string? ContentType { get; init; }
}
