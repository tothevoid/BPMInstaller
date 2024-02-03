using BPMInstaller.Core.Model;
using StackExchange.Redis;

namespace BPMInstaller.Core.Services
{
    public class RedisService
    {
        public void FlushData(RedisConfig redisConfig)
        {
            //Reuse this connection
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect($"{redisConfig.Host}:{redisConfig.Port},allowAdmin=true");
            var server = redis.GetServer($"{redisConfig.Host}:{redisConfig.Port}");
            server.FlushDatabase(redisConfig.DbNumber);
        }

    }
}
