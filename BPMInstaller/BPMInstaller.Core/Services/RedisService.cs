using BPMInstaller.Core.Model;
using StackExchange.Redis;

namespace BPMInstaller.Core.Services
{
    public class RedisService
    {
        public string ValidateConnection(RedisConfig redisConfig)
        {
            try
            {
                ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{redisConfig.Host}:{redisConfig.Port},allowAdmin=true");
                return String.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public void FlushData(RedisConfig redisConfig)
        {
            //Reuse this connection
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{redisConfig.Host}:{redisConfig.Port},allowAdmin=true");
            var server = redis.GetServer($"{redisConfig.Host}:{redisConfig.Port}");
            server.FlushDatabase(redisConfig.DbNumber);
        }

    }
}
