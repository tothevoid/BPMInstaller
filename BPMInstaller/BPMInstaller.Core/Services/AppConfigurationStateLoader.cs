using BPMInstaller.Core.Model;

namespace BPMInstaller.Core.Services
{
    public class AppConfigurationStateLoader
    {
        public (DatabaseConfig DatabaseConfig, RedisConfig RedisConfig, ApplicationConfig ApplicationConfig) GetConfig(string applicationPath)
        {
            var distributiveService = new DistributiveService();
            var dbType = distributiveService.GetDatabaseType(applicationPath);
            var connectionString = distributiveService.GetConnectionStrings(applicationPath, dbType);
            var url = distributiveService.GetAppSettings(applicationPath).Settings?.Kestrel?.Endpoints?.Http?.Url ?? string.Empty;
            var portParts = url.Reverse().TakeWhile(symbol => char.IsDigit(symbol)).Reverse();
            var port = ushort.Parse(string.Join("", portParts));


            return (connectionString.DatabaseConfig, connectionString.RedisConfig, new ApplicationConfig
            {
                ApplicationPort = port
            });
        }
        
    }
}
