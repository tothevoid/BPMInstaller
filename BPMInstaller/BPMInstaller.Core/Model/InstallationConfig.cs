namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Конфигурация установки
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
