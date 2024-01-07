namespace BPMInstaller.UI.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class DatabaseConfig: BaseUIModel
    {
        private string? host = "localhost";

        private int port = 5432;

        private string? userName = "postgres";

        private string? password;

        private string? backupPath;

        private string? databaseName = "bpm";

        private bool hostedByDocker = true;

        private string? restorationCliLocation;

        public string? Host { get { return host; } set { Set(ref host, value); } }

        public int Port { get { return port; } set { Set(ref port, value); } }

        public string? UserName { get { return userName; } set { Set(ref userName, value); } }

        public string? Password { get { return password; } set { Set(ref password, value); } }

        public string? BackupPath { get { return backupPath; } set { Set(ref backupPath, value); } }

        public string? DatabaseName { get { return databaseName; } set { Set(ref databaseName, value); } }

        public bool HostedByDocker { get { return hostedByDocker; } set { Set(ref hostedByDocker, value); } }

        public string? RestorationCliLocation { get { return restorationCliLocation; } set { Set(ref restorationCliLocation, value); } }
    }
}
