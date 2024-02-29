using System.Drawing;

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
        public string Content { get; init; }

        /// <summary>
        /// Дата сообщения
        /// </summary>
        public string Date { get; } = DateTime.Now.ToString("HH:mm:ss");

        /// <summary>
        /// Ошибка
        /// </summary>
        public bool IsError { get; set; } = false;

        /// <summary>
        /// Сообщение, завершающее установку
        /// </summary>
        public bool IsTerminal { get; init; } = false;
    }
}
