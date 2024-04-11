using BPMInstaller.UI.Desktop.Models.Basics;

namespace BPMInstaller.UI.Desktop.Models
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class ApplicationConfig: ResponsiveModel
    {
        private ushort port = 5001;

        private string host = "localhost";

        /// <inheritdoc cref="Core.Model.ApplicationConfig.ApplicationPort"/>
        public ushort Port 
        { 
            get => port;
            set => Set(ref port, value);
        }

        public string Host
        {
            get => host;
            set => Set(ref host, value);
        }

        public void MergeConfig(Core.Model.ApplicationConfig applicationConfig)
        {
            Port = applicationConfig.ApplicationPort;
        }

        public Core.Model.ApplicationConfig ToCoreModel()
        {
            return new Core.Model.ApplicationConfig
            {
                ApplicationPort = this.Port
            };
        }
    }
}
