using BPMInstaller.Core.Model.Runtime;

namespace BPMInstaller.Core.Interfaces
{
    public interface IInstallationLogger
    {
        void Log(InstallationMessage message);
    }
}
