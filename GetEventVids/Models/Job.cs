using System.IO;
using System.Net.Http;

namespace GetEventVids;

internal class Job
{
    private const int BUFFER_SIZE = 1024 * 1024 * 4;

    private static readonly HttpClient client = new();

    public Job(Session session)
    {
        Session = session;
    }

    public Session Session { get; }

    public event EventHandler<ProgressArgs>? OnProgress;

    public async Task<bool> DownloadAndSaveAsync(CancellationToken cancellationToken)
    {
        var folder = MiscHelpers.GetFolder(Session.Event);

        var startedOn = DateTime.UtcNow;

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);

        var fullPath = Session.GetFullPath(folder);

        var buffer = new byte[BUFFER_SIZE];

        var response = await client.GetAsync(Session.VideoUri,
            HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (!response.IsSuccessStatusCode)
            return false;

        if (cancellationToken.IsCancellationRequested)
            return false;

        var target = new MemoryStream();

        var fileSize = response.Content.Headers.ContentLength!.Value;

        if (fileSize == 22)
        {
            OnProgress?.Invoke(this, new ProgressArgs(22, true));

            return false;
        }

        using (var source = await response.Content.ReadAsStreamAsync(cancellationToken))
        {
            int bytesRead;

            do
            {
                if (cancellationToken.IsCancellationRequested)
                    return false;

                bytesRead = await source.ReadAsync(buffer, cancellationToken);

                if (bytesRead > 0)
                {
                    target.Write(buffer, 0, bytesRead);

                    OnProgress?.Invoke(this, new ProgressArgs(bytesRead, false));
                }
            }
            while (bytesRead != 0);

            OnProgress?.Invoke(this, new ProgressArgs(bytesRead, true));
        }

        if (cancellationToken.IsCancellationRequested)
            return false;

        target.Position = 0;

        using var saveTo = File.Open(fullPath, FileMode.Create);

        await target.CopyToAsync(saveTo, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
            return false;

        return true;
    }
}
