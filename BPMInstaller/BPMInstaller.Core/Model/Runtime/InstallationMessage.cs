namespace BPMInstaller.Core.Model.Runtime
{
    /// <summary>
    /// Сообщение о ходе установки
    /// </summary>
    public class InstallationMessage
    {
        /// <summary>
        /// Текст сообщения
        /// </summary>
        public string Text { get; init; }

        /// <summary>
        /// Дата сообщения
        /// </summary>
        public DateTime Date { get; init; }

        /// <summary>
        /// Ошибка
        /// </summary>
        public bool IsError { get; set; } = false;

        /// <summary>
        /// Сообщение, завершающее установку
        /// </summary>
        public bool IsTerminal { get; init; } = false;

        private InstallationMessage() { }

        public static InstallationMessage Error(string message) =>
            new InstallationMessage
            {
                Text = message,
                IsError = true,
                IsTerminal = true,
                Date = DateTime.Now
            };

        public static InstallationMessage Info(string message, bool isTerminal = false) =>
            new InstallationMessage
            {
                Text = message,
                IsTerminal = isTerminal,
                Date = DateTime.Now
            };
    }
}
