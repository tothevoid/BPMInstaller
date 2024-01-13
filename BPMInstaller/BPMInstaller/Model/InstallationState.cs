using BPMInstaller.Core.Model;
using BPMInstaller.UI.Model;
using System.Collections.ObjectModel;
using System.Windows;

namespace BPMInstaller.Model
{
    public class ControlsSessionState: BaseUIModel
    {
        private Visibility startButtonVisibility = Visibility.Visible;

        public Visibility StartButtonVisibility { get { return startButtonVisibility; } set { Set(ref startButtonVisibility, value); } }


        public ObservableCollection<InstallationMessage> Output { get; set; } = new ObservableCollection<InstallationMessage>();
    }
}
