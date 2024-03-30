using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Services
{
    public static class ApplicationApiUtilities
    {
        private const string ApplicationHost = "http://localhost";

        public static string GetAPIUrl(ushort port, string service, string method) =>
            $"{ApplicationHost}:{port}/ServiceModel/{service}.svc/{method}";
    }
}
