using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json.Serialization;

namespace BPMInstaller.UI.Desktop.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class InstallationConfig: BaseUIModel
    {
        public event Action? OnModelChanged;

        public InstallationConfig()
        {
            ApplicationConfig = new ApplicationConfig();
            DatabaseConfig = new DatabaseConfig();
            RedisConfig = new RedisConfig();
            LicenseConfig = new LicenseConfig();
            InstallationWorkflow = new InstallationWorkflow();

            ActualizeTriggers();
        }

        public void ActualizeTriggers()
        {
            var configs = new List<BaseUIModel>()
            {
                ApplicationConfig,
                DatabaseConfig,
                RedisConfig,
                LicenseConfig,
                InstallationWorkflow
            };

            configs.ForEach(config =>
            {
                config.PropertyChanged += (object? sender, PropertyChangedEventArgs e) =>
                {
                    OnModelChanged?.Invoke();
                };
            });
        }

        private string? applicationPath;

        /// <inheritdoc cref="Core.Model.ApplicationConfig.ApplicationPath"/
        public string? ApplicationPath { get { return applicationPath; } set { Set(ref applicationPath, value); } }

        /// <inheritdoc cref="Model.ApplicationConfig"/>
        public ApplicationConfig ApplicationConfig { get; set; }

        /// <inheritdoc cref="Model.DatabaseConfig"/>
        public DatabaseConfig DatabaseConfig { get; set; } 

        /// <inheritdoc cref="Model.RedisConfig"/>
        public RedisConfig RedisConfig { get; set; }

        /// <inheritdoc cref="Core.LicenseConfig"/>
        public LicenseConfig LicenseConfig { get; set; }

        /// <inheritdoc cref="Core.InstallationWorkflow"/>
        public InstallationWorkflow InstallationWorkflow { get; set; }

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
            return new Core.Model.InstallationConfig
            {
                ApplicationPath = ApplicationPath ?? string.Empty,
                ApplicationConfig = ApplicationConfig.ToCoreModel(),
                DatabaseConfig = DatabaseConfig.ToCoreModel(),
                RedisConfig = new Core.Model.RedisConfig()
                {
                    Host = RedisConfig.Host ?? string.Empty,
                    DbNumber = RedisConfig.DbNumber,
                    Port = RedisConfig.Port
                },
                LicenseConfig = new Core.Model.LicenseConfig()
                {
                    Path = LicenseConfig.Path ?? string.Empty,
                    CId = LicenseConfig.CId
                },
                InstallationWorkflow = new Core.Model.InstallationWorkflow()
                {
                    InstallLicense = InstallationWorkflow.InstallLicense,
                    RemoveCertificate = InstallationWorkflow.RemoveCertificate,
                    RestoreDatabaseBackup = InstallationWorkflow.RestoreDatabaseBackup,
                    UpdateApplicationPort = InstallationWorkflow.UpdateApplicationPort,
                    UpdateDatabaseConnectionString = InstallationWorkflow.UpdateDatabaseConnectionString,
                    UpdateRedisConnectionString = InstallationWorkflow.UpdateRedisConnectionString,
                    DisableForcePasswordChange = InstallationWorkflow.DisableForcePasswordChange,
                    CompileApplication = InstallationWorkflow.CompileApplication,
                    StartApplication = InstallationWorkflow.StartApplication,
                    FixCookies = InstallationWorkflow.FixCookies
                }
            };
        }
    }
}
