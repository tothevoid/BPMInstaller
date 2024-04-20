using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Interfaces
{
    public interface ICommandLineExecutor
    {
        public (string Output, string Error) Execute();

        public bool Execute(Action<string, bool> asyncHandler);
    }
}
