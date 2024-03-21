﻿using BPMInstaller.Core.Enums;

namespace BPMInstaller.UI.Desktop.Model
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
        public string? DockerImage { get; set; }

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
