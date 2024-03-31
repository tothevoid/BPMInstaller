using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using BPMInstaller.Core.Enums;
using System.Collections.ObjectModel;
using BPMInstaller.Core.Services;

namespace BPMInstaller.UI.Desktop.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class DatabaseConfig : BaseUIModel
    {
        private string? host = "localhost";

        private ushort port = 5432;

        private string? userName = "postgres";

        private string? password;

        private string? databaseName = "bpm";

        /// <inheritdoc cref="Core.Model.ApplicationConfig.Host"/>
        public string? Host 
        { 
            get => host;
            set => Set(ref host, value);
        }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.Port"/>
        public ushort Port
        {
            get => port;
            set => Set(ref port, value);
        }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.UserName"/>
        public string? UserName 
        { 
            get => userName;
            set => Set(ref userName, value);
        }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.Password"/>
        public string? Password 
        { 
            get => password;
            set => Set(ref password, value);
        }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.DatabaseName"/>
        public string? DatabaseName 
        { 
            get => databaseName;
            set => Set(ref databaseName, value);
        }


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
                DatabaseName = this.DatabaseName
            };      
        }
    }
}
