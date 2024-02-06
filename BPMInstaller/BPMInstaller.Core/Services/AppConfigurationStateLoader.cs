using BPMInstaller.Core.Model;

namespace BPMInstaller.Core.Services
{
    public class AppConfigurationStateLoaded
    {
        public (DatabaseConfig DatabaseConfig, RedisConfig RedisConfig, ApplicationConfig ApplicationConfig) GetConfig(string applicationPath)
        {
            var distributiveService = new DistributiveService();
            var connectionString = distributiveService.GetConnectionStrings(applicationPath);
            var url = distributiveService.GetAppSettings(applicationPath)?.Kestrel?.Endpoints?.Http?.Url ?? string.Empty;
            var portParts = url.Reverse().TakeWhile(symbol => char.IsDigit(symbol)).Reverse();
            var port = ushort.Parse(string.Join("", portParts));


            return (connectionString.DatabaseConfig, connectionString.RedisConfig, new ApplicationConfig
            {
                ApplicationPort = port
            });
        }
        
    }
}
