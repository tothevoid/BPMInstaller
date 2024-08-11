using BPMInstaller.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace BPMInstaller.Core.Services
{
    public class DistributiveStateService
    {
        private XmlDocument XmlConfig { get; }

        private DatabaseType databaseType;

        public DatabaseType DatabaseType => databaseType == DatabaseType.NotSpecified ? databaseType = GetDatabaseType() : databaseType;

        private ApplicationMode applicationMode;

        public ApplicationMode ApplicationMode => applicationMode == ApplicationMode.NotSpecified ? applicationMode = GetApplicationMode() : applicationMode;

        public DistributiveStateService(string applicationPath)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Path.Combine(applicationPath, "BPMSoft.WebHost.dll.config"));
            XmlConfig = doc;
        }

        public DatabaseType GetDatabaseType()
        {
            var dbConfig = XmlConfig.SelectSingleNode("configuration/bpmsoft/db/general");
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

        public ApplicationMode GetApplicationMode()
        {
            var useStaticFileContentSetting = XmlConfig.SelectSingleNode("configuration/appSettings[@name='UseStaticFileContent']");
            string staticContentStatus = useStaticFileContentSetting?.Attributes?.GetNamedItem("securityEngineType")?.Value ?? string.Empty;

            return staticContentStatus.Contains("true") ? ApplicationMode.FileSystem : ApplicationMode.Database;
        }
    }
}
