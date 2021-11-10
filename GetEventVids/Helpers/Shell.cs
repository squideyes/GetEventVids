using System.Diagnostics;
using System.Windows;

namespace GetEventVids;

public static class Shell
{
    public static bool Execute(string fullPath)
    {
        var p = new Process
        {
            StartInfo = new ProcessStartInfo(fullPath)
            {
                UseShellExecute = true
            }
        };

        try
        {
            p.Start();

            return true;
        }
        catch (Exception error)
        {
            MessageBox.Show("ERROR: " + error.Message,
                "Error", MessageBoxButton.OK, MessageBoxImage.Warning);

            return false;
        }
    }
}
