using BPMInstaller.Core.Constants;
using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;
using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.Core.Resources;
using BPMInstaller.Core.Services.Application;
using BPMInstaller.Core.Services.Database.Postgres;

namespace BPMInstaller.Core.Services
{
    public class InstallationService
    {
        private IInstallationLogger InstallationLogger { get; }

        private IDatabaseService DatabaseService { get; }

        private RedisService RedisService { get; }

        private InstallationConfig InstallationConfig { get; }

        public InstallationService(IInstallationLogger logger, InstallationConfig installationConfig)
        {
            InstallationLogger = logger;
            InstallationConfig = installationConfig ?? throw new ArgumentException(nameof(installationConfig));
            DatabaseService = new PostgresDatabaseService(installationConfig.DatabaseConfig);
            RedisService = new RedisService();
        }

        public void Install()
        {
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.MainWorkflow.Started));
            try
            {
                StartBasicInstallation();
            }
            catch (Exception ex)
            {
                InstallationLogger.Log(InstallationMessage.Error(ex.Message));
            }
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.MainWorkflow.Ended, true));
        }

        private void StartBasicInstallation()
        {
            if (!ExecuteBeforeApplicationStarted())
            {
                return;
            }

            if (!InstallationConfig.Pipeline.StartApplication)
            {
                return;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Starting));
            var runningApplication = ApplicationRepository.GetInstance(InstallationConfig.ApplicationPath,
                InstallationConfig.ApplicationConfig);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Started));

            ExecuteAfterApplicationStarted(runningApplication);
        }

        private bool ExecuteBeforeApplicationStarted()
        {
            ExecuteFileSystemOperations();

            if (!InitializeDatabase())
            {
                return false;
            }

            if (!RestoreDatabase())
            {
                return false;
            }

            if (InstallationConfig.Pipeline.DisableForcePasswordChange)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ForcePasswordChange.Fixing));
                DatabaseService.DisableForcePasswordChange(ApplicationAdministrator.UserName);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ForcePasswordChange.Fixed));
            }

            return true;
        }

        private void ExecuteAfterApplicationStarted(IRunningApplication runningApplication)
        {
            if (InstallationConfig.Pipeline.InstallLicense)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Started));
                InstallLicense(runningApplication);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Ended));
            }

            if (InstallationConfig.Pipeline.CompileApplication)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Compiling));
                //TODO: Handle compilation response
                runningApplication.Compile();
            }
        }

        private bool InstallLicense(IRunningApplication runningApplication)
        {
            if (!InstallationConfig.Pipeline.InstallLicense || InstallationConfig.LicenseConfig == null)
            {
                return true;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.CidActualization));
            DatabaseService.UpdateCid(InstallationConfig.LicenseConfig.CId);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.CidActualized));

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Applying));
            runningApplication.AddLicenses(InstallationConfig.LicenseConfig);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Applied));

            InstallationLogger.Log(InstallationMessage.Info(string.Format(InstallationResources.Licensing.AssigningTo,
                ApplicationAdministrator.UserName)));
            DatabaseService.ApplyAdministratorLicenses(ApplicationAdministrator.UserName);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Assigned));

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Redis.Flushing));
            RedisService.FlushData(InstallationConfig.RedisConfig);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Redis.Flushed));

            return true;
        }

        public bool InitializeDatabase()
        {
            if (!InstallationConfig.Pipeline.RestoreDatabaseBackup)
            {
                return true;
            }

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

        private void ExecuteFileSystemOperations()
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
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.CookiesFixed));
            }
        }
    }
}
