namespace BPMInstaller.UI.Model
{
    /// <inheritdoc cref="Core.Model.ApplicationConfig"/>
    public class RedisConfig: BaseUIModel
    {
        private string? host = "localhost";

        private int port = 6379;

        private int dbNumber = 1;

        public string? Host { get { return host; } set { Set(ref host, value); } }

        public int Port { get { return port; } set { Set(ref port, value); } }

        public int DbNumber { get { return dbNumber; } set { Set(ref dbNumber, value); } }
    }
}
