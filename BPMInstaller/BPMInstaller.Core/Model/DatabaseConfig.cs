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
        /// Логин админа БД
        /// </summary>
        public string AdminUserName { get; set; }

        /// <summary>
        /// Пароль админа БД
        /// </summary>
        public string AdminPassword { get; set; }

        /// <summary>
        /// Путь к бекапу БД
        /// </summary>
        public string BackupPath { get; set; }

        /// <summary>
        /// Название создаваемой БД
        /// </summary>
        public string DatabaseName { get; set; }

        /// <summary>
        /// Способ восстановления БД
        /// </summary>
        public RestorationOption RestorationKind { get; set; }

        /// <summary>
        /// Путь до CLI восстановления бд, если она развёрнута не в контейнере
        /// </summary>
        public string? RestorationCliLocation { get; set; }
    }
}
