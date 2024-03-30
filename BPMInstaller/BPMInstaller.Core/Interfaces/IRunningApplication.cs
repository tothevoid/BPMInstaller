using BPMInstaller.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Interfaces
{
    public interface IRunningApplication
    {
        public void Compile();

        public void AddLicenses(LicenseConfig licenseConfig);

    }
}
