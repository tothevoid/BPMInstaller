using BPMInstaller.UI.Desktop.Models.Basics;

namespace BPMInstaller.UI.Desktop.Models.Configs
{
    /// <inheritdoc cref="Core.Model.DatabaseConfig"/>
    public class DatabaseConfig : ResponsiveModel
    {
        private string host = "localhost";

        private ushort port = 5432;

        private string adminUserName = "postgres";

        private string adminPassword = string.Empty;

        private string databaseName = "bpm";

        /// <inheritdoc cref="Core.Model.DatabaseConfig.Host"/>
        public string Host
        {
            get => host;
            set => Set(ref host, value);
        }

        /// <inheritdoc cref="Core.Model.DatabaseConfig.Port"/>
        public ushort Port
        {
            get => port;
            set => Set(ref port, value);
        }

        /// <inheritdoc cref="Core.Model.DatabaseConfig.AdminUserName"/>
        public string AdminUserName
        {
            get => adminUserName;
            set => Set(ref adminUserName, value);
        }

        /// <inheritdoc cref="Core.Model.DatabaseConfig.AdminPassword"/>
        public string AdminPassword
        {
            get => adminPassword;
            set => Set(ref adminPassword, value);
        }

        /// <inheritdoc cref="Core.Model.DatabaseConfig.DatabaseName"/>
        public string DatabaseName
        {
            get => databaseName;
            set => Set(ref databaseName, value);
        }


        public void MergeConfig(Core.Model.DatabaseConfig databaseConfig)
        {
            Host = databaseConfig.Host;
            Port = databaseConfig.Port;
            AdminUserName = databaseConfig.AdminUserName;
            AdminPassword = databaseConfig.AdminPassword;
            DatabaseName = databaseConfig.DatabaseName;
        }

        /// TODO: Rework it with automapper or something lightweight
        public Core.Model.DatabaseConfig ToCoreModel()
        {
            return new Core.Model.DatabaseConfig
            {
                Host = Host,
                Port = Port,
                AdminUserName = AdminUserName,
                AdminPassword = AdminPassword,
                DatabaseName = DatabaseName
            };
        }
    }
}
