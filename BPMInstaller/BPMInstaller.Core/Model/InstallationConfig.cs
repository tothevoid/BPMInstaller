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
        public ApplicationConfig ApplicationConfig { get; init; }

        /// <inheritdoc cref="Model.DatabaseConfig"/>
        public DatabaseConfig DatabaseConfig { get; init; }

        /// <inheritdoc cref="Model.RedisConfig"/>
        public RedisConfig RedisConfig { get; init; }

        /// <inheritdoc cref="Core.LicenseConfig"/>
        public LicenseConfig? LicenseConfig { get; init; }
    }
}
