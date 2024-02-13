using BPMInstaller.Core.Model;
using System.IO;

namespace BPMInstaller.UI.Desktop.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class DatabaseConfig: BaseUIModel
    {
        private string? host = "localhost";

        private ushort port = 5432;

        private string? userName = "postgres";

        private string? password;

        private string? backupPath;

        private string? databaseName = "bpm";

        private bool hostedByDocker = true;

        private string? restorationCliLocation;

        /// <inheritdoc cref="Core.Model.ApplicationConfig.Host"/>
        public string? Host { get { return host; } set { Set(ref host, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.Port"/>
        public ushort Port { get { return port; } set { Set(ref port, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.UserName"/>
        public string? UserName { get { return userName; } set { Set(ref userName, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.Password"/>
        public string? Password { get { return password; } set { Set(ref password, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.BackupPath"/>
        public string? BackupPath { get { return backupPath; } set { Set(ref backupPath, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.DatabaseName"/>
        public string? DatabaseName { get { return databaseName; } set { Set(ref databaseName, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.HostedByDocker"/>
        public bool HostedByDocker { get { return hostedByDocker; } set { Set(ref hostedByDocker, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.RestorationCliLocation"/>
        public string? RestorationCliLocation { get { return restorationCliLocation; } set { Set(ref restorationCliLocation, value); } }

        public void MergeConfig(Core.Model.DatabaseConfig databaseConfig)
        {
            Host = databaseConfig.Host;
            Port = databaseConfig.Port;
            UserName = databaseConfig.AdminUserName;
            Password = databaseConfig.AdminPassword;
            DatabaseName = databaseConfig.DatabaseName;
        }

        public Core.Model.DatabaseConfig ToCoreModel()
        {
            return new Core.Model.DatabaseConfig
            {
                Host = this.Host,
                Port = this.Port,
                AdminUserName = this.UserName,
                AdminPassword = this.Password,
                DatabaseName = this.DatabaseName,
                BackupPath = this.BackupPath,
                RestorationCliLocation = this.RestorationCliLocation,
                HostedByDocker = this.HostedByDocker

            };      
        }
    }
}
