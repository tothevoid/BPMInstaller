using BPMInstaller.Core.Model;
using System;
using System.Data;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;

namespace BPMInstaller.Core.Services
{
    public class DistributiveService
    {
        public DatabaseType GetDatabaseType(string applicationPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(applicationPath, "BPMSoft.WebHost.dll.config"));
            var dbConfig = doc.SelectSingleNode("configuration/bpmsoft/db/general");
            string securityEngine = dbConfig?.Attributes?.GetNamedItem("securityEngineType")?.Value ?? string.Empty;

            if (securityEngine.Contains("DB.PostgreSql"))
            {
                return DatabaseType.PostgreSql;
            }
            else if (securityEngine.Contains("DB.MSSql"))
            {
                return DatabaseType.MsSql;
            }
            else
            {
                throw new NotImplementedException(securityEngine);
            }
        }

        public void UpdateConnectionStrings(string applicationPath, DatabaseType dbType, DatabaseConfig databaseConfig = null, 
            RedisConfig redisConfig = null)
        {
            var connectionStringsPath = GetConnectionStringsPath(applicationPath);
            var rootNode = GetConnectionString(connectionStringsPath);

            UpdateDatabaseConfig(databaseConfig, GetDatabaseString(rootNode.Configs), dbType);
            UpdateRedisConfig(redisConfig, GetRedisString(rootNode.Configs));

            rootNode.Document.Save(connectionStringsPath);
        }

        public (DatabaseConfig DatabaseConfig, RedisConfig RedisConfig) GetConnectionStrings(string applicationPath, DatabaseType dbType)
        {
            var connectionStringsPath = GetConnectionStringsPath(applicationPath);
            var rootNode = GetConnectionString(connectionStringsPath);

            return (
                //TODO: Handle case-insensetive namings
                ParseDatabaseConnectionString(GetDatabaseString(rootNode.Configs)?.Value ?? string.Empty, dbType),
                ParseRedisConnectionString(GetRedisString(rootNode.Configs)?.Value ?? string.Empty)
            );
        }

        public (string SettingsPath, AppSettings Settings) GetAppSettings(string applicationPath)
        {
            var appSettingsPath = Path.Combine(applicationPath, "appsettings.json");
            var appSettingsJson = File.ReadAllText(appSettingsPath);
            return (appSettingsPath, JsonSerializer.Deserialize<AppSettings>(appSettingsJson) ?? new AppSettings());
        }

        private DatabaseConfig ParseDatabaseConnectionString(string connectionString, DatabaseType dbType)
        {
            if (string.IsNullOrEmpty(connectionString))
            {
                return new DatabaseConfig();
            }

            var config = ParseKeyValuesString(connectionString);

            switch (dbType)
            {
                case DatabaseType.MsSql:
                    return ParseMsConfig(config);
                case DatabaseType.PostgreSql:
                    return ParsePostgresConfig(config);
                default:
                    throw new NotImplementedException(dbType.ToString());
            }
            
        }

        private DatabaseConfig ParsePostgresConfig(Dictionary<string, string> config)
        {
            return new DatabaseConfig
            {
                Host = config.ContainsKey(nameof(DatabaseConfig.Host)) ? config[nameof(DatabaseConfig.Host)] : string.Empty,
                Port = config.ContainsKey(nameof(DatabaseConfig.Port)) ? ushort.Parse(config[nameof(DatabaseConfig.Port)]) : default,
                AdminUserName = config.ContainsKey("Username") ? config["Username"] : string.Empty,
                AdminPassword = config.ContainsKey("Password") ? config["Password"] : string.Empty,
                DatabaseName = config.ContainsKey("Database") ? config["Database"] : string.Empty
            };
        }

        private DatabaseConfig ParseMsConfig(Dictionary<string, string> config)
        {
            var host = config.ContainsKey("Data Source") ? config["Data Source"] : string.Empty;
            ushort port = 1433; //default ms port
            if (!string.IsNullOrEmpty(host))
            {
                var hostParts = host.Split(",");
                if (hostParts.Length == 2)
                {
                    host = hostParts[0].Trim();
                    port = Convert.ToUInt16(hostParts[1].Trim());
                }
            }

            return new DatabaseConfig
            {
                Host = host,
                Port = port,
                AdminUserName = config.ContainsKey("User ID") ? config["User ID"] : string.Empty,
                AdminPassword = config.ContainsKey("Password") ? config["Password"] : string.Empty,
                DatabaseName = config.ContainsKey("Initial Catalog") ? config["Initial Catalog"] : string.Empty
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

        private void UpdateDatabaseConfig(DatabaseConfig databaseConfig, XmlAttribute attribute, DatabaseType dbType)
        {
            attribute.Value = GetConnectionStringByDbType(databaseConfig, dbType);
        }

        private string GetConnectionStringByDbType(DatabaseConfig dbConfig, DatabaseType dbType)
        {
            switch (dbType)
            {
                case DatabaseType.MsSql:
                    return FormatMsConnectionString(dbConfig);
                case DatabaseType.PostgreSql:
                    return FormatPostgresConnectionString(dbConfig);
                default:
                    throw new NotImplementedException(dbType.ToString());
            }

        }

        // TODO: Migrate logic to specific db classes
        private string FormatMsConnectionString(DatabaseConfig dbConfig)
        {
            return $"Data Source={dbConfig.Host},{dbConfig.Port};" +
                   $"Initial Catalog={dbConfig.DatabaseName};" +
                   $"User ID={dbConfig.AdminUserName};" +
                   $"Password={dbConfig.AdminPassword};" +
                   "Pooling = true;" +
                   "Max Pool Size = 1000;" +
                   "Connection Timeout=600" +
                   "Persist Security Info=True;" +
                   "MultipleActiveResultSets=True;";
        }

        private string FormatPostgresConnectionString(DatabaseConfig dbConfig)
        {
            return $"Database={dbConfig.DatabaseName};" +
                   $"Host={dbConfig.Host};" +
                   $"Port={dbConfig.Port};" +
                   $"Username={dbConfig.AdminUserName};" +
                   $"Password={dbConfig.AdminPassword};" +
                   "Timeout=500;" +
                   "Command Timeout=400" +
                   "Pooling=True;";
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
