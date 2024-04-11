using System;
using System.Collections.Generic;
using System.IO;
using BPMInstaller.UI.Desktop.Models.Basics;

namespace BPMInstaller.UI.Desktop.Models.Configs
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class InstallationConfig : ResponsiveModel
    {
        public event Action? OnModelChanged;

        public InstallationConfig()
        {
            ActualizeTriggers();
        }

        public void ActualizeTriggers()
        {
            var configs = new List<ResponsiveModel>()
            {
                ApplicationConfig,
                DatabaseConfig,
                RedisConfig,
                LicenseConfig,
                InstallationPipeline
            };

            configs.ForEach(config =>
            {
                config.PropertyChanged += (_, _) =>
                {
                    OnModelChanged?.Invoke();
                };
            });
        }

        private string? applicationPath;

        /// <inheritdoc cref="Core.Model.InstallationConfig.ApplicationPath"/>
        public string? ApplicationPath
        {
            get => applicationPath;
            set => Set(ref applicationPath, value);
        }

        /// <inheritdoc cref="Core.Model.InstallationConfig.ApplicationConfig"/>
        public ApplicationConfig ApplicationConfig { get; set; } = new();

        /// <inheritdoc cref="Core.Model.InstallationConfig.DatabaseConfig"/>
        public DatabaseConfig DatabaseConfig { get; set; } = new();

        /// <inheritdoc cref="Core.Model.InstallationConfig.RedisConfig"/>
        public RedisConfig RedisConfig { get; set; } = new();

        /// <inheritdoc cref="Core.Model.InstallationConfig.LicenseConfig"/>
        public LicenseConfig LicenseConfig { get; set; } = new();

        /// <inheritdoc cref="Core.Model.InstallationConfig.Pipeline"/>
        public InstallationPipeline InstallationPipeline { get; set; } = new();

        /// <inheritdoc cref="Core.Model.InstallationConfig.BackupRestorationConfig"/>
        public BackupRestorationConfig BackupRestorationConfig { get; set; } = new();

        public string? ValidateApplicationPath()
        {
            if (string.IsNullOrEmpty(applicationPath?.Trim()))
            {
                return "Путь до дистрибутива не может быть пустым";
            }
            else if (!Directory.Exists(applicationPath))
            {
                return "Указанной директории не существует";
            }
            else if (!File.Exists(Path.Combine(applicationPath, "BPMSoft.WebHost.dll")))
            {
                return "Указанный путь не является дистрибутивом";
            }
            return null;
        }

        // TODO: Fix architecture
        public Core.Model.InstallationConfig ConvertToCoreModel()
        {
            return new Core.Model.InstallationConfig(ApplicationPath ?? string.Empty,
                ApplicationConfig.ToCoreModel(),
                DatabaseConfig.ToCoreModel(),
                RedisConfig.ToCoreModel(),
                InstallationPipeline.ToCoreModel())
            {
                LicenseConfig = LicenseConfig.ToCoreModel(),
                BackupRestorationConfig = BackupRestorationConfig.ToCoreModel()
            };
        }
    }
}
