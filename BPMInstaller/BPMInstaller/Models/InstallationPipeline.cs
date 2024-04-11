using BPMInstaller.UI.Desktop.Models.Basics;

namespace BPMInstaller.UI.Desktop.Models
{
    /// <inheritdoc cref="Core.Model.InstallationPipeline"/>
    public class InstallationPipeline: ResponsiveModel
    {
        private bool updateApplicationPort = true;

        private bool updateDatabaseConnectionString = true;

        private bool updateRedisConnectionString = true;

        private bool disableForcePasswordChange = true;

        private bool fixCookies = true;

        private bool restoreDatabaseBackup;

        private bool installLicense;

        private bool removeCertificate;

        private bool compileApplication;

        private bool startApplication;

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.UpdateApplicationPort"/>
        /// </summary>
        public bool UpdateApplicationPort 
        { 
            get => updateApplicationPort;
            set => Set(ref updateApplicationPort, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.UpdateDatabaseConnectionString"/>
        /// </summary>
        public bool UpdateDatabaseConnectionString 
        { 
            get => updateDatabaseConnectionString;
            set => Set(ref updateDatabaseConnectionString, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.RestoreDatabaseBackup"/>
        /// </summary>
        public bool RestoreDatabaseBackup 
        { 
            get => restoreDatabaseBackup;
            set => Set(ref restoreDatabaseBackup, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.UpdateRedisConnectionString"/>
        /// </summary>
        public bool UpdateRedisConnectionString 
        { 
            get => updateRedisConnectionString;
            set => Set(ref updateRedisConnectionString, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.InstallLicense"/>
        /// </summary>
        public bool InstallLicense 
        { 
            get => installLicense;
            set { Set(ref installLicense, value); if (!StartApplication) StartApplication = true; } }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.RemoveCertificate"/>
        /// </summary>
        public bool RemoveCertificate 
        { 
            get => removeCertificate;
            set => Set(ref removeCertificate, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.DisableForcePasswordChange"/>
        /// </summary>
        public bool DisableForcePasswordChange 
        {
            get => disableForcePasswordChange;
            set => Set(ref disableForcePasswordChange, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.CompileApplication"/>
        /// </summary>
        public bool CompileApplication 
        { 
            get => compileApplication;
            set { Set(ref compileApplication, value); if (!StartApplication) StartApplication = true;  }
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.StartApplication"/>
        /// </summary>
        public bool StartApplication 
        { 
            get => startApplication;
            set => Set(ref startApplication, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.FixCookies"/>
        /// </summary>
        public bool FixCookies 
        { 
            get => fixCookies;
            set => Set(ref fixCookies, value);
        }

        public Core.Model.InstallationPipeline ToCoreModel()
        {
            return new Core.Model.InstallationPipeline
            {
                CompileApplication = CompileApplication,
                DisableForcePasswordChange = DisableForcePasswordChange,
                FixCookies = FixCookies,
                InstallLicense = InstallLicense,
                RemoveCertificate = RemoveCertificate,
                RestoreDatabaseBackup = RestoreDatabaseBackup,
                StartApplication = StartApplication,
                UpdateApplicationPort = UpdateApplicationPort,
                UpdateDatabaseConnectionString = UpdateDatabaseConnectionString,
                UpdateRedisConnectionString = UpdateRedisConnectionString
            };
        }
    }
}
