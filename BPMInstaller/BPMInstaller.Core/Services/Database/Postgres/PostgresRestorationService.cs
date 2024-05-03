using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;
using System.Diagnostics;

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
            if (string.IsNullOrEmpty(BackupRestorationConfig.RestorationCliLocation))
            {
                return false;
            }

            if (File.Exists(BackupRestorationConfig.RestorationCliLocation))
            {
                return false;
            }

            Process process = new Process();
            var backupFileName = Path.GetFileName(BackupRestorationConfig.BackupPath);
            process.StartInfo.WorkingDirectory = BackupRestorationConfig.BackupPath.Substring(0, 
                BackupRestorationConfig.BackupPath.Length - backupFileName.Length - 1);
            process.StartInfo.FileName = $"{BackupRestorationConfig.RestorationCliLocation}/pg_restore.exe";
            process.StartInfo.Arguments = $"--port={DatabaseConfig.Port} --username={DatabaseConfig.AdminUserName} --dbname={DatabaseConfig.DatabaseName} --no-owner --no-privileges ./{backupFileName}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.EnvironmentVariables["PGPASSWORD"] = DatabaseConfig.AdminPassword;

            process.Start();
            process.WaitForExit();
            var output = process.StandardError.ReadToEnd();
            return string.IsNullOrEmpty(output);
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

            DockerService.CopyFileIntoContainer(BackupRestorationConfig.BackupPath, BackupRestorationConfig.DockerImage, DatabaseConfig.DatabaseName);
            RestorePostgresBackup(BackupRestorationConfig.DockerImage, DatabaseConfig.AdminUserName, DatabaseConfig.DatabaseName);
            return true;
        }

        public bool RestorePostgresBackup(string containerId, string userName, string databaseName)
        {
            var restorationDockerCommand = GetPostgresRestorationCommand(containerId, userName);
            var restorationOutput = DockerService.ExecuteCommandInContainer(containerId, restorationDockerCommand);
            return string.IsNullOrEmpty(restorationOutput.ErrorOutput);
        }

        private string GetPostgresRestorationCommand(string containerId, string userName)
        {
            var backupName = GetBackupName();

            var restorationParameters = new[]
            {
                $"--username={userName}",
                $"--dbname={DatabaseConfig.DatabaseName}",
                "--no-owner",
                "--no-privileges",
                $"./{backupName}"
            };

            var restorationScript = $"pg_restore {string.Join(" ", restorationParameters)}";
            return DockerService.ExecuteCommandInContainer(containerId, restorationScript).Output;
        }

        private string GetBackupName()
        {
            return $"{DatabaseConfig.DatabaseName}.backup";
        }
    }
}
