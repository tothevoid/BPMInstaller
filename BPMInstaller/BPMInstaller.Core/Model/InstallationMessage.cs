namespace BPMInstaller.Core.Model
{
    public class InstallationMessage
    {
        public string Content { get; init; }

        public bool IsTerminal { get; init; } = false;
    }
}
