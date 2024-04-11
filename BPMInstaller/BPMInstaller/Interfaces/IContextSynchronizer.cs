using System;

namespace BPMInstaller.UI.Desktop.Interfaces
{
    /// <summary>
    /// Синхронизатор контекста выполнения логики
    /// </summary>
    public interface IContextSynchronizer
    {
        /// <summary>
        /// Выполнение конкретного действия в основном потоке
        /// </summary>
        /// <param name="action">Выполняемое действие</param>
        void InvokeSynced(Action action);
    }
}
