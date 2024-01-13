namespace BPMInstaller.UI.Model
{
    /// <summary>
    /// Конфигурация установки
    /// </summary>
    public class InstallationOptionsConfig
    {
        /// <summary>
        /// Восстанавилвать бекап БД
        /// </summary>
        public bool RestoreBackup { get; init; }

        /// <summary>
        /// Устанавливать лицензии
        /// </summary>
        public bool AddLicense { get; init; }

        /// <summary>
        /// Запускать компиляцию
        /// </summary>
        public bool StartCompilation { get; init; }

        /// <summary>
        /// Актуаилизировать порт в AppSettings.json
        /// </summary>
        public bool ActualizeAppSettings { get; init; }

        /// <summary>
        /// Запустить компиляцию приложения
        /// </summary>
        public bool CompileApplication { get; init; }

        /// <summary>
        /// Отключить принудительную смену пароля
        /// </summary>
        public bool DisableForcePasswordChange { get; init; }
        
    }
}
