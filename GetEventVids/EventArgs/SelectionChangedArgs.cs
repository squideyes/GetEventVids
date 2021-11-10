namespace GetEventVids;

public class SelectionChangedArgs : EventArgs
{
    public SelectionChangedArgs(bool value)
    {
        Value = value;
    }

    public bool Value { get; }
}
