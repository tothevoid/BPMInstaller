namespace BPMInstaller.UI.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class LicenseConfig: BaseUIModel
    {
        private string? path;

        private long cId;

        public string? Path { get { return path; } set { Set(ref path, value); } }

        public long CId { get { return cId; } set { Set(ref cId, value); } }
    }
}
