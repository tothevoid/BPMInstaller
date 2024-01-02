using BPMInstaller.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BPMInstaller.Core.Services
{
    public class DistributiveService
    {
        public void ActualizeAppComponentsConfig(InstallationConfig installationConfig)
        {
            XmlDocument doc = new XmlDocument();
            var elementPath = Path.Combine(installationConfig.ApplicationConfig.ApplicationPath, "ConnectionStrings.config");
            doc.Load(elementPath);
            var rootNode = doc.GetElementsByTagName("connectionStrings")[0];
            UpdateDatabaseConfig(installationConfig.DatabaseConfig, rootNode);
            UpdateRedisConfig(installationConfig.RedisConfig, rootNode);

            if (installationConfig.ApplicationConfig.FixAuthorizationCookies)
            {
                FixAuthorizationCookies(installationConfig.ApplicationConfig);
            }

            doc.Save(elementPath);
        }

        private void UpdateDatabaseConfig(DatabaseConfig databaseConfig, XmlNode rootNode)
        {
            var dbSetting = rootNode.SelectSingleNode("add[@name='db']");
            dbSetting.Attributes[1].Value = $"Pooling=True;Database={databaseConfig.DatabaseName};Host={databaseConfig.Host};" +
                $"Port={databaseConfig.Port};Username={databaseConfig.UserName};Password={databaseConfig.Password};Timeout=500;Command Timeout=400";
        }

        private void UpdateRedisConfig(RedisConfig redisConfig, XmlNode rootNode)
        {
            var dbSetting = rootNode.SelectSingleNode("add[@name='redis']");
            dbSetting.Attributes[1].Value = $"host={redisConfig.Host};db={redisConfig.DbNumber};port={redisConfig.Port}";
        }

        private void FixAuthorizationCookies(ApplicationConfig appConfig)
        {
            XmlDocument doc = new XmlDocument();
            var elementPath = Path.Combine(appConfig.ApplicationPath, "BPMSoft.WebHost.dll.config");
            doc.Load(elementPath);
            var rootNode = doc.SelectSingleNode("configuration/appSettings");

            var dbSetting = rootNode.SelectSingleNode("add[@key='CookiesSameSiteMode']");
            dbSetting.Attributes[1].Value = $"Lax";
            doc.Save(elementPath);
        }
    }
}
