using SquidEyes.Basics;
using System.IO;

namespace GetEventVids;

internal static class MiscExtenders
{
    public static string GetLocalAppDataPath(this AppInfo appInfo, params string[] subFolders)
    {
        var path = Path.Combine(Environment.GetFolderPath(
           Environment.SpecialFolder.LocalApplicationData),
           CleanUp(appInfo.Company.Replace(",", "")), CleanUp(appInfo.Product));

        foreach (var subFolder in subFolders)
            path = Path.Combine(path, CleanUp(subFolder));

        return path;
    }

    private static string CleanUp(string value)
    {
        return Path.GetInvalidFileNameChars().Aggregate(value,
            (current, c) => current.Replace(c.ToString(), " ")).Trim();
    }
}
