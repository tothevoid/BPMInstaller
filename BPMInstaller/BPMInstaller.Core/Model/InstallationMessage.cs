using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Model
{
    public class InstallationMessage
    {
        public string Content { get; init; }

        public bool IsTerminal { get; init; } = false;
    }
}
