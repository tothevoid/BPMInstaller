using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Enums;
using BPMInstaller.Core.Model;
using System.Diagnostics;

namespace BPMInstaller.Core.Services.Database.Postgres
{
    public class PostgresRestorationService: IDatabaseRestorationService
    {
        private DatabaseConfig DatabaseConfig { get; }
        private BackupRestorationConfig BackupRestorationConfig { get; }

        public PostgresRestorationService(BackupRestorationConfig backupRestorationConfig, DatabaseConfig databaseConfig)
        {
            BackupRestorationConfig = backupRestorationConfig ?? throw new ArgumentNullException(nameof(backupRestorationConfig));
            DatabaseConfig = databaseConfig ?? throw new ArgumentNullException(nameof(databaseConfig)); ;
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

            var docker = new DockerService();
            var backupName = DateTime.Now.ToString("dd-MM-HH:mm.backup");
            docker.CopyBackup(BackupRestorationConfig.BackupPath, BackupRestorationConfig.DockerImage, backupName);
            docker.RestoreBackup(BackupRestorationConfig.DockerImage, DatabaseConfig.AdminUserName,
                DatabaseConfig.DatabaseName, backupName);
            return true;
        }
    }
}
