using System;
using System.Windows.Threading;
using BPMInstaller.UI.Desktop.Interfaces;

namespace BPMInstaller.UI.Desktop.Models.Basics
{
    public class ViewModelContextSynchronizer : IContextSynchronizer
    {
        private Dispatcher UiDispatcher { get; }

        public ViewModelContextSynchronizer(Dispatcher dispatcher)
        {
            UiDispatcher = dispatcher;
        }

        public void InvokeSynced(Action action)
        {
            UiDispatcher.Invoke(action);
        }
    }
}
