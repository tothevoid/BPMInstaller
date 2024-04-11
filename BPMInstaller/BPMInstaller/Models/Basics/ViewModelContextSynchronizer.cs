using System;
using System.Windows.Threading;
using BPMInstaller.UI.Desktop.Interfaces;

namespace BPMInstaller.UI.Desktop.Models.Basics
{
    /// <summary>
    /// Синхронизатор логики с потоком UI
    /// </summary>
    public class ViewModelContextSynchronizer : IContextSynchronizer
    {
        private Dispatcher UiDispatcher { get; }

        public ViewModelContextSynchronizer(Dispatcher dispatcher)
        {
            UiDispatcher = dispatcher;
        }

        /// <inheritdoc cref="IContextSynchronizer.InvokeSynced"/>
        public void InvokeSynced(Action action)
        {
            UiDispatcher.Invoke(action);
        }
    }
}
