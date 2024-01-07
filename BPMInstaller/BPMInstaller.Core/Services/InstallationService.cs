using BPMInstaller.Core.Model;

namespace BPMInstaller.Core.Services
{
    public class InstallationService
    {
        private event Action<InstallationMessage> OnInstallationMessageRecieved;

        public InstallationService(Action<InstallationMessage> messageHandler)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            OnInstallationMessageRecieved += messageHandler;
        }

        public void StartBasicInstallation(InstallationConfig installationConfig)
        {
            var databaseService = new PostgresDatabaseService(installationConfig.DatabaseConfig);

            OnInstallationMessageRecieved.Invoke(new InstallationMessage() { Content = "Database server validation started" });

            if (!databaseService.ValidateConnection())
            {
                return;
            }

            OnInstallationMessageRecieved.Invoke(new InstallationMessage() { Content = "Database initialization started" });

            if (!databaseService.CreateDatabase())
            {
                return;
            }

            OnInstallationMessageRecieved.Invoke(new InstallationMessage() { Content = "Backup restoration started" });

            databaseService.RestoreDatabase();
            //TODO: migrate to specific dbService method that operates db model
            OnInstallationMessageRecieved.Invoke(new InstallationMessage() { Content = "Password fix started" });
            databaseService.SuperuserPasswordFix(installationConfig.ApplicationConfig);
            OnInstallationMessageRecieved.Invoke(new InstallationMessage() { Content = "Cid update started" });
            databaseService.UpdateCid(installationConfig.LicenseConfig);

            var distributiveService = new DistributiveService();

            OnInstallationMessageRecieved.Invoke(new InstallationMessage() { Content = "Appsettings actualization started" });
            distributiveService.ActualizeAppComponentsConfig(installationConfig);
            var appService = new ApplicationService();

            OnInstallationMessageRecieved.Invoke(new InstallationMessage() { Content = "Application started" });
            appService.RunApplication(installationConfig.ApplicationConfig, () =>
            {
                OnInstallationMessageRecieved.Invoke(new InstallationMessage() { Content = "Application rebuild started" });

                appService.UploadLicenses(installationConfig.ApplicationConfig, installationConfig.LicenseConfig);

                appService.RebuildApplication(installationConfig.ApplicationConfig);
                OnInstallationMessageRecieved.Invoke(new InstallationMessage() { Content = "Installation ended", IsTerminal = true });
            });
        }
    }
}
