using System;
using System.Linq;
using BPMInstaller.UI.Desktop.Models.Basics;

namespace BPMInstaller.UI.Desktop.Models.Configs
{
    /// <inheritdoc cref="Core.Model.InstallationPipeline"/>
    public class InstallationPipeline : ResponsiveModel
    {
        public InstallationPipeline()
        {
            var selectedSteps = GetType().GetProperties().Sum(prop =>
                prop.PropertyType == typeof(bool) ? Convert.ToInt32((bool)prop.GetValue(this, null)) : 0);
            TotalSteps = selectedSteps - 1;
        }

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

        private bool switchApplicationMode;

        private int totalSteps = 0;

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.UpdateApplicationPort"/>
        /// </summary>
        public bool UpdateApplicationPort
        {
            get => updateApplicationPort;
            set => SetStep(ref updateApplicationPort, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.UpdateDatabaseConnectionString"/>
        /// </summary>
        public bool UpdateDatabaseConnectionString
        {
            get => updateDatabaseConnectionString;
            set => SetStep(ref updateDatabaseConnectionString, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.RestoreDatabaseBackup"/>
        /// </summary>
        public bool RestoreDatabaseBackup
        {
            get => restoreDatabaseBackup;
            set => SetStep(ref restoreDatabaseBackup, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.UpdateRedisConnectionString"/>
        /// </summary>
        public bool UpdateRedisConnectionString
        {
            get => updateRedisConnectionString;
            set => SetStep(ref updateRedisConnectionString, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.InstallLicense"/>
        /// </summary>
        public bool InstallLicense
        {
            get => installLicense;
            set { SetStep(ref installLicense, value); if (!StartApplication) StartApplication = true; }
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.RemoveCertificate"/>
        /// </summary>
        public bool RemoveCertificate
        {
            get => removeCertificate;
            set => SetStep(ref removeCertificate, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.DisableForcePasswordChange"/>
        /// </summary>
        public bool DisableForcePasswordChange
        {
            get => disableForcePasswordChange;
            set => SetStep(ref disableForcePasswordChange, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.CompileApplication"/>
        /// </summary>
        public bool CompileApplication
        {
            get => compileApplication;
            set { SetStep(ref compileApplication, value); if (!StartApplication) StartApplication = true; }
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.StartApplication"/>
        /// </summary>
        public bool StartApplication
        {
            get => startApplication;
            set => SetStep(ref startApplication, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.FixCookies"/>
        /// </summary>
        public bool FixCookies
        {
            get => fixCookies;
            set => SetStep(ref fixCookies, value);
        }

        /// <summary>
        /// <inheritdoc cref="Core.Model.InstallationPipeline.SwitchApplicationMode"/>
        /// </summary>
        public bool SwitchApplicationMode
        {
            get => switchApplicationMode;
            set => SetStep(ref switchApplicationMode, value);
        }

        public int TotalSteps
        {
            get => totalSteps;
            set => Set(ref totalSteps, value);
        }

        protected void SetStep(ref bool field, bool value, string? propName = null)
        {
            base.Set(ref field, value, propName);
            TotalSteps += value ? 1 : -1;
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
                UpdateRedisConnectionString = UpdateRedisConnectionString,
                SwitchApplicationMode = SwitchApplicationMode,
                TotalSteps = TotalSteps
            };
        }
    }
}
