using BPMInstaller.UI.Model;
using System.Windows;

namespace BPMInstaller.Model
{
    public class InstallationState: BaseUIModel
    {
        private Visibility startButtonVisibility = Visibility.Visible;

        public Visibility StartButtonVisibility { get { return startButtonVisibility; } set { Set(ref startButtonVisibility, value); } }
    }
}
