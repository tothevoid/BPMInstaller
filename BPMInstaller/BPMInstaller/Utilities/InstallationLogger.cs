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
            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            OnInstallationMessageReceived += messageHandler;
        }

        public void Log(InstallationMessage message)
        {
            OnInstallationMessageReceived.Invoke(message);
        }
    }
}
