﻿using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BPMInstaller.UI.Model
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


            ActualizeTriggers();
        }

        public void ActualizeTriggers()
        {
            var configs = new List<BaseUIModel>()
            {
                ApplicationConfig,
                DatabaseConfig,
                RedisConfig,
                LicenseConfig
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


        // TODO: Fix architecture
        public Core.Model.InstallationConfig ConvertToCoreModel()
        {
            return new Core.Model.InstallationConfig
            {
                ApplicationConfig = new Core.Model.ApplicationConfig()
                {
                    ApplicationPath = ApplicationConfig.ApplicationPath ?? string.Empty,
                    FixAuthorizationCookies = ApplicationConfig.FixAuthorizationCookies,
                    ApplicationPort = ApplicationConfig.ApplicationPort
                },
                DatabaseConfig = new Core.Model.DatabaseConfig
                {
                    Host = DatabaseConfig.Host ?? string.Empty,
                    UserName = DatabaseConfig.UserName ?? string.Empty,
                    Password = DatabaseConfig.Password ?? string.Empty,
                    BackupPath = DatabaseConfig.BackupPath ?? string.Empty,
                    RestorationCliLocation = DatabaseConfig.RestorationCliLocation ?? string.Empty,
                    DatabaseMode = DatabaseConfig.IsDocker ? Core.Model.Enums.DatabaseMode.Docker : Core.Model.Enums.DatabaseMode.NonDocker,
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
                }
            };
        }
    }
}
