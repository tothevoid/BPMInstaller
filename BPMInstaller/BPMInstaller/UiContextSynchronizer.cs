using System;
using System.Threading;
using System.Windows.Threading;
using BPMInstaller.UI.Desktop.Interfaces;

namespace BPMInstaller.UI.Desktop
{
    public class UiContextSynchronizer: IContextSynchronizer
    {
        private Dispatcher UiDispatcher { get; }

        public UiContextSynchronizer(Dispatcher dispatcher)
        {
            UiDispatcher = dispatcher;
        }

        public void InvokeSynced(Action action)
        {
            UiDispatcher.Invoke(action);
        }
    }
}
