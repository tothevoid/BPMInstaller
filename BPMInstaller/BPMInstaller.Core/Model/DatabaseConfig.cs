using BPMInstaller.Core.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public int Port { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        /// <summary>
        /// Путь к бекапу
        /// </summary>
        public string BackupPath { get; set; }

        /// <summary>
        /// Название БД
        /// </summary>
        public string DatabaseName { get; set; } = "bpm";

        public DatabaseMode DatabaseMode { get; set; } = DatabaseMode.NonDocker;
    }
}
