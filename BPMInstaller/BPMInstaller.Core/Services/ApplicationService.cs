using BPMInstaller.Core.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Services
{
    public class ApplicationService
    {
        public void RunApplication(ApplicationConfig applicationConfig)
        {
            //TODO: handle not installed core 3.1 exception
            Process process = new Process();
            process.StartInfo.WorkingDirectory = applicationConfig.ApplicationPath;
            process.StartInfo.FileName = $"dotnet";
            process.StartInfo.Arguments = "BPMSoft.WebHost.dll";
            process.StartInfo.UseShellExecute = false;
            process.Start();
        }
    }
}
