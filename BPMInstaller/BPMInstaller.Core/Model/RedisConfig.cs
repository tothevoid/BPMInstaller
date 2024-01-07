using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Конфигурация взаимодействия с Redis
    /// </summary>
    public class RedisConfig
    {
        /// <summary>
        /// Хост
        /// </summary>
        public string Host { get; set; }

        /// <summary>
        /// Порт
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Номер БД
        /// </summary>
        public int DbNumber { get; set; }
    }
}
