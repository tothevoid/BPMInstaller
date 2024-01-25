using System.Configuration;
using System.IO;

namespace BPMInstaller.UI.Desktop.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class ApplicationConfig: BaseUIModel
    {
        private string? applicationPath;

        private bool fixAuthorizationCookies = true;

        private ushort applicationPort = 5001;

        /// <inheritdoc cref="Core.Model.ApplicationConfig.ApplicationPath"/
        public string? ApplicationPath { get { return applicationPath; } set { Set(ref applicationPath, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.FixAuthorizationCookies"/
        public bool FixAuthorizationCookies { get { return fixAuthorizationCookies; } set { Set(ref fixAuthorizationCookies, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.ApplicationPort"/
        public ushort ApplicationPort { get { return applicationPort; } set { Set(ref applicationPort, value); } }

        public string? ValidateApplicationPath()
        {
            if (string.IsNullOrEmpty(applicationPath?.Trim()))
            {
                return "Путь до дистрибутива не может быть пустым";
            }
            else if (!Directory.Exists(applicationPath))
            {
                return "Указанной директории не существует";
            }
            else if (!File.Exists(Path.Combine(applicationPath, "BPMSoft.WebHost.dll")))
            {
                return "Указанный путь не является дистрибутивом";
            }
            return null;
        }
    }
}
