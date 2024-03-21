namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Конфигурация установки
    /// </summary>
    public class InstallationConfig
    {
        /// <summary>
        /// Путь к папке с приложением
        /// </summary>
        public string ApplicationPath { get; init; }

        public InstallationConfig(string applicationPath, ApplicationConfig applicationConfig,
            DatabaseConfig databaseConfig, RedisConfig redisConfig, InstallationPipeline pipeline)
        {
            if (string.IsNullOrEmpty(applicationPath))
            {
                throw new ArgumentNullException(nameof(applicationPath));
            }

            ApplicationPath = applicationPath;
            ApplicationConfig = applicationConfig ?? throw new ArgumentNullException(nameof(applicationConfig));
            DatabaseConfig = databaseConfig ?? throw new ArgumentNullException(nameof(databaseConfig));
            RedisConfig = redisConfig ?? throw new ArgumentNullException(nameof(redisConfig));
            Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        }

        /// <summary>
        /// Путь к dll для запуска приложения
        /// </summary>
        public string ExecutableApplicationPath => !string.IsNullOrEmpty(ApplicationPath) ?
            Path.Combine(ApplicationPath, "BPMSoft.WebHost.dll") :
            string.Empty;

        /// <inheritdoc cref="Model.ApplicationConfig"/>
        public ApplicationConfig ApplicationConfig { get; init; }

        /// <inheritdoc cref="Model.DatabaseConfig"/>
        public DatabaseConfig DatabaseConfig { get; init; }

        /// <inheritdoc cref="Model.RedisConfig"/>
        public RedisConfig RedisConfig { get; init; }

        /// <inheritdoc cref="InstallationPipeline"/>
        public InstallationPipeline Pipeline { get; init; }

        #region Optional

        /// <inheritdoc cref="Model.LicenseConfig"/>
        public LicenseConfig? LicenseConfig { get; init; }

        /// <inheritdoc cref="Model.BackupRestorationConfig"/>
        public BackupRestorationConfig? BackupRestorationConfig { get; init; }

        #endregion
    }
}
