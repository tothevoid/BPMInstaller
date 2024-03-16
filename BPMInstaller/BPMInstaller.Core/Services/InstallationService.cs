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
        private IInstallationLogger InstallationLogger { get; }

        public InstallationService(IInstallationLogger logger)
        {
            InstallationLogger = logger;
        }

        public void StartBasicInstallation(InstallationConfig installationConfig)
        {
            if (installationConfig == null)
            {
                throw new ArgumentException(nameof(installationConfig));
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.MainWorkflow.Started));

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
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ForcePasswordChange.Fixing));
              
                databaseService.DisableForcePasswordChange(ApplicationAdministrator.UserName);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ForcePasswordChange.Fixed));
            }
            
            if (!installationConfig.InstallationWorkflow.StartApplication)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.MainWorkflow.Ended, true));
                return;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Instance.Validation));
            var closed = appService.CloseActiveApplication(installationConfig.ApplicationConfig.ApplicationPort, 
                installationConfig.ExecutableApplicationPath);
            InstallationLogger.Log(InstallationMessage.Info(closed ?
                InstallationResources.Application.Instance.Terminated:
                InstallationResources.Application.Instance.ThereIsNoActiveInstance
            ));

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Starting));
            appService.RunApplication(installationConfig.ApplicationPath, () =>
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Started));

                if (installationConfig.InstallationWorkflow.InstallLicense)
                {
                    InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.CidActualization));
                    databaseService.UpdateCid(installationConfig.LicenseConfig.CId);
                    InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.CidActualized));

                    InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Applying));
                    appService.UploadLicenses(installationConfig.ApplicationConfig, installationConfig.LicenseConfig);
                    InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Applied));

                    InstallationLogger.Log(InstallationMessage.Info(string.Format(InstallationResources.Licensing.AssingingTo,
                       ApplicationAdministrator.UserName)));
                    databaseService.ApplyAdministratorLicenses(ApplicationAdministrator.UserName);
                    InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Applied));

                    InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Redis.Flushing));
                    redisService.FlushData(installationConfig.RedisConfig);
                    InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Redis.Flushed));
                }

                if (installationConfig.InstallationWorkflow.CompileApplication)
                {
                    InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Compiling));
                    appService.RebuildApplication(installationConfig.ApplicationConfig);
                }
              
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.MainWorkflow.Ended, true));
            });       
        }

        public bool InitializeDatabase(DatabaseConfig dbConfig)
        {
            var databaseService = new PostgresDatabaseService(dbConfig);

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Connection.Validating));
            var exceptionMessage = databaseService.ValidateConnection();
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                InstallationLogger.Log(InstallationMessage.Error(InstallationResources.Database.Connection.Failed));
                return false;
            }
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Connection.Success));

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.OtherConnections.Disconnecting));
            databaseService.TerminateAllActiveSessions(dbConfig.DatabaseName);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.OtherConnections.Disconnected));

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Creation.Started));

            var databaseCreationResult = databaseService.CreateDatabase();
            if (!string.IsNullOrEmpty(databaseCreationResult))
            {
                var errorMessage = string.Format(InstallationResources.Database.Creation.Failed, databaseCreationResult);
                InstallationLogger.Log(InstallationMessage.Info(errorMessage, true));
                return false;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Creation.Done));
            return true;
        }

        public bool RestoreDatabase(DatabaseConfig dbConfig, BackupRestorationConfig restorationConfig)
        {
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Restoration.Started));

            IDatabaseRestorationService databaseRestorationService = new PostgresRestorationService(restorationConfig, dbConfig);
            if (!databaseRestorationService.Restore())
            {
                InstallationLogger.Log(InstallationMessage.Error(InstallationResources.Database.Restoration.Failed));
                return false;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Restoration.Ended));
            return true;
        }

        public void SetupDistributive(InstallationWorkflow workflow, InstallationConfig installationConfig)
        {
            var distributiveService = new DistributiveService();

            if (workflow.RestoreDatabaseBackup)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ConnectionStrings.Actualization));
                distributiveService.UpdateConnectionStrings(installationConfig, installationConfig.ApplicationPath);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ConnectionStrings.Actualized));
            }

            if (workflow.UpdateApplicationPort)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.PortActualization));
                distributiveService.UpdateApplicationPort(installationConfig.ApplicationConfig, installationConfig.ApplicationPath);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.PortActualized));
            }

            if (workflow.FixCookies)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.FixingCookies));
                distributiveService.FixAuthorizationCookies(installationConfig.ApplicationPath);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.CookiedFixed));
            }
        }
    }
}
