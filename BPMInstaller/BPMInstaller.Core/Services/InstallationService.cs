using BPMInstaller.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Services
{
    public class InstallationService
    {
        public void StartBasicInstallation(InstallationConfig installationConfig)
        {
            var databaseService = new PostgresDatabaseService(installationConfig.DatabaseConfig);

            if (!databaseService.ValidateConnection())
            {
                return;
            }

            if (!databaseService.CreateDatabase())
            {
                return;
            }

            databaseService.RestoreDatabase();
            //TODO: migrate to specific dbService method that operates db model
            databaseService.IncreasePasswordDuration(installationConfig.ApplicationConfig);

            var distributiveService = new DistributiveService();
            distributiveService.ActualizeAppComponentsConfig(installationConfig);
            var appService = new ApplicationService();

            appService.RunApplication(installationConfig.ApplicationConfig);
            appService.RebuildApplication(installationConfig.ApplicationConfig);
        }

    }
}
