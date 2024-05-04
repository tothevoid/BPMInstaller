using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;
using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.Core.Utilities;
using RestorationResources = BPMInstaller.Core.Resources.InstallationResources.Database.Restoration;

namespace BPMInstaller.Core.Services.Database.Postgres
{
    public class PostgresRestorationService: IDatabaseRestorationService
    {
        private DatabaseConfig DatabaseConfig { get; }
        private BackupRestorationConfig BackupRestorationConfig { get; }
        private IInstallationLogger InstallationLogger { get; }
        private DockerService DockerService{ get; }

        public PostgresRestorationService(BackupRestorationConfig backupRestorationConfig, DatabaseConfig databaseConfig, IInstallationLogger logger)
        {
            BackupRestorationConfig = backupRestorationConfig ?? throw new ArgumentNullException(nameof(backupRestorationConfig));
            DatabaseConfig = databaseConfig ?? throw new ArgumentNullException(nameof(databaseConfig));
            InstallationLogger = logger ?? throw new ArgumentNullException(nameof(logger));

            DockerService = new DockerService();
        }

        /// <inheritdoc cref="IDatabaseRestorationService.RestoreByCli"/>
        public string RestoreByCli()
        {
            var backupFileName = Path.GetFileName(BackupRestorationConfig.BackupPath);
            var directory = BackupRestorationConfig.BackupPath.Substring(0,
                BackupRestorationConfig.BackupPath.Length - backupFileName.Length - 1);

            var executionResult = new CommandLineQueryExecutor(BackupRestorationConfig.RestorationCliLocation, directory)
                .AddParameter("--port", DatabaseConfig.Port.ToString(), "=")
                .AddParameter("--username", DatabaseConfig.AdminUserName, "=")
                .AddParameter("--dbname", DatabaseConfig.DatabaseName, "=")
                .AddParameter("--no-owner")
                .AddParameter("--no-privileges")
                .AddParameter($"./{backupFileName}")
                .AddEnvironmentVariable("PGPASSWORD", DatabaseConfig.AdminPassword)
                .Execute();

            return executionResult.Output;
        }

        /// <summary>
        /// Восстановление с помощью Docker
        /// </summary>
        /// <returns>Бекап восстановлен</returns>
        public string RestoreByDocker()
        {
            var backupPath = $"/{GetBackupName()}";
            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.CopyingBackupFile));
            var isCopied = DockerService.CopyFileIntoContainer(BackupRestorationConfig.BackupPath, 
                BackupRestorationConfig.DockerImage, backupPath);

            if (!isCopied)
            {
                InstallationLogger.Log(InstallationMessage.Info(RestorationResources.FileIsNotCopiedIntoContainer));
            }

            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.BackupFileCopied));

           
            RestorePostgresBackup(backupPath);
            
            return string.Empty;
        }

        //TODO: Use CLI API
        public bool RestorePostgresBackup(string backupPath)
        {
            var restorationParameters = new[]
            {
                $"--username={DatabaseConfig.AdminUserName}",
                $"--dbname={DatabaseConfig.DatabaseName}",
                "--no-owner",
                "--no-privileges",
                $"./{backupPath}"
            };

            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.GeneratingDataMigrationCommand));
            var restorationScript = $"pg_restore {string.Join(" ", restorationParameters)}";
            var restorationDockerCommand = DockerService.ExecuteCommandInContainer(BackupRestorationConfig.DockerImage, 
                restorationScript).Output;
            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.DataMigrationCommandGenerated));

            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.DataMigrationStarted));
            var restorationOutput = DockerService.ExecuteCommandInContainer(BackupRestorationConfig.DockerImage, 
                restorationDockerCommand);
            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.DataMigrationEnded));
            return string.IsNullOrEmpty(restorationOutput.ErrorOutput);
        }

        private string GetBackupName()
        {
            return $"{DatabaseConfig.DatabaseName}.backup";
        }
    }
}
