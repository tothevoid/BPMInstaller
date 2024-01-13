﻿namespace BPMInstaller.UI.Desktop.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class ApplicationConfig: BaseUIModel
    {
        private string? applicationPath;

        private bool fixAuthorizationCookies = true;

        private int applicationPort = 5001;

        private string adminUserName = "Supervisor";

        private string adminUserPassword = "Supervisor";

        /// <inheritdoc cref="Core.Model.ApplicationConfig.ApplicationPath"/
        public string? ApplicationPath { get { return applicationPath; } set { Set(ref applicationPath, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.FixAuthorizationCookies"/
        public bool FixAuthorizationCookies { get { return fixAuthorizationCookies; } set { Set(ref fixAuthorizationCookies, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.ApplicationPort"/
        public int ApplicationPort { get { return applicationPort; } set { Set(ref applicationPort, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.AdminUserName"/
        public string AdminUserName { get { return adminUserName; } set { Set(ref adminUserName, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.AdminUserPassword"/
        public string AdminUserPassword { get { return adminUserPassword; } set { Set(ref adminUserPassword, value); } }

    }
}
