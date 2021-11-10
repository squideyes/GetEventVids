using SquidEyes.Basics;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks.Dataflow;
using System.Windows;

namespace GetEventVids;

public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

        var dialog = new LoadingDialog();

        dialog.Show();

        var sessions = new Dictionary<Event, List<Session>>();

        Task.Run(async () =>
            {
                (await GetSessionsAsync())
                    .ForEach(s => sessions.Add(s.Key, s.Value));
            })
            .ConfigureAwait(true)
            .GetAwaiter()
            .OnCompleted(() =>
            {
                dialog.Close();

                var vm = new MainWindowViewModel(sessions);

                var window = new MainWindow { DataContext = vm };

                window.Closed += (s, e) =>
                {
                    vm.SaveFavorites();

                    Current.Shutdown();
                };

                vm.OnHelp += (s, e) =>
                {
                    var vm = new HelpDialogViewModel();

                    var dialog = new HelpDialog
                    {
                        DataContext = vm,
                        Owner = window
                    };

                    vm.OnClose += (s, e) => dialog.Close();

                    dialog.ShowDialog();
                };

                window.Show();
            });
    }

    private static async Task<Dictionary<Event, List<Session>>> GetSessionsAsync()
    {
        var dict = new ConcurrentDictionary<Event, List<Session>>();

        var fetcher = new ActionBlock<Event>(
            async @event =>
            {
                var sessions = await RssClient.GetSessionsAsync(@event);

                dict.AddOrUpdate(@event, sessions, (e, s) => sessions);

                var folder = MiscHelpers.GetFolder(@event);

                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileNames = new HashSet<string>(Directory.GetFiles(folder));

                foreach (var session in sessions)
                {
                    if (fileNames.Contains(session.GetFullPath(folder)))
                        session.HasVideo = true;
                }
            },
            new ExecutionDataflowBlockOptions()
            {
                MaxDegreeOfParallelism = Math.Min(
                    Environment.ProcessorCount, Enum.GetValues(typeof(Event)).Length)
            });

        EnumList.FromAll<Event>().ForEach(e => fetcher.Post(e));

        fetcher.Complete();

        await fetcher.Completion;

        return dict.ToDictionary(e => e.Key, e => e.Value);
    }
}
