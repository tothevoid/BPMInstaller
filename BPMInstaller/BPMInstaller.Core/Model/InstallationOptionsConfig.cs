namespace BPMInstaller.Core.Model
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
    }
}
