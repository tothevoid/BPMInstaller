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

        private string adminUserName = "Supervisor";

        private string adminUserPassword = "Supervisor";

        /// <inheritdoc cref="Core.Model.ApplicationConfig.ApplicationPath"/
        public string? ApplicationPath { get { return applicationPath; } set { Set(ref applicationPath, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.FixAuthorizationCookies"/
        public bool FixAuthorizationCookies { get { return fixAuthorizationCookies; } set { Set(ref fixAuthorizationCookies, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.ApplicationPort"/
        public ushort ApplicationPort { get { return applicationPort; } set { Set(ref applicationPort, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.AdminUserName"/
        public string AdminUserName { get { return adminUserName; } set { Set(ref adminUserName, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.AdminUserPassword"/
        public string AdminUserPassword { get { return adminUserPassword; } set { Set(ref adminUserPassword, value); } }

        public string? Validate(string propertyName)
        {
            switch (propertyName)
            {
                case nameof(AdminUserName):
                    return string.IsNullOrEmpty(adminUserName) ? "Имя суперпользователя не может быть пустым": null;
                case nameof(AdminUserPassword):
                    return string.IsNullOrEmpty(adminUserName) ? "Пароль суперпользователя не может быть пустым" : null;
                case nameof(ApplicationPath):
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
                default:
                    return null;
            }
        }
    }
}
