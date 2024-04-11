using BPMInstaller.UI.Desktop.Models.Basics;

namespace BPMInstaller.UI.Desktop.Models.Configs
{
    /// <inheritdoc cref="Core.Model.RedisConfig"/>
    public class RedisConfig : ResponsiveModel
    {
        private string? host = "localhost";

        private int port = 6379;

        private int dbNumber = 1;

        /// <inheritdoc cref="Core.Model.RedisConfig.Host"/>
        public string? Host
        {
            get => host;
            set => Set(ref host, value);
        }

        /// <inheritdoc cref="Core.Model.RedisConfig.Port"/>
        public int Port
        {
            get => port;
            set => Set(ref port, value);
        }

        /// <inheritdoc cref="Core.Model.RedisConfig.DbNumber"/>
        public int DbNumber
        {
            get => dbNumber;
            set => Set(ref dbNumber, value);
        }

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
                Host = Host ?? string.Empty,
                Port = Port,
                DbNumber = DbNumber
            };
        }
    }
}
