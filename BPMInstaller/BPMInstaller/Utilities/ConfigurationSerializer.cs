using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BPMInstaller.UI.Desktop.Utilities
{
    public class ConfigurationSerializer
    {
        private const string ConfigPath = "configurations.json";

        public void SaveLocations(IEnumerable<string> locations)
        {
            var json = JsonSerializer.Serialize(locations);
            File.WriteAllText(ConfigPath, json);
        }

        public IEnumerable<string> LoadLocations()
        {
            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                return JsonSerializer.Deserialize<IEnumerable<string>>(json) ?? Enumerable.Empty<string>();
            }

            return Enumerable.Empty<string>();
        }
    }
}
