namespace BPMInstaller.UI.Desktop.Model
{
    /// <inheritdoc cref="Core.Model.InstallationOptionsConfig"/>
    public class InstallationOptionsConfig
    {
        /// <inheritdoc cref="Core.Model.InstallationOptionsConfig.RestoreBackup"/>
        public bool RestoreBackup { get; init; }

        /// <inheritdoc cref="Core.Model.InstallationOptionsConfig.AddLicense"/>
        public bool AddLicense { get; init; }

        /// <inheritdoc cref="Core.Model.InstallationOptionsConfig.StartCompilation"/>
        public bool StartCompilation { get; init; }

        /// <inheritdoc cref="Core.Model.InstallationOptionsConfig.ActualizeAppSettings"/>
        public bool ActualizeAppSettings { get; init; }

        /// <inheritdoc cref="Core.Model.InstallationOptionsConfig.CompileApplication"/>
        public bool CompileApplication { get; init; }

        /// <inheritdoc cref="Core.Model.InstallationOptionsConfig.DisableForcePasswordChange"/>
        public bool DisableForcePasswordChange { get; init; }
        
    }
}
