namespace GetEventVids;

public class ProgressArgs : EventArgs
{
    public ProgressArgs(int bytesRead, bool finished)
    {
        BytesRead = bytesRead;
        Finished = finished;
    }

    public int BytesRead { get; }
    public bool Finished { get; }
}
