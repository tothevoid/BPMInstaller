using BPMInstaller.Core.Model;

namespace BPMInstaller.UI.Desktop.Model
{
    /// <inheritdoc cref="Core.Model.RedisConfig"/>
    public class RedisConfig: BaseUIModel
    {
        private string? host = "localhost";

        private int port = 6379;

        private int dbNumber = 1;

        /// <inheritdoc cref="Core.Model.ApplicationConfig.Host"/>
        public string? Host { get { return host; } set { Set(ref host, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.Port"/>
        public int Port { get { return port; } set { Set(ref port, value); } }

        /// <inheritdoc cref="Core.Model.ApplicationConfig.DbNumber"/>
        public int DbNumber { get { return dbNumber; } set { Set(ref dbNumber, value); } }
       
        public void MergeConfig(Core.Model.RedisConfig redisConfig)
        {
            Host = redisConfig.Host;
            Port = redisConfig.Port;
            DbNumber = redisConfig.DbNumber;
        }

        public Core.Model.RedisConfig ToCoreModel()
        {
            return new Core.Model.RedisConfig
            {
                Host = this.Host,
                Port = this.Port,
                DbNumber = this.DbNumber
            };
        }
    }
}
