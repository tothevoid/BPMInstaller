using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.UI.Desktop.Models.Basics;
using BPMInstaller.UI.Desktop.Utilities;

namespace BPMInstaller.UI.Desktop.Models.UI
{
    public class ControlsSessionState : ResponsiveModel
    {
        private int stepsPassed = 0;

        private sbyte activeTabIndex = 0;

        private int secondsFromStart = 0;

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

        public sbyte ActiveTabIndex
        {
            get => activeTabIndex;
            set => Set(ref activeTabIndex, value);
        }

        public string InstallationDuration
        {
            get => installationDuration;
            private set => Set(ref installationDuration, value);
        }

        public int StepsPassed
        {
            get => stepsPassed;
            set => Set(ref stepsPassed, value);
        }

        public ObservableCollection<InstallationMessage> Output { get; set; } = new ObservableCollection<InstallationMessage>();

        public int SecondsFromStart
        {
            get => secondsFromStart;
            private set
            {
                secondsFromStart = value;
                UpdateInstallationDuration(value);
            }
        }

        // TODO: fix dispatcher injection
        public void StartInstallation()
        {
            Output.Clear();
            StepsPassed = 0;
            SecondsFromStart = 0;
            ActiveTabIndex = 1;
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
                    SecondsFromStart++;
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
