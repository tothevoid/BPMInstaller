using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace BPMInstaller.UI.Desktop.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class InstallationConfig
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

        [JsonIgnore]
        public ControlsSessionState ControlsSessionState { get; set; } = new ControlsSessionState();

        // TODO: Fix architecture
        public Core.Model.InstallationConfig ConvertToCoreModel()
        {
            return new Core.Model.InstallationConfig
            {
                ApplicationConfig = new Core.Model.ApplicationConfig()
                {
                    ApplicationPath = ApplicationConfig.ApplicationPath ?? string.Empty,
                    ApplicationPort = ApplicationConfig.ApplicationPort
                },
                DatabaseConfig = new Core.Model.DatabaseConfig
                {
                    Host = DatabaseConfig.Host ?? string.Empty,
                    AdminUserName = DatabaseConfig.UserName ?? string.Empty,
                    AdminPassword = DatabaseConfig.Password ?? string.Empty,
                    BackupPath = DatabaseConfig.BackupPath ?? string.Empty,
                    RestorationCliLocation = DatabaseConfig.RestorationCliLocation ?? string.Empty,
                    HostedByDocker = DatabaseConfig.HostedByDocker,
                    DatabaseName = DatabaseConfig.DatabaseName ?? string.Empty,
                    Port = DatabaseConfig.Port
                },
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
