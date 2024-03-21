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

        private IDatabaseService DatabaseService { get; }

        private ApplicationService ApplicationService { get; }

        private RedisService RedisService { get; }

        private InstallationConfig InstallationConfig { get; }

        public InstallationService(IInstallationLogger logger, InstallationConfig installationConfig)
        {
            InstallationLogger = logger;
            InstallationConfig = installationConfig ?? throw new ArgumentException(nameof(installationConfig));
            DatabaseService = new PostgresDatabaseService(installationConfig.DatabaseConfig);
            ApplicationService = new ApplicationService();
            RedisService = new RedisService();
        }

        public void Install()
        {
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.MainWorkflow.Started));
        }

        public void StartBasicInstallation()
        {
            

            ActualizeConfigs();

            if (!InitializeDatabase())
            {
                return;
            }

            if (!RestoreDatabase())
            {
                return;
            }

            if (InstallationConfig.Pipeline.DisableForcePasswordChange)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ForcePasswordChange.Fixing));

                DatabaseService.DisableForcePasswordChange(ApplicationAdministrator.UserName);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ForcePasswordChange.Fixed));
            }
            
            if (!InstallationConfig.Pipeline.StartApplication)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.MainWorkflow.Ended, true));
                return;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Instance.Validation));
            var closed = ApplicationService.CloseActiveApplication(InstallationConfig.ApplicationConfig.ApplicationPort,
                InstallationConfig.ExecutableApplicationPath);
            InstallationLogger.Log(InstallationMessage.Info(closed ?
                InstallationResources.Application.Instance.Terminated:
                InstallationResources.Application.Instance.ThereIsNoActiveInstance
            ));

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Starting));
            ApplicationService.RunApplication(InstallationConfig.ApplicationPath, () =>
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Started));

                if (InstallationConfig.Pipeline.InstallLicense)
                {
                    InstallLicense();
                }

                if (InstallationConfig.Pipeline.CompileApplication)
                {
                    InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Compiling));
                    ApplicationService.RebuildApplication(InstallationConfig.ApplicationConfig);
                }
              
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.MainWorkflow.Ended, true));
            });       
        }

        public bool InstallLicense()
        {
            if (!InstallationConfig.Pipeline.InstallLicense || InstallationConfig.LicenseConfig == null)
            {
                return false;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.CidActualization));
            DatabaseService.UpdateCid(InstallationConfig.LicenseConfig.CId);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.CidActualized));

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Applying));
            ApplicationService.UploadLicenses(InstallationConfig.ApplicationConfig, InstallationConfig.LicenseConfig);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Applied));

            InstallationLogger.Log(InstallationMessage.Info(string.Format(InstallationResources.Licensing.AssingingTo,
                ApplicationAdministrator.UserName)));
            DatabaseService.ApplyAdministratorLicenses(ApplicationAdministrator.UserName);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Applied));

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Redis.Flushing));
            RedisService.FlushData(InstallationConfig.RedisConfig);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Redis.Flushed));

            return true;
        }

        public bool InitializeDatabase()
        {
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Connection.Validating));
            var exceptionMessage = DatabaseService.ValidateConnection();
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                InstallationLogger.Log(InstallationMessage.Error(InstallationResources.Database.Connection.Failed));
                return false;
            }
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Connection.Success));

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.OtherConnections.Disconnecting));
            DatabaseService.TerminateAllActiveSessions(InstallationConfig.DatabaseConfig.DatabaseName);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.OtherConnections.Disconnected));

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Creation.Started));

            var databaseCreationResult = DatabaseService.CreateDatabase();
            if (!string.IsNullOrEmpty(databaseCreationResult))
            {
                var errorMessage = string.Format(InstallationResources.Database.Creation.Failed, databaseCreationResult);
                InstallationLogger.Log(InstallationMessage.Info(errorMessage, true));
                return false;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Creation.Done));
            return true;
        }

        public bool RestoreDatabase()
        {
            if (!InstallationConfig.Pipeline.RestoreDatabaseBackup)
            {
                return true;
            }

            if (InstallationConfig.BackupRestorationConfig == null)
            {
                return false;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Restoration.Started));

            IDatabaseRestorationService databaseRestorationService = new PostgresRestorationService(InstallationConfig.BackupRestorationConfig, InstallationConfig.DatabaseConfig);
            if (!databaseRestorationService.Restore())
            {
                InstallationLogger.Log(InstallationMessage.Error(InstallationResources.Database.Restoration.Failed));
                return false;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Restoration.Ended));
            return true;
        }

        public void ActualizeConfigs()
        {
            var distributiveService = new DistributiveService();

            if (InstallationConfig.Pipeline.UpdateDatabaseConnectionString || 
                InstallationConfig.Pipeline.UpdateRedisConnectionString)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ConnectionStrings.Actualization));
                distributiveService.UpdateConnectionStrings(InstallationConfig.ApplicationPath,
                    InstallationConfig.DatabaseConfig,
                    InstallationConfig.RedisConfig);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ConnectionStrings.Actualized));
            }

            if (InstallationConfig.Pipeline.UpdateApplicationPort)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.PortActualization));
                distributiveService.UpdateApplicationPort(InstallationConfig.ApplicationConfig, InstallationConfig.ApplicationPath);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.PortActualized));
            }

            if (InstallationConfig.Pipeline.FixCookies)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.FixingCookies));
                distributiveService.FixAuthorizationCookies(InstallationConfig.ApplicationPath);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.CookiedFixed));
            }
        }
    }
}
