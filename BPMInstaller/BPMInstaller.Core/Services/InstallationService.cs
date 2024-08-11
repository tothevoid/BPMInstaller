using System.Reflection;
using BPMInstaller.Core.Constants;
using BPMInstaller.Core.Enums;
using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;
using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.Core.Resources;
using BPMInstaller.Core.Services.Application;
using BPMInstaller.Core.Services.Database.MsSql;
using BPMInstaller.Core.Services.Database.Postgres;

namespace BPMInstaller.Core.Services
{
    public class InstallationService
    {
        private IInstallationLogger InstallationLogger { get; }

        private IDatabaseService DatabaseService { get; }

        private IDatabaseRestorationService DatabaseRestorationService { get; }

        private RedisService RedisService { get; }

        private InstallationConfig InstallationConfig { get; }

        private DistributiveService DistributiveService { get; }

        private Action<string>? OnStepCompleted { get; }

        public InstallationService(IInstallationLogger logger, InstallationConfig installationConfig, Action<string>? onStepCompleted = null)
        {
            InstallationLogger = logger;
            InstallationConfig = installationConfig ?? throw new ArgumentException(nameof(installationConfig));
            DistributiveService = new DistributiveService();

            var dbServices = GetDatabaseServices();
            DatabaseService = dbServices.DbService;
            DatabaseRestorationService = dbServices.BackupService;
            RedisService = new RedisService();

            if (onStepCompleted != null)
            {
                OnStepCompleted = onStepCompleted;
            }
        }

        private (IDatabaseService DbService, IDatabaseRestorationService BackupService) GetDatabaseServices()
        {
            switch (InstallationConfig.DatabaseType)
            {
                case DatabaseType.MsSql:
                    return (new MsSqlDatabaseService(InstallationConfig.DatabaseConfig), 
                        new MsSqlRestorationService(InstallationConfig.BackupRestorationConfig, InstallationConfig.DatabaseConfig, InstallationLogger));
                case DatabaseType.PostgreSql:
                    return (new PostgresDatabaseService(InstallationConfig.DatabaseConfig), 
                        new PostgresRestorationService(InstallationConfig.BackupRestorationConfig, InstallationConfig.DatabaseConfig, InstallationLogger));
                default:
                    throw new NotImplementedException(InstallationConfig.DatabaseType.ToString());
            }
        }

        public string Install()
        {
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.MainWorkflow.Started));
            try
            {
                var installationResult = StartBasicInstallation();
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.MainWorkflow.Ended, true));
                return installationResult;
            }
            catch (Exception ex)
            {
                InstallationLogger.Log(InstallationMessage.Error(ex.Message));
                return ex.Message;
            }
        }

        private string StartBasicInstallation()
        {
            var executionBeforeApplicationStartedResult = ExecuteBeforeApplicationStarted();
            if (!string.IsNullOrEmpty(executionBeforeApplicationStartedResult))
            {
                return executionBeforeApplicationStartedResult;
            }

            if (!InstallationConfig.Pipeline.StartApplication)
            {
                return string.Empty;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Starting));
            var runningApplication = ApplicationRepository.GetInstance(InstallationConfig.ApplicationPath,
                InstallationConfig.ApplicationConfig);
            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Started));
            OnStepCompleted?.Invoke("");

            ExecuteAfterApplicationStarted(runningApplication);

            return string.Empty;
        }

        private string ExecuteBeforeApplicationStarted()
        {
            ExecuteFileSystemOperations();

            var initializationResult = InitializeDatabase();
            if (!string.IsNullOrEmpty(initializationResult))
            {
                return initializationResult;
            }

            var restorationResult = RestoreDatabase();
            if (!string.IsNullOrEmpty(restorationResult))
            {
                return restorationResult;
            }

            if (InstallationConfig.Pipeline.DisableForcePasswordChange)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ForcePasswordChange.Fixing));
                DatabaseService.DisableForcePasswordChange(ApplicationAdministrator.UserName);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ForcePasswordChange.Fixed));
                OnStepCompleted?.Invoke("");
            }

            return string.Empty;
        }

        private void ExecuteAfterApplicationStarted(IRunningApplication runningApplication)
        {
            if (InstallationConfig.Pipeline.InstallLicense)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Started));
                InstallLicense(runningApplication);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Licensing.Ended));
                OnStepCompleted?.Invoke("");
            }

            if (InstallationConfig.Pipeline.CompileApplication)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Application.Compiling));
                //TODO: Handle compilation response
                runningApplication.Compile();
                OnStepCompleted?.Invoke("");
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

        private string InitializeDatabase()
        {
            if (!InstallationConfig.Pipeline.RestoreDatabaseBackup)
            {
                return string.Empty;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Connection.Validating));
            var exceptionMessage = DatabaseService.ValidateConnection();
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                var errorMessage = InstallationResources.Database.Connection.Failed;
                InstallationLogger.Log(InstallationMessage.Error(errorMessage));
                return errorMessage;
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
                return errorMessage;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Creation.Done));
            OnStepCompleted?.Invoke("");
            return string.Empty;
        }

        private string RestoreDatabase()
        {
            if (!InstallationConfig.Pipeline.RestoreDatabaseBackup)
            {
                return string.Empty;
            }

            if (InstallationConfig.BackupRestorationConfig == null)
            {
                return InstallationResources.Database.Restoration.EmptyConfig;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Restoration.Started));

            var restorationResult = Restore(InstallationConfig.BackupRestorationConfig.RestorationKind);
            if (!string.IsNullOrEmpty(restorationResult))
            {
                var errorMessage = string.Format(InstallationResources.Database.Restoration.Failed, restorationResult);
                InstallationLogger.Log(InstallationMessage.Error(errorMessage));
                return errorMessage;
            }

            InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Database.Restoration.Ended));
            OnStepCompleted?.Invoke("");
            return string.Empty;
        }

        private string Restore(DatabaseDeploymentType restorationKind)
        {
            return restorationKind switch
            {
                DatabaseDeploymentType.Cli => DatabaseRestorationService.RestoreByCli(),
                DatabaseDeploymentType.Docker => DatabaseRestorationService.RestoreByDocker(),
                _ => throw new NotImplementedException()
            };
        }

        private void ExecuteFileSystemOperations()
        {
            if (InstallationConfig.Pipeline.UpdateDatabaseConnectionString || 
                InstallationConfig.Pipeline.UpdateRedisConnectionString)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ConnectionStrings.Actualization));
                DistributiveService.UpdateConnectionStrings(InstallationConfig.ApplicationPath,
                    InstallationConfig.DatabaseType,
                    InstallationConfig.DatabaseConfig,
                    InstallationConfig.RedisConfig);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.ConnectionStrings.Actualized));

                OnStepCompleted?.Invoke("");
            }

            if (InstallationConfig.Pipeline.UpdateApplicationPort)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.PortActualization));
                DistributiveService.UpdateApplicationPort(InstallationConfig.ApplicationConfig, InstallationConfig.ApplicationPath);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.PortActualized));

                OnStepCompleted?.Invoke("");
            }

            if (InstallationConfig.Pipeline.FixCookies)
            {
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.FixingCookies));
                DistributiveService.FixAuthorizationCookies(InstallationConfig.ApplicationPath);
                InstallationLogger.Log(InstallationMessage.Info(InstallationResources.Distributive.CookiesFixed));

                OnStepCompleted?.Invoke("");
            }

            if (InstallationConfig.Pipeline.SwitchApplicationMode && 
                InstallationConfig.ApplicationMode != ApplicationMode.NotSpecified)
            {
                var modeCaption = InstallationConfig.ApplicationMode == ApplicationMode.Database? 
                    InstallationResources.ApplicationMode.Db:
                    InstallationResources.ApplicationMode.FileSystem;

                InstallationLogger.Log(InstallationMessage
                    .Info(string.Format(InstallationResources.Distributive.SwitchingApplicationMode, modeCaption)));
                DistributiveService.SwitchApplicationMode(InstallationConfig.ApplicationPath, InstallationConfig.ApplicationMode);
                InstallationLogger.Log(InstallationMessage
                    .Info(string.Format(InstallationResources.Distributive.ApplicationModeSwitched, modeCaption)));

                OnStepCompleted?.Invoke("");
            }
        }
    }
}
