using BPMInstaller.Core.Model;
using BPMInstaller.Core.Services.Database.MsSql;
using BPMInstaller.Core.Services.Database.Postgres;

namespace BPMInstaller.Core.Services
{
    public class ConfigValidator
    {
        public string ValidateDatabaseConnection(DatabaseType dbType, DatabaseConfig dbConfig)
        {
            // TODO: Migrate explicit db dependent service initialization to specific factory
            switch (dbType)
            {
                case DatabaseType.MsSql:
                    return new MsSqlDatabaseService(dbConfig).ValidateConnection();
                case DatabaseType.PostgreSql:
                    return new PostgresDatabaseService(dbConfig).ValidateConnection();
                default:
                    return string.Empty;
            }
        }

        public string ValidateRedisConnection(RedisConfig redisConfig) =>
            new RedisService().ValidateConnection(redisConfig);

        public string ValidateAppConfig(ApplicationConfig applicationConfig) =>
            applicationConfig.ApplicationPort == 0 ? "Порт должен быть отличен от нуля" : string.Empty;
    }
}
