using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model.Runtime;
using System;

namespace BPMInstaller.UI.Desktop.Utilities
{
    internal class InstallationLogger : IInstallationLogger
    {
        private event Action<InstallationMessage> OnInstallationMessageReceived;

        public InstallationLogger(Action<InstallationMessage> messageHandler)
        {
            OnInstallationMessageReceived += messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        }

        public void Log(InstallationMessage message)
        {
            OnInstallationMessageReceived.Invoke(message);
        }
    }
}
