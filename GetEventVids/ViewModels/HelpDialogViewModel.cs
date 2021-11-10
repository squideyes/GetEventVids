using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace GetEventVids;

public class HelpDialogViewModel : ViewModelBase
{
    private const string GitHubUrl = "http://github.com/squideyes/GetEventVids";
    private const string EmailUrl = "mailto:louis_berman@epam.com";
    private const string IntroUrl = "http://somesite.com/GetDncVidsWalkthrough.mp4";

    public event EventHandler? OnClose;

    public static RelayCommand PlayIntroCommand => new(() => Shell.Execute(IntroUrl));

    public static RelayCommand GoToGithubCommand => new(() => Shell.Execute(GitHubUrl));

    public static RelayCommand SendEmailCommand => new(() => Shell.Execute(EmailUrl));

    public RelayCommand CloseCommand => new(() => OnClose?.Invoke(this, EventArgs.Empty));
}
