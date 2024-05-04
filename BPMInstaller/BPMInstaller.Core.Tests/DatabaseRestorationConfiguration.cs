using BPMInstaller.Core.Enums;
using BPMInstaller.Core.Model;
using BPMInstaller.Core.Services;

namespace BPMInstaller.Core.Tests
{
    public class DatabaseRestorationConfiguration
    {
        public InstallationService GetInstallationInstallationService(DatabaseType dbType, DatabaseDeploymentType deploymentType)
        {
            var path = dbType == DatabaseType.MsSql ?
                InfrastructureConstants.SqlServer.DistributivePath:
                InfrastructureConstants.Postgres.DistributivePath;

            return new InstallationService(new TestLogger(), GetInstallationConfig(path, dbType, deploymentType));
        }

        private InstallationConfig GetInstallationConfig(string distributivePath, DatabaseType dbType, DatabaseDeploymentType deploymentType)
        {
            return new InstallationConfig(distributivePath, GetTestApplicationConfig(), GetTestDatabaseConfig(dbType, deploymentType),
                GetTestRedisConfig(), GetTestDatabaseRestorationPipeline(), dbType)
            {
                BackupRestorationConfig = GetTestBackupRestorationConfig(distributivePath, dbType, deploymentType)
            };
        }

        private BackupRestorationConfig GetTestBackupRestorationConfig(string distributivePath, DatabaseType databaseType, DatabaseDeploymentType deploymentType)
        {
            var backupFileLocation = Directory.GetFiles(Path.Combine(distributivePath, "db")).FirstOrDefault();

            if (backupFileLocation == null)
            {
                throw new NotImplementedException();
            }

            return new BackupRestorationConfig()
            {
                RestorationKind = deploymentType,
                BackupPath = backupFileLocation,
                DockerImage = GetDockerImage(databaseType, deploymentType),
                RestorationCliLocation = (databaseType == DatabaseType.PostgreSql && deploymentType == DatabaseDeploymentType.Cli) ?
                    InfrastructureConstants.Postgres.RestoreCliPath :
                    null
            };
        }

        private string GetDockerImage(DatabaseType databaseType, DatabaseDeploymentType deploymentType)
        {
            if (deploymentType == DatabaseDeploymentType.Cli)
            {
                return string.Empty;
            }

            return databaseType == DatabaseType.PostgreSql ? 
                InfrastructureConstants.Postgres.DockerImage : 
                InfrastructureConstants.SqlServer.DockerImage;
        }

        private DatabaseConfig GetTestDatabaseConfig(DatabaseType dbType, DatabaseDeploymentType deploymentType)
        {
            string adminUserName = dbType == DatabaseType.PostgreSql ?
                InfrastructureConstants.Postgres.AdminUserName :
                InfrastructureConstants.SqlServer.AdminUserName;

            string adminPassword = dbType == DatabaseType.PostgreSql ?
                InfrastructureConstants.Postgres.AdminPassword:
                InfrastructureConstants.SqlServer.AdminPassword;

            return new DatabaseConfig()
            {
                DatabaseName = "bpm",
                Host = "localhost",
                AdminUserName = adminUserName,
                AdminPassword = adminPassword,
                Port = GetTestDatabasePort(dbType, deploymentType)
            };
        }

        private ushort GetTestDatabasePort(DatabaseType dbType, DatabaseDeploymentType deploymentType)
        {
            if (dbType == DatabaseType.MsSql)
            {
                return deploymentType == DatabaseDeploymentType.Docker ? 
                    InfrastructureConstants.SqlServer.DockerDatabasePort :
                    InfrastructureConstants.SqlServer.LocalDatabasePort;
            }

            if (dbType == DatabaseType.PostgreSql)
            {
                return deploymentType == DatabaseDeploymentType.Docker ? 
                    InfrastructureConstants.Postgres.DockerDatabasePort :
                    InfrastructureConstants.Postgres.LocalDatabasePort;
            }

            throw new NotImplementedException();
        }

        private RedisConfig GetTestRedisConfig()
        {
            return new RedisConfig()
            {
                DbNumber = 1,
                Host = "localhost",
                Port = 6379
            };
        }

        private ApplicationConfig GetTestApplicationConfig()
        {
            return new ApplicationConfig()
            {
                ApplicationPort = 5829
            };
        }

        private InstallationPipeline GetTestDatabaseRestorationPipeline()
        {
            return new InstallationPipeline()
            {
                DisableForcePasswordChange = true,
                RestoreDatabaseBackup = true,
                UpdateDatabaseConnectionString = true
            };
        }
    }
}
