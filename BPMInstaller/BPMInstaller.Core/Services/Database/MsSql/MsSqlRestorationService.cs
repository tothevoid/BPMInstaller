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
        public MsSqlRestorationService(BackupRestorationConfig backupRestorationConfig, DatabaseConfig databaseConfig)
        {
            
        }

        public bool RestoreByDocker()
        {
            throw new NotImplementedException();
        }

        public bool RestoreByCli()
        {
            throw new NotImplementedException();
        }
    }
}
