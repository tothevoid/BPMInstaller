using BPMInstaller.Core.Enums;

namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Конфигурация БД
    /// </summary>
    public class DatabaseConfig
    {
        /// <summary>
        /// Хост
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Порт
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Название создаваемой БД
        /// </summary>
        /// TODO: Add FS names validation for backup cases
        public string DatabaseName { get; set; }

        /// <summary>
        /// Логин админа БД
        /// </summary>
        public string AdminUserName { get; set; }

        /// <summary>
        /// Пароль админа БД
        /// </summary>
        public string AdminPassword { get; set; }
    }
}
