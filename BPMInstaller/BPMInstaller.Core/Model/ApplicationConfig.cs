namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Конфигурация приложения
    /// </summary>
    public class ApplicationConfig
    {
        /// <summary>
        /// Порт локального хоста, на котором будет развернуто приложение
        /// </summary>
        public ushort ApplicationPort { get; set; }

        /// <summary>
        /// URL приложения
        /// </summary>
        public string ApplicationUrl => $"http://localhost:{ApplicationPort}";
    }
}
