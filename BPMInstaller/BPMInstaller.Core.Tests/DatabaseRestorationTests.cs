using BPMInstaller.Core.Enums;
using BPMInstaller.Core.Model;

namespace BPMInstaller.Core.Tests
{
    /// Перед запуском текстов необходимо актуализировать InfrastructureConstants
    [TestFixture]
    public class DatabaseRestorationTests
    {
        /// <summary>
        /// docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=Test123!" -p 1434:1433 --name mssql -d mcr.microsoft.com/mssql/server
        /// </summary>
        [Test]
        [NonParallelizable]
        public void TestSqlServerWithDocker()
        {
            TestBackupRestoration(DatabaseType.MsSql, DatabaseDeploymentType.Docker);

        }

        [Test]
        [NonParallelizable]
        public void TestSqlServerCli()
        {
            TestBackupRestoration(DatabaseType.MsSql, DatabaseDeploymentType.Cli);
        }

        [Test]
        [NonParallelizable]
        public void TestPgWithDocker()
        {
            TestBackupRestoration(DatabaseType.PostgreSql, DatabaseDeploymentType.Docker);
        }

        [Test]
        [NonParallelizable]
        public void TestPgWithCli()
        {
            TestBackupRestoration(DatabaseType.PostgreSql, DatabaseDeploymentType.Cli);
        }

        private void TestBackupRestoration(DatabaseType dbType, DatabaseDeploymentType deploymentType)
        {
            var manager = new DatabaseRestorationConfiguration();
            var installationService = manager.GetInstallationInstallationService(dbType, deploymentType);
            var installationResult = installationService.Install();
            Assert.AreEqual(string.Empty, installationResult);
        }
    }

}