using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Services
{
    public class ConfigValidator
    {
        public bool ValidateDatabaseConnection(DatabaseConfig dbConfig) =>
            new PostgresDatabaseService(dbConfig).ValidateConnection();
    }
}
