using System.Windows.Input;

namespace BPMInstaller.UI.Desktop.Model.UI
{
    public class ActionHandlersConfiguration
    {
        public ICommand StartInstallationCommand { get; init; }

        #region Commands:FS manipulation

        public ICommand SelectDistributivePathCommand { get; init; }

        public ICommand SelectBackupFileCommand { get; init; }

        public ICommand SelectCliPathCommand { get; init; }

        public ICommand SelectLicenseFileCommand { get; init; }
        #endregion

        #region Commands:Validation

        public ICommand ValidateRedisCommand { get; init; }

        public ICommand ValidateDatabaseCommand { get; init; }

        public ICommand ValidateApplicationCommand { get; init; }

        #endregion
    }
}
