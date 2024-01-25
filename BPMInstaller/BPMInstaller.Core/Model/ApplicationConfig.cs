﻿namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Конфигурация приложения
    /// </summary>
    public class ApplicationConfig
    {
        /// <summary>
        /// Путь к папке с приложением
        /// </summary>
        public string ApplicationPath { get; set; }

        /// <summary>
        /// Порт локального хоста, на котором будет развернуто приложение
        /// </summary>
        public int ApplicationPort { get; set; }
        
        /// <summary>
        /// URL приложения
        /// </summary>
        public string ApplicationUrl => $"http://localhost:{ApplicationPort}";
    }
}
