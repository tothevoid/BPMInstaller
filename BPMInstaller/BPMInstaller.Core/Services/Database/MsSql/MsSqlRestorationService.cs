using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;

namespace BPMInstaller.Core.Services.Database.MsSql
{
    public class MsSqlRestorationService: IDatabaseRestorationService
    {
        private DatabaseConfig DatabaseConfig { get; }
        private BackupRestorationConfig BackupRestorationConfig { get; }

        public MsSqlRestorationService(BackupRestorationConfig backupRestorationConfig, DatabaseConfig databaseConfig)
        {
            BackupRestorationConfig = backupRestorationConfig ?? throw new ArgumentNullException(nameof(backupRestorationConfig));
            DatabaseConfig = databaseConfig ?? throw new ArgumentNullException(nameof(databaseConfig));
        }

        public bool RestoreByDocker(IInstallationLogger logger)
        {
            if (string.IsNullOrEmpty(BackupRestorationConfig.DockerImage))
            {
                return false;
            }

            var docker = new DockerService(logger);
            docker.CopyFileIntoContainer(BackupRestorationConfig.BackupPath, BackupRestorationConfig.DockerImage, DatabaseConfig.DatabaseName, DatabaseType.MsSql);
            docker.RestoreMsBackup(BackupRestorationConfig.DockerImage, DatabaseConfig, logger);
            return true;
        }

        public bool RestoreByCli()
        {
            return false;
        }
    }
}
