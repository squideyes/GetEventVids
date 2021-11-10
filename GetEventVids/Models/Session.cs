using GalaSoft.MvvmLight;
using System.IO;

namespace GetEventVids;

public class Session : ObservableObject
{
    private bool selected = false;
    private bool hasVideo = false;
    private bool isFavorite = false;

    public event EventHandler? OnIsFavoriteChanged;
    public event EventHandler<SelectionChangedArgs>? OnSelectionChanged;

    public bool IsFavorite
    {
        get => isFavorite;
        set
        {
            Set(ref isFavorite, value);

            OnIsFavoriteChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public bool Selected
    {
        get
        {
            return selected;
        }
        set
        {
            Set(ref selected, value);

            OnSelectionChanged?.Invoke(this, new SelectionChangedArgs(value));
        }
    }

    public Event Event { get; init; }
    public Guid SessionId { get; init; }
    public string? Code { get; init; }
    public DateTime? PubDate { get; init; }
    public Uri? SessionUri { get; init; }
    public Uri? StreamUri { get; init; }
    public Uri? VideoUri { get; init; }
    public TimeSpan Duration { get; init; }
    public string? Title { get; init; }
    public string? Synopsis { get; init; }
    public List<Speaker>? Speakers { get; init; }

    public bool HasVideo
    {
        get => hasVideo;
        set => Set(ref hasVideo, value);
    }

    public bool CanFetchVideo => !HasVideo;

    public string Talent
    {
        get
        {
            if (Speakers != null && Speakers.Any())
                return string.Join(",", Speakers!.Select(s => s.DisplayName));
            else
                return "";
        }
    }

    public string GetCleanFileName()
    {
        return Path.GetInvalidFileNameChars().Aggregate(Title!,
            (t, c) => t.Replace(c.ToString(), " ")).Trim() + ".mp4";
    }

    public string GetFullPath(string folder) =>
        Path.Combine(folder, GetCleanFileName());

    public override string ToString() => Title!;
}
