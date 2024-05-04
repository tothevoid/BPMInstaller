using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model.Runtime;

namespace BPMInstaller.Core.Tests
{
    public class TestLogger : IInstallationLogger
    {
        public void Log(InstallationMessage message)
        {
            TestContext.Progress.WriteLine(message.Text);
        }
    }
}
