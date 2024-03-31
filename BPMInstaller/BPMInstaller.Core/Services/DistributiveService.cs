using BPMInstaller.Core.Model;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace BPMInstaller.Core.Services
{
    public class DistributiveService
    {
        public void UpdateConnectionStrings(string applicationPath, DatabaseConfig databaseConfig = null, 
            RedisConfig redisConfig = null)
        {
            var connectionStringsPath = GetConnectionStringsPath(applicationPath);
            var rootNode = GetConnectionString(connectionStringsPath);

            UpdateDatabaseConfig(databaseConfig, GetDatabaseString(rootNode.Configs));
            UpdateRedisConfig(redisConfig, GetRedisString(rootNode.Configs));

            rootNode.Document.Save(connectionStringsPath);
        }

        public (DatabaseConfig DatabaseConfig, RedisConfig RedisConfig) GetConnectionStrings(string applicationPath)
        {
            var connectionStringsPath = GetConnectionStringsPath(applicationPath);
            var rootNode = GetConnectionString(connectionStringsPath);

            return (
                //TODO: Handle case-insensetive namings
                ParseDatabaseConnectionString(GetDatabaseString(rootNode.Configs)?.Value ?? string.Empty),
                ParseRedisConnectionString(GetRedisString(rootNode.Configs)?.Value ?? string.Empty)
            );
        }

        public (string SettingsPath, AppSettings Settings) GetAppSettings(string applicationPath)
        {
            var appSettingsPath = Path.Combine(applicationPath, "appsettings.json");
            var appSettingsJson = File.ReadAllText(appSettingsPath);
            return (appSettingsPath, JsonSerializer.Deserialize<AppSettings>(appSettingsJson) ?? new AppSettings());
        }

        private DatabaseConfig ParseDatabaseConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return new DatabaseConfig();
            }

            var keyValues = ParseKeyValuesString(connectionString);

            return new DatabaseConfig
            {
                Host = keyValues.ContainsKey(nameof(DatabaseConfig.Host)) ? keyValues[nameof(DatabaseConfig.Host)] : string.Empty,
                Port = keyValues.ContainsKey(nameof(DatabaseConfig.Port)) ? ushort.Parse(keyValues[nameof(DatabaseConfig.Port)]): default,
                AdminUserName = keyValues.ContainsKey("Username") ? keyValues["Username"] : string.Empty,
                AdminPassword = keyValues.ContainsKey("Password") ? keyValues["Password"] : string.Empty,
                DatabaseName = keyValues.ContainsKey("Database") ? keyValues["Database"] : string.Empty
            };
        }

        private RedisConfig ParseRedisConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return new RedisConfig();
            }

            var keyValues = ParseKeyValuesString(connectionString);
            return new RedisConfig
            {
                Host = keyValues.ContainsKey("host") ? keyValues["host"] : string.Empty,
                Port = keyValues.ContainsKey("port") ? int.Parse(keyValues["port"]) : 0,
                DbNumber = keyValues.ContainsKey("db") ? int.Parse(keyValues["db"]) : 0
            };
        }

        private Dictionary<string, string> ParseKeyValuesString(string value)
        {
            return value.Split(";")
               .Select(keyValue => keyValue.Trim())
               .Where(keyValue => !string.IsNullOrEmpty(keyValue))
               .Select(keyValue => keyValue.Split("="))
               .ToDictionary(keyValue => keyValue[0].Trim(), keyValue => keyValue[1].Trim());
        }

        private string GetConnectionStringsPath(string applicationPath) =>
            Path.Combine(applicationPath, "ConnectionStrings.config");

        private (XmlDocument Document, XmlNode Configs) GetConnectionString(string connectionStringsPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(connectionStringsPath);
            return (doc, doc.GetElementsByTagName("connectionStrings")[0]);
        }

        public void UpdateApplicationPort(ApplicationConfig appConfig, string applicationPath)
        {
            var appSettings = GetAppSettings(applicationPath);
            var settings = appSettings.Settings;
            if (!string.IsNullOrEmpty(settings?.Kestrel?.Endpoints?.Http?.Url))
            {
                appSettings.Settings.Kestrel.Endpoints.Http.Url = $"http://::{appConfig.ApplicationPort}";
            }
            var updatedSettings = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            }).Replace(@"  ", "\t");
            File.WriteAllText(appSettings.SettingsPath, updatedSettings);
        }

        public void FixAuthorizationCookies(string applicationPath)
        {
            XmlDocument doc = new XmlDocument();
            var elementPath = Path.Combine(applicationPath, "BPMSoft.WebHost.dll.config");
            doc.Load(elementPath);
            var rootNode = doc.SelectSingleNode("configuration/appSettings");

            var dbSetting = rootNode.SelectSingleNode("add[@key='CookiesSameSiteMode']");
            dbSetting.Attributes[1].Value = $"Lax";
            doc.Save(elementPath);
        }

        private XmlAttribute GetDatabaseString(XmlNode rootNode)
        {
            var dbSetting = rootNode.SelectSingleNode("add[@name='db']");
            return dbSetting.Attributes[1];
        }

        private XmlAttribute GetRedisString(XmlNode rootNode)
        {
            var dbSetting = rootNode.SelectSingleNode("add[@name='redis']");
            return dbSetting.Attributes[1];
        }

        private void UpdateDatabaseConfig(DatabaseConfig databaseConfig, XmlAttribute attribute)
        {
            attribute.Value = $"Pooling=True;Database={databaseConfig.DatabaseName};Host={databaseConfig.Host};" +
                $"Port={databaseConfig.Port};Username={databaseConfig.AdminUserName};Password={databaseConfig.AdminPassword};Timeout=500;Command Timeout=400";
        }

        private void UpdateRedisConfig(RedisConfig redisConfig, XmlNode attribute)
        {
            attribute.Value = $"host={redisConfig.Host};db={redisConfig.DbNumber};port={redisConfig.Port}";
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
        [JsonExtensionData]
        public IDictionary<string, object> OtherData { get; set; }
    }
}
