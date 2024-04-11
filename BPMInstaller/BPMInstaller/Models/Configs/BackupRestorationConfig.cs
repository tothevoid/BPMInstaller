using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using BPMInstaller.Core.Enums;
using BPMInstaller.Core.Services;
using BPMInstaller.UI.Desktop.Models.Basics;

namespace BPMInstaller.UI.Desktop.Models.Configs
{
    /// <inheritdoc cref="Core.Model.BackupRestorationConfig"/>
    public class BackupRestorationConfig : ResponsiveModel
    {
        private string? backupPath;
        private string? restorationCliLocation;
        private string? dockerImage;

        private string selectedRestorationOption = RestorationMapping
            .FirstOrDefault(mapping => mapping.Value == DatabaseDeploymentType.Docker).Key ?? string.Empty;

        private ObservableCollection<string> activeContainers = new();

        /// TODO: Rework with specific converter
        private bool isDocker = true;
        private bool isCli = false;

        /// <inheritdoc cref="Core.Model.BackupRestorationConfig.BackupPath"/>
        public string? BackupPath
        {
            get => backupPath;
            set => Set(ref backupPath, value);
        }

        /// <inheritdoc cref="Core.Model.BackupRestorationConfig.RestorationCliLocation"/>
        public string? RestorationCliLocation
        {
            get => restorationCliLocation;
            set => Set(ref restorationCliLocation, value);
        }

        public ObservableCollection<string> ActiveContainers
        {
            get => activeContainers;
            set => Set(ref activeContainers, value);
        }

        public DatabaseDeploymentType RestorationKind => RestorationMapping[SelectedRestorationOption];

        public IEnumerable<string> RestorationOptions { get; } = RestorationMapping.Keys;

        private static Dictionary<string, DatabaseDeploymentType> RestorationMapping { get; } = new Dictionary<string, DatabaseDeploymentType>
        {
            { "Docker", DatabaseDeploymentType.Docker },
            { "PG_restore", DatabaseDeploymentType.Cli },
        };

        /// <inheritdoc cref="Core.Model.BackupRestorationConfig.DockerImage"/>
        public string? DockerImage
        {
            get
            {
                if (!ActiveContainers.Any())
                {
                    GetActiveContainers();
                }

                return dockerImage;
            }
            set => Set(ref dockerImage, value);
        }

        public string SelectedRestorationOption
        {
            get => selectedRestorationOption;
            set
            {
                Set(ref selectedRestorationOption, value);
                ModifyVisibility();
                if (RestorationKind == DatabaseDeploymentType.Docker)
                {
                    GetActiveContainers();
                }
            }
        }

        public bool IsDocker
        {
            get => isDocker;
            set => Set(ref isDocker, value);
        }

        public bool IsCli
        {
            get => isCli;
            set => Set(ref isCli, value);
        }

        public void ModifyVisibility()
        {
            IsDocker = RestorationKind == DatabaseDeploymentType.Docker;
            IsCli = RestorationKind == DatabaseDeploymentType.Cli;
        }

        public void GetActiveContainers()
        {
            var containers = new DockerService().GetActiveContainers().Select(x => x.Value).ToList();
            ActiveContainers = new ObservableCollection<string>(containers);
            if (ActiveContainers.Any())
            {
                DockerImage = ActiveContainers.First();
            }
        }

        public Core.Model.BackupRestorationConfig ToCoreModel()
        {
            return new Core.Model.BackupRestorationConfig
            {
                BackupPath = BackupPath,
                RestorationKind = RestorationKind,
                RestorationCliLocation = RestorationCliLocation,
                DockerImage = DockerImage
            };
        }
    }
}
