namespace BPMInstaller.Core.Model
{
    public class InstallationWorkflow
    {
        public bool UpdateApplicationPort {get;set;}
        public bool UpdateDatabaseConnectionString {get;set;}
        public bool RestoreDatabaseBackup {get;set;}
        public bool UpdateRedisConnectionString {get;set;}
        public bool InstallLicense {get;set;}
        public bool RemoveCertificate { get; set; }
        public bool DisableForcePasswordChange { get; set; }
        public bool CompileApplication { get; set; }
        public bool StartApplication { get; set; }
    }
}
