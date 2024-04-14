using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

            var docker = new DockerService();
            var backupName = DateTime.Now.ToString("ddMMHHmm");
            var backupFullName = backupName + ".bak";
            docker.CopyBackup(BackupRestorationConfig.BackupPath, BackupRestorationConfig.DockerImage, backupFullName);
            docker.RestoreMsBackup(BackupRestorationConfig.DockerImage, DatabaseConfig, backupName, backupFullName, logger);
            return true;
        }

        public bool RestoreByCli()
        {
            return false;
        }
    }
}
