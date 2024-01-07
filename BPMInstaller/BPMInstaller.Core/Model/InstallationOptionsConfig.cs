using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Model
{
    public class InstallationOptionsConfig
    {
        public bool RestoreBackup { get; init; }

        public bool AddLicense { get; init; }

        public bool FixAuthorizationCookies { get; init; }

        public bool FixUser { get; init; }
    }
}
