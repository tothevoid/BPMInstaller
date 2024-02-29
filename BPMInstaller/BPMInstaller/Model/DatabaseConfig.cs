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

        private string? backupPath;

        private string? databaseName = "bpm";

        private string? restorationCliLocation;

        private string? selectedRestorationOption = RestorationMapping
            .FirstOrDefault(mapping => mapping.Value == RestorationOption.Docker).Key ?? string.Empty;

        private string? selectedContainer;

        //Rework with special converter
        private bool isDocker = true;

        private bool isCli;

        private ObservableCollection<string> activeContainers = new ObservableCollection<string>();

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

        /// <inheritdoc cref="Core.Model.ApplicationConfig.RestorationCliLocation"/>
        public string? RestorationCliLocation { get { return restorationCliLocation; } set { Set(ref restorationCliLocation, value); } }

        public bool IsDocker { get { return isDocker; } set { Set(ref isDocker, value); } }

        public bool IsCli { get { return isCli; } set { Set(ref isCli, value); } }

        public string? SelectedContainer
        {
            get 
            {
                if (ActiveContainers == null || !ActiveContainers.Any())
                {
                    GetActiveContainers();
                }

                return selectedContainer;
            } 
            set { Set(ref selectedContainer, value); } 

        }

        public string SelectedRestorationOption 
        { 
            get { return selectedRestorationOption; }
            set 
            { 
                Set(ref selectedRestorationOption, value);
                ModifyVisibility();
                if (RestorationKind == RestorationOption.Docker)
                {
                    GetActiveContainers();
                }
            }
        }

        public ObservableCollection<string> ActiveContainers  { get { return activeContainers; } set { Set(ref activeContainers, value); } }

        public RestorationOption RestorationKind { get { return RestorationMapping[SelectedRestorationOption]; } } 

        public IEnumerable<string> RestorationOptions { get; } = RestorationMapping.Keys;

        private static Dictionary<string, RestorationOption> RestorationMapping { get; } = new Dictionary<string, RestorationOption>
        {
            { "Docker", RestorationOption.Docker },
            { "PG_restore", RestorationOption.CLI },
        };

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
                RestorationKind = this.RestorationKind
            };      
        }

        public void GetActiveContainers()
        {
            var containers = new DockerService().GetActiveContainers().Select(x => x.Value).ToList();
            ActiveContainers = new ObservableCollection<string>(containers);
            if (ActiveContainers.Any())
            {
                SelectedContainer = ActiveContainers.First();
            }
        }

        public void ModifyVisibility()
        {
            IsDocker = RestorationKind == RestorationOption.Docker;
            IsCli = RestorationKind == RestorationOption.CLI;
        }
    }
}
