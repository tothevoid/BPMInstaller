using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;
using System.Diagnostics;
using BPMInstaller.Core.Utilities;

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
        public bool RestoreByCli()
        {
            if (!File.Exists(BackupRestorationConfig.BackupPath))
            {
                return false;
            }

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

            return string.IsNullOrEmpty(executionResult.Output);
        }

        /// <summary>
        /// Восстановление с помощью Docker
        /// </summary>
        /// <returns>Бекап восстановлен</returns>
        public bool RestoreByDocker()
        {
            if (string.IsNullOrEmpty(BackupRestorationConfig.DockerImage))
            {
                return false;
            }

            var backupPath = $"/{GetBackupName()}";
            DockerService.CopyFileIntoContainer(BackupRestorationConfig.BackupPath, 
                BackupRestorationConfig.DockerImage, backupPath);
            RestorePostgresBackup(backupPath);
            return true;
        }

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

            var restorationScript = $"pg_restore {string.Join(" ", restorationParameters)}";
            var restorationDockerCommand = DockerService.ExecuteCommandInContainer(BackupRestorationConfig.DockerImage, 
                restorationScript).Output;

            var restorationOutput = DockerService.ExecuteCommandInContainer(BackupRestorationConfig.DockerImage, 
                restorationDockerCommand);
            return string.IsNullOrEmpty(restorationOutput.ErrorOutput);
        }

        private string GetBackupName()
        {
            return $"{DatabaseConfig.DatabaseName}.backup";
        }
    }
}
