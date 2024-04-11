using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.UI.Desktop.Interfaces
{
    public interface IContextSynchronizer
    {
        void InvokeSynced(Action action);
    }
}
