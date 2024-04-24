using BPMInstaller.Core.Enums;

namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Данные для восстановления бекапа БД
    /// </summary>
    public class BackupRestorationConfig
    {
        /// <summary>
        /// Путь к бекапу БД
        /// </summary>
        public string BackupPath { get; set; }

        /// <summary>
        /// Способ восстановления БД
        /// </summary>
        public DatabaseDeploymentType RestorationKind { get; set; }

        /// <summary>
        /// Путь до Cli восстановления бд, если она развёрнута не в контейнере
        /// </summary>
        public string? RestorationCliLocation { get; set; }
        
        /// <summary>
        /// Образ в Docker-е, если БД развернута в контейнере
        /// </summary>
        /// TODO: rename to ContainerId
        public string? DockerImage { get; set; }
    }
}
