using BPMInstaller.Core.Model;
using BPMInstaller.Core.Services.Database.Postgres;

namespace BPMInstaller.Core.Services
{
    public class ConfigValidator
    {
        public string ValidateDatabaseConnection(DatabaseConfig dbConfig) =>
            new PostgresDatabaseService(dbConfig).ValidateConnection();

        public string ValidateRedisConnection(RedisConfig redisConfig) =>
            new RedisService().ValidateConnection(redisConfig);

        public string ValidateAppConfig(ApplicationConfig applicationConfig) =>
            applicationConfig.ApplicationPort == 0 ? "Порт должен быть отличен от нуля" : string.Empty;
    }
}
