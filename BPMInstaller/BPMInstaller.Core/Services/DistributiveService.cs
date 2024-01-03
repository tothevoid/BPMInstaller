using BPMInstaller.Core.Model;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
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

            if (installationConfig.ApplicationConfig.ApplicationPort != 0)
            {
                UpdateApplicationPort(installationConfig.ApplicationConfig);
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

        private void UpdateApplicationPort(ApplicationConfig appConfig)
        {
            var appSettingsPath = Path.Combine(appConfig.ApplicationPath, "appsettings.json");
            var appSettingsJson = File.ReadAllText(appSettingsPath);

            var appSettings = JsonSerializer.Deserialize<AppSettings>(appSettingsJson);
            if (!string.IsNullOrEmpty(appSettings?.Kestrel?.Endpoints?.Http?.Url))
            {
                appSettings.Kestrel.Endpoints.Http.Url = $"http://::{appConfig.ApplicationPort}";
            }
            var updatedSettings = JsonSerializer.Serialize(appSettings, new JsonSerializerOptions { WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }).Replace(@"  ", "\t");
            File.WriteAllText(appSettingsPath, updatedSettings);
        }
    }

    public class AppSettings: PartialJson
    {
        [JsonPropertyName("Kestrel")]
        public KestrelConfig Kestrel { get; set; }

        public class KestrelConfig: PartialJson
        {
            [JsonPropertyName("Endpoints")]
            public EndpointConfig Endpoints { get; set; }

            public class EndpointConfig: PartialJson
            {
                [JsonPropertyName("Http")]
                public HttpConfig Http { get; set; }

                public class HttpConfig: PartialJson
                {
                    public string Url { get; set; }
                }
            }

        }
    }

    public class PartialJson
    {
        [System.Text.Json.Serialization.JsonExtensionDataAttribute]
        public IDictionary<string, object> ExtensionData { get; set; }
    }
}
