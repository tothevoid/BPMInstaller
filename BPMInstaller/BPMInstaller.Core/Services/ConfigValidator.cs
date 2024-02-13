using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
