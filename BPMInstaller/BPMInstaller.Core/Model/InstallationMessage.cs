using System.Drawing;

namespace BPMInstaller.Core.Model
{
    public class InstallationMessage
    {
        public string Content { get; init; }

        public string Date { get; } = DateTime.Now.ToString("HH:mm:ss");

        public bool IsError { get; set; } = false;

        public bool IsTerminal { get; init; } = false;
    }
}
