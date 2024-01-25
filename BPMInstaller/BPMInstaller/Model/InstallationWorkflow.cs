namespace BPMInstaller.UI.Desktop.Model
{
    public class InstallationWorkflow: BaseUIModel
    {
        private bool updateApplicationPort;

        private bool updateDatabaseConnectionString;

        private bool restoreDatabaseBackup;

        private bool updateRedisConnectionString;

        private bool installLicense;

        private bool removeCertificate;

        private bool disableForcePasswordChange;

        private bool compileApplication;

        private bool startApplication;

        public bool UpdateApplicationPort { get { return updateApplicationPort; } set { Set(ref updateApplicationPort, value); } }

        public bool UpdateDatabaseConnectionString { get { return updateDatabaseConnectionString; } set { Set(ref updateDatabaseConnectionString, value); } }

        public bool RestoreDatabaseBackup { get { return restoreDatabaseBackup; } set { Set(ref restoreDatabaseBackup, value); } }

        public bool UpdateRedisConnectionString { get { return updateRedisConnectionString; } set { Set(ref updateRedisConnectionString, value); } }

        public bool InstallLicense { get { return installLicense; } set { Set(ref installLicense, value); if (!StartApplication) StartApplication = true; } }

        public bool RemoveCertificate { get { return removeCertificate; } set { Set(ref removeCertificate, value); } }

        public bool DisableForcePasswordChange { get { return disableForcePasswordChange; } set { Set(ref disableForcePasswordChange, value); } }

        public bool CompileApplication { get { return compileApplication; } set { Set(ref compileApplication, value); if (!StartApplication) StartApplication = true;  } }

        public bool StartApplication { get { return startApplication; } set { Set(ref startApplication, value); } }
    }
}
