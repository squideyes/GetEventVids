using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using SquidEyes.Basics;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using System.Windows.Data;

namespace GetEventVids;

public class MainWindowViewModel : ViewModelBase
{
    private bool downloading = false;
    private bool allSelected = false;
    private bool downloadVideos = false;
    private string progress = "";
    private string cancelOrDownload = "";
    private string sessionsCount = "";
    private string filterText = "";
    private string statusPrompt = "";

    private Dictionary<Event, List<Session>> dict;
    private CancellationTokenSource cts;
    private List<RunInfo>? runInfos;
    private List<Session>? sessions;
    private Event selectedEvent;
    private FilterKind? selectedFilterKind;

    public event EventHandler? OnHelp;

    public MainWindowViewModel(Dictionary<Event, List<Session>> sessions)
    {
        dict = sessions;

        Title = "Channel 9 Video Viewer / Downloader v1.0";

        cts = new CancellationTokenSource();

        Events = EnumList.FromAll<Event>();

        selectedEvent = Event.VisualStudio2022;

        SetSession(selectedEvent);

        DownloadVideos = true;

        Downloading = false;

        ShowStandardStatusPrompt();
    }

    private void SetSession(Event @event)
    {
        var fileName = GetFavoritesFileName(true);

        var favorites = GetFavorites(fileName);

        foreach (var session in dict[@event])
        {
            session.OnSelectionChanged += (s, e) =>
            {
                if (allSelected && !e.Value)
                {
                    allSelected = false;

                    RaisePropertyChanged(() => AllSelected);
                }
                else if (!allSelected && dict[@event].All(c => c.Selected))
                {
                    allSelected = true;

                    RaisePropertyChanged(() => AllSelected);
                }

                UpdateUI();
            };

            if (favorites.TryGetValue(session.Code!, out bool isFavorite))
                session.IsFavorite = isFavorite;
        }

        SessionFilterView = (CollectionView)CollectionViewSource.GetDefaultView(dict[@event]);

        SessionFilterView.Filter = OnFilterTriggered;

        SessionFilterView.CurrentChanged += (s, e) => UpdateSessionCount();

        Sessions = dict[@event];

        UpdateUI();

        UpdateSessionCount();
    }

    private void UpdateSessionCount() =>
        SessionsCount = $"{SessionFilterView.Count:N0} Sessions";

    public CollectionView SessionFilterView { get; set; }

    public string Title { get; }

    public List<Event> Events { get; }

    public string SessionsCount
    {
        get => sessionsCount;
        set => Set(ref sessionsCount, value);
    }

    public string StatusPrompt
    {
        get => statusPrompt;
        set => Set(ref statusPrompt, value);
    }

    public List<Session>? Sessions
    {
        get => sessions;
        set => Set(ref sessions, value);
    }

    public bool DownloadVideos
    {
        get => downloadVideos;
        set => Set(ref downloadVideos, value);
    }

    public Event SelectedEvent
    {
        get => selectedEvent;
        set
        {
            Set(ref selectedEvent, value);

            SetSession(selectedEvent);
        }
    }

    public FilterKind? SelectedFilterKind
    {
        get => selectedFilterKind;
        set
        {
            Set(ref selectedFilterKind, value);

            ApplyFilter();
        }
    }

    public bool Downloading
    {
        get => downloading;
        set
        {
            downloading = value;

            CancelOrDownloadLabel = Downloading ? "Cancel" : "Download";
        }
    }

    public bool AllSelected
    {
        get
        {
            return allSelected;
        }
        set
        {
            Set(ref allSelected, value);

            foreach (var session in dict[SelectedEvent])
                session.Selected = value;

            UpdateUI();
        }
    }

    public string CancelOrDownloadLabel
    {
        get => cancelOrDownload;
        set => Set(ref cancelOrDownload, value);
    }

    public string FilterText
    {
        get => filterText;
        set
        {
            Set(ref filterText, value);

            ApplyFilter();
        }
    }

    public List<RunInfo> RunInfos
    {
        get => runInfos;
        set => Set(nameof(RunInfos), ref runInfos!);
    }

    public string Progress
    {
        get => progress;
        set => Set(ref progress, value);
    }

    private void ShowStandardStatusPrompt() =>
        StatusPrompt = "Click the \"Help\" button for full usage details";

    public bool OnFilterTriggered(object item)
    {
        bool ContainsFilterText(Session session)
        {
            return session.Title!.Contains(FilterText, StringComparison.OrdinalIgnoreCase)
                || session.Talent.Contains(FilterText, StringComparison.OrdinalIgnoreCase);
        }

        bool PassesFilter(Session session)
        {
            return (session.Selected && (SelectedFilterKind == FilterKind.Selected))
                || (session.IsFavorite && (SelectedFilterKind == FilterKind.Favorites));
        }

        if (item is Session session)
        {
            return (filterText == null || ContainsFilterText(session))
                && (SelectedFilterKind == null || PassesFilter(session));
        }

        return true;
    }

    public void ApplyFilter() =>
        CollectionViewSource.GetDefaultView(dict[SelectedEvent]).Refresh();

    private void UpdateUI()
    {
        RaisePropertyChanged(() => Downloading);
        RaisePropertyChanged(() => CancelOrDownloadLabel);
        RaisePropertyChanged(() => CancelOrDownloadCommand);
    }

    private async Task DownloadFilesAsync(List<Job> jobs)
    {
        const string SUCCESS = "Success";
        const string WARNING = "Warning";

        static string Plural(int count) => count >= 2 ? "s" : "";

        var progressLock = new object();

        long totalBytesRead = 0;
        int downloaded = 0;

        StatusPrompt = $"Downloading {jobs.Count} video file(s); click the \"Cancel\" button to cancel";

        var badJobs = new ConcurrentBag<Job>();

        var downloader = new ActionBlock<Job>(
            async job =>
            {
                job.OnProgress += (s, e) =>
                {
                    lock (progressLock)
                    {
                        totalBytesRead += e.BytesRead;

                        if (e.Finished)
                            downloaded++;

                        Progress = $"{downloaded:N0} of {jobs.Count:N0} ({totalBytesRead:N0} bytes)";
                    }
                };

                if (await job.DownloadAndSaveAsync(cts.Token))
                {
                    if (!cts.Token.IsCancellationRequested)
                    {
                        job.Session.HasVideo = true;
                        job.Session.Selected = false;
                    }
                }
                else
                {
                    badJobs.Add(job);
                    job.Session.Selected = false;
                };
            },
            new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount,
                CancellationToken = cts.Token
            });

        jobs.ForEach(job => downloader.Post(job));

        downloader.Complete();

        try
        {
            await downloader.Completion;

            var sb = new StringBuilder();

            var title = SUCCESS;
            var image = MessageBoxImage.Information;

            if (jobs.Count == 1)
            {
                var fileName = jobs[0].Session.GetCleanFileName();

                if (!badJobs.Any())
                {
                    sb.Append($"The \"");
                    sb.Append(fileName);
                    sb.Append("\" video was downloaded to the \"");
                    sb.Append(MiscHelpers.GetFolder(SelectedEvent));
                    sb.Append("\" folder.  Click on the \"Play\" button to play the video or navigate to the folder and play it from there.");
                }
                else
                {
                    sb.Append($"The \"");
                    sb.Append(fileName);
                    sb.Append("\" video has no downloadable content!");

                    title = WARNING;
                    image = MessageBoxImage.Warning;
                }
            }
            else
            {
                var goodCount = jobs.Count - badJobs.Count;

                sb.Append(goodCount.ToString("N0"));
                sb.Append(" video file");
                sb.Append(Plural(goodCount));
                sb.Append(' ');
                sb.Append(goodCount >= 2 ? "were" : "was");
                sb.Append(" successfully downloaded to the \"");
                sb.Append(MiscHelpers.GetFolder(SelectedEvent));
                sb.Append("\" folder");

                if (!badJobs.Any())
                {
                    sb.Append('.');
                }
                else
                {
                    title = "Warning";
                    image = MessageBoxImage.Warning;

                    sb.Append(", but ");
                    sb.Append(badJobs.Count.ToString("N0"));
                    sb.Append(" session");
                    sb.Append(Plural(badJobs.Count));
                    sb.Append(" had no downloadable content!");
                }
            }

            MessageBox.Show(sb.ToString(), title, MessageBoxButton.OK, image);
        }
        catch (TaskCanceledException)
        {
        }
        catch (Exception error)
        {
            UpdateUI();

            MessageBox.Show(error.Message,
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            Downloading = false;

            UpdateUI();

            Progress = "";

            cts = new CancellationTokenSource();

            ShowStandardStatusPrompt();
        }
    }

    private static Dictionary<string, bool> GetFavorites(string fileName)
    {
        var favorites = new Dictionary<string, bool>();

        try
        {
            if (File.Exists(fileName))
            {
                var json = File.ReadAllText(fileName);

                favorites = JsonSerializer.Deserialize<
                    Dictionary<string, bool>>(json)!;
            }
        }
        catch
        {
            favorites = new Dictionary<string, bool>();
        }

        return favorites;
    }

    private static string GetFavoritesFileName(bool ensurePath = false)
    {
        var localPath = new AppInfo(typeof(MainWindowViewModel).Assembly).GetLocalAppDataPath();

        if (ensurePath && !Directory.Exists(localPath))
            Directory.CreateDirectory(localPath);

        return Path.Combine(localPath, "Favorites.json");
    }

    public void SaveFavorites()
    {
        try
        {
            var favorites = dict[SelectedEvent]!.Where(s => s.IsFavorite)
                .ToDictionary(s => s.Code!, s => s.IsFavorite);

            var json = JsonSerializer.Serialize(favorites);

            File.WriteAllText(GetFavoritesFileName(), json);
        }
        catch (Exception error)
        {
            MessageBox.Show($"FATAL ERROR: " + error.Message,
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public static RelayCommand<Session> StreamVideoCommand =>
        new(s => Shell.Execute(s.StreamUri!.AbsoluteUri));

    public static RelayCommand<Session> GoToSessionCommand =>
        new(s => Shell.Execute(s.SessionUri!.AbsoluteUri));

    public RelayCommand CancelOrDownloadCommand =>
        new(async () =>
        {
            static List<Job> GetJobs(List<Session> sessions)
            {
                var jobs = new List<Job>();

                foreach (var session in sessions)
                    jobs.Add(new Job(session));

                return jobs;
            }

            if (Downloading)
            {
                Downloading = false;

                UpdateUI();

                cts.Cancel();
            }
            else
            {
                Downloading = true;

                UpdateUI();

                var sessions = this.dict[SelectedEvent]
                    .Where(s => s.Selected).ToList();

                if (sessions.Count > 0)
                {
                    var jobs = new List<Job>();

                    if (DownloadVideos)
                        jobs.AddRange(GetJobs(sessions));

                    if (jobs.Count > 0)
                        await DownloadFilesAsync(jobs);
                }
            }
        },
        () => dict[SelectedEvent].Any(s => s.Selected) || Downloading);

    public static RelayCommand<Session> PlayVideoCommand =>
        new(session =>
        {
            if (!Shell.Execute(session.GetFullPath(MiscHelpers.GetFolder(session.Event))))
                session.HasVideo = false;
        });

    public RelayCommand ClearFilterTextCommand => new(() => FilterText = "");

    public RelayCommand HelpCommand => new(() => OnHelp?.Invoke(this, EventArgs.Empty));
}
