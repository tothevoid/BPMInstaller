using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.WebRequestMethods;

namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Конфигурация приложения
    /// </summary>
    public class ApplicationConfig
    {
        /// <summary>
        /// Путь к папке с приложением
        /// </summary>
        public string ApplicationPath { get; set; }

        public bool FixAuthorizationCookies { get; set; }

        public int ApplicationPort { get; set; }

        public string AdminUserName { get; set; } = "Supervisor";

        public string AdminUserPassword { get; set; } = "Supervisor";

        public string ApplicationUrl => $"http://localhost:{ApplicationPort}";
    }
}
