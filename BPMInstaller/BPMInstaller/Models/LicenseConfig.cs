using BPMInstaller.UI.Desktop.Models.Basics;

namespace BPMInstaller.UI.Desktop.Models
{
    /// <inheritdoc cref="Core.Model.LicenseConfig"/>
    public class LicenseConfig: ResponsiveModel
    {
        private string path = string.Empty;

        private long cId;

        /// <inheritdoc cref="Core.Model.LicenseConfig.Path"/>
        public string Path 
        { 
            get => path;
            set => Set(ref path, value);
        }

        /// <inheritdoc cref="Core.Model.LicenseConfig.CId"/>
        public long CId
        {
            get => cId;
            set => Set(field: ref cId, value);
        }

        public Core.Model.LicenseConfig ToCoreModel()
        {
            return new Core.Model.LicenseConfig
            {
                CId = CId,
                Path = Path
            };
        }
    }
}
