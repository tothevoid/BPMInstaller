using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Конфигурация лицензии
    /// </summary>
    public class LicenseConfig
    {
        /// <summary>
        /// Путь к файлу с лицензией
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// CID лицензии
        /// </summary>
        public string CId { get; set; }
    }
}
