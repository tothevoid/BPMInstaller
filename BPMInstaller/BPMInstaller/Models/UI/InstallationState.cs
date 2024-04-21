using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.UI.Desktop.Models.Basics;
using BPMInstaller.UI.Desktop.Utilities;

namespace BPMInstaller.UI.Desktop.Models.UI
{
    public class ControlsSessionState : ResponsiveModel
    {
        private int counter = 0;

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

        public int Counter
        {
            get => counter;
            private set
            {
                counter = value;
                UpdateInstallationDuration(value);
            }
        }

        // TODO: fix dispatcher injection
        public void StartInstallation()
        {
            Output.Clear();
            Counter = 0;
            StartButtonVisibility = Visibility.Collapsed;
            IsInstallationRunning = true;
        }

        public async Task StartCounter(Action<Action> uiThreadAction)
        {
            while (isInstallationRunning)
            {
                await Task.Delay(1000);
                uiThreadAction(() =>
                {
                    Counter++;
                });
            }
        }

        private void UpdateInstallationDuration(int currentCounterValue)
        {
            InstallationDuration = DateTimeUtilities.SecondsToString(currentCounterValue);
        }

        public void InstallationEnded()
        {
            IsInstallationRunning = false;
            StartButtonVisibility = Visibility.Visible;
        }
    }
}
