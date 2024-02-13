namespace BPMInstaller.UI.Desktop.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class ApplicationConfig: BaseUIModel
    {
        private ushort port = 5001;

        private string host = "localhost";

        /// <inheritdoc cref="Core.Model.ApplicationConfig.ApplicationPort"/
        public ushort Port { get { return port; } set { Set(ref port, value); } }

        public string Host { get { return host; } set { Set(ref host, value); } }

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
