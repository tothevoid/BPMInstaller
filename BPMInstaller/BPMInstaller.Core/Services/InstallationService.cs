using BPMInstaller.Core.Model;

namespace BPMInstaller.Core.Services
{
    public class InstallationService
    {
        private event Action<string> OnInstallationMessageRecieved;

        public InstallationService(Action<string> messageHandler)
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

            OnInstallationMessageRecieved.Invoke("Database server validation started");

            if (!databaseService.ValidateConnection())
            {
               return;
            }

            OnInstallationMessageRecieved.Invoke("Database initialization started");

            if (!databaseService.CreateDatabase())
            {
               return;
            }

            OnInstallationMessageRecieved.Invoke("Backup restoration started");

            databaseService.RestoreDatabase();
            //TODO: migrate to specific dbService method that operates db model
            OnInstallationMessageRecieved.Invoke("Password fix started");
            databaseService.SuperuserPasswordFix(installationConfig.ApplicationConfig);
            OnInstallationMessageRecieved.Invoke("Cid update started");
            databaseService.UpdateCid(installationConfig.LicenseConfig);

            var distributiveService = new DistributiveService();

            OnInstallationMessageRecieved.Invoke("Appsettings actualization started");
            distributiveService.ActualizeAppComponentsConfig(installationConfig);
            var appService = new ApplicationService();

            OnInstallationMessageRecieved.Invoke("Application started");
            appService.RunApplication(installationConfig.ApplicationConfig, () =>
            {
                OnInstallationMessageRecieved.Invoke("Application rebuild started");

                appService.UploadLicenses(installationConfig.ApplicationConfig, installationConfig.LicenseConfig);

                appService.RebuildApplication(installationConfig.ApplicationConfig);
                OnInstallationMessageRecieved.Invoke("Installation ended");
            });
        }

    }
}
