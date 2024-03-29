﻿namespace BPMInstaller.Core.Model
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
        public long CId { get; set; }
    }
}
