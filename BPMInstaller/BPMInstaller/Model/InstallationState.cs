using System;
using BPMInstaller.Core.Model;
using BPMInstaller.Core.Model.Runtime;
using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Threading;
using System.Windows;
using BPMInstaller.UI.Desktop.Utilities;

namespace BPMInstaller.UI.Desktop.Model
{
    public class ControlsSessionState: BaseUIModel
    {
        private bool isInstallationRunning = false;

        private string installationDuration = string.Empty;

        private Visibility startButtonVisibility = Visibility.Visible;

        public Visibility StartButtonVisibility { get { return startButtonVisibility; } private set { Set(ref startButtonVisibility, value); } }

        public bool IsInstallationRunning { get { return isInstallationRunning; } private set { Set(ref isInstallationRunning, value); } }

        public string InstallationDuration { get { return installationDuration; } private set { Set(ref installationDuration, value); } }

        public ObservableCollection<InstallationMessage> Output { get; set; } = new ObservableCollection<InstallationMessage>();

        private int counter = 0;

        public void StartInstallation()
        {
            Output.Clear();
            counter = 0;
            StartButtonVisibility = Visibility.Collapsed;
            IsInstallationRunning = true;
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            InstallationDuration = DateTimeUtilities.SecondsToString(++counter);
        }

        public void InstallationEnded()
        {
            IsInstallationRunning = false;
            StartButtonVisibility = Visibility.Visible;
        }


       
    }
}
