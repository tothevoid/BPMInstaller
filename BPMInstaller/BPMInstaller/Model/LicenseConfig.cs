namespace BPMInstaller.UI.Desktop.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class LicenseConfig: BaseUIModel
    {
        private string? path;

        private long cId;

        /// <inheritdoc cref="Core.Model.ApplicationConfig.Path"/>
        public string? Path { get { return path; } set { Set(ref path, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.CId"/>
        public long CId { get { return cId; } set { Set(ref cId, value); } }
    }
}
