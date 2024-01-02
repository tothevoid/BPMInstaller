using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Конфигурация усттановки
    /// </summary>
    public class InstallationConfig
    {
        /// <inheritdoc cref="Model.ApplicationConfig"/>
        public ApplicationConfig ApplicationConfig { get; set; }

        /// <inheritdoc cref="Model.DatabaseConfig"/>
        public DatabaseConfig DatabaseConfig { get; set; }

        /// <inheritdoc cref="Model.RedisConfig"/>
        public RedisConfig RedisConfig { get; set; }

        /// <summary>
        /// Путь до дополнительных пакетов, которые будут поставлены после инициализации
        /// </summary>
        public string? AdditionalPackagesPath { get; set; }

        /// <inheritdoc cref="Core.LicenseConfig"/>
        public LicenseConfig? LicenseConfig { get; set; }
    }
}
