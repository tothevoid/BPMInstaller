using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Interfaces
{
    public interface IDatabaseService
    {
        public bool ValidateConnection();

        public bool CreateDatabase();

        public void RestoreDatabase();
    }
}
