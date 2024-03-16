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
        public string ApplicationPath { get; set; }

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

        /// <inheritdoc cref="Model.LicenseConfig"/>
        public LicenseConfig? LicenseConfig { get; init; }

        /// <inheritdoc cref="Model.InstallationWorkflow"/>
        public InstallationWorkflow? InstallationWorkflow { get; init;}
    }
}
