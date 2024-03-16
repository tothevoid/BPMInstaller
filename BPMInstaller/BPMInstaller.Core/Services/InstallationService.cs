using BPMInstaller.Core.Constants;
using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;
using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.Core.Resources;
using BPMInstaller.Core.Services.Database.Postgres;

namespace BPMInstaller.Core.Services
{
    public class InstallationService
    {
        private event Action<InstallationMessage> OnInstallationMessageReceived;

        public InstallationService(Action<InstallationMessage> messageHandler)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            OnInstallationMessageReceived += messageHandler;
        }

        public void StartBasicInstallation(InstallationConfig installationConfig)
        {
            if (installationConfig == null)
            {
                throw new ArgumentException(nameof(installationConfig));
            }

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.MainWorkflow.Started));

            if (installationConfig.InstallationWorkflow.RestoreDatabaseBackup)
            {
                if (!InitializeDatabase(installationConfig.DatabaseConfig))
                {
                    return;
                }
            }

            SetupDistributive(installationConfig.InstallationWorkflow, installationConfig);

            var databaseService = new PostgresDatabaseService(installationConfig.DatabaseConfig);

            var appService = new ApplicationService();
            var redisService = new RedisService();

            if (installationConfig.InstallationWorkflow.DisableForcePasswordChange)
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.ForcePasswordChange.Fixing));
              
                databaseService.DisableForcePasswordChange(ApplicationAdministrator.UserName);
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.ForcePasswordChange.Fixed));
            }
            
            if (!installationConfig.InstallationWorkflow.StartApplication)
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.MainWorkflow.Ended, true));
                return;
            }

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Application.Instance.Validation));
            var closed = appService.CloseActiveApplication(installationConfig.ApplicationConfig.ApplicationPort, 
                installationConfig.ExecutableApplicationPath);
            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(closed ?
                InstallationResources.Application.Instance.Terminated:
                InstallationResources.Application.Instance.ThereIsNoActiveInstance
            ));

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Application.Starting));
            appService.RunApplication(installationConfig.ApplicationPath, () =>
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Application.Started));

                if (installationConfig.InstallationWorkflow.InstallLicense)
                {
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Licensing.CidActualization));
                    databaseService.UpdateCid(installationConfig.LicenseConfig.CId);
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Licensing.CidActualized));

                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Licensing.Applying));
                    appService.UploadLicenses(installationConfig.ApplicationConfig, installationConfig.LicenseConfig);
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Licensing.Applied));

                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info(string.Format(InstallationResources.Licensing.AssingingTo,
                       ApplicationAdministrator.UserName)));
                    databaseService.ApplyAdministratorLicenses(ApplicationAdministrator.UserName);
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Licensing.Applied));

                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Redis.Flushing));
                    redisService.FlushData(installationConfig.RedisConfig);
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Redis.Flushed));
                }

                if (installationConfig.InstallationWorkflow.CompileApplication)
                {
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Application.Compiling));
                    appService.RebuildApplication(installationConfig.ApplicationConfig);
                }
              
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.MainWorkflow.Ended, true));
            });       
        }

        public bool InitializeDatabase(DatabaseConfig dbConfig)
        {
            var databaseService = new PostgresDatabaseService(dbConfig);

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Database.Connection.Validating));
            var exceptionMessage = databaseService.ValidateConnection();
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Error(InstallationResources.Database.Connection.Failed));
                return false;
            }
            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Database.Connection.Success));

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Database.OtherConnections.Disconnecting));
            databaseService.TerminateAllActiveSessions(dbConfig.DatabaseName);
            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Database.OtherConnections.Disconnected));

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Database.Creation.Started));

            var databaseCreationResult = databaseService.CreateDatabase();
            if (!string.IsNullOrEmpty(databaseCreationResult))
            {
                var errorMessage = string.Format(InstallationResources.Database.Creation.Failed, databaseCreationResult);
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(errorMessage, true));
                return false;
            }

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Database.Creation.Done));
            return true;
        }

        public bool RestoreDatabase(DatabaseConfig dbConfig, BackupRestorationConfig restorationConfig)
        {
            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Database.Restoration.Started));

            IDatabaseRestorationService databaseRestorationService = new PostgresRestorationService(restorationConfig, dbConfig);
            if (!databaseRestorationService.Restore())
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Error(InstallationResources.Database.Restoration.Failed));
                return false;
            }

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Database.Restoration.Ended));
            return true;
        }

        public void SetupDistributive(InstallationWorkflow workflow, InstallationConfig installationConfig)
        {
            var distributiveService = new DistributiveService();

            if (workflow.RestoreDatabaseBackup)
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.ConnectionStrings.Actualization));
                distributiveService.UpdateConnectionStrings(installationConfig, installationConfig.ApplicationPath);
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.ConnectionStrings.Actualized));
            }

            if (workflow.UpdateApplicationPort)
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Distributive.PortActualization));
                distributiveService.UpdateApplicationPort(installationConfig.ApplicationConfig, installationConfig.ApplicationPath);
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Distributive.PortActualized));
            }

            if (workflow.FixCookies)
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Distributive.FixingCookies));
                distributiveService.FixAuthorizationCookies(installationConfig.ApplicationPath);
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info(InstallationResources.Distributive.CookiedFixed));
            }
        }
    }
}
