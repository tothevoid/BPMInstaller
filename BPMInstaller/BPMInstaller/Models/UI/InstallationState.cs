using System.Collections.ObjectModel;
using System.Windows;
using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.UI.Desktop.Models.Basics;
using BPMInstaller.UI.Desktop.Utilities;

namespace BPMInstaller.UI.Desktop.Models.UI
{
    public class ControlsSessionState : ResponsiveModel
    {
        private bool isInstallationRunning = false;

        private string installationDuration = string.Empty;

        private Visibility startButtonVisibility = Visibility.Visible;

        public Visibility StartButtonVisibility
        {
            get => startButtonVisibility;
            private set => Set(ref startButtonVisibility, value);
        }

        public bool IsInstallationRunning
        {
            get => isInstallationRunning;
            private set => Set(ref isInstallationRunning, value);
        }

        public string InstallationDuration
        {
            get => installationDuration;
            private set => Set(ref installationDuration, value);
        }

        public ObservableCollection<InstallationMessage> Output { get; set; } = new ObservableCollection<InstallationMessage>();

        private int counter = 0;

        public void StartInstallation()
        {
            Output.Clear();
            counter = 0;
            StartButtonVisibility = Visibility.Collapsed;
            IsInstallationRunning = true;
            InstallationDuration = DateTimeUtilities.SecondsToString(++counter);
        }

        public void InstallationEnded()
        {
            IsInstallationRunning = false;
            StartButtonVisibility = Visibility.Visible;
        }
    }
}
