namespace BPMInstaller.UI.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class ApplicationConfig: BaseUIModel
    {
        private string? applicationPath;

        private bool fixAuthorizationCookies = true;

        private int applicationPort = 5001;

        private string adminUserName = "Supervisor";

        private string adminUserPassword = "Supervisor";

        public string? ApplicationPath { get { return applicationPath; } set { Set(ref applicationPath, value); } }

        public bool FixAuthorizationCookies { get { return fixAuthorizationCookies; } set { Set(ref fixAuthorizationCookies, value); } }

        public int ApplicationPort { get { return applicationPort; } set { Set(ref applicationPort, value); } }

        public string AdminUserName { get { return adminUserName; } set { Set(ref adminUserName, value); } }

        public string AdminUserPassword { get { return adminUserPassword; } set { Set(ref adminUserPassword, value); } }

    }
}
