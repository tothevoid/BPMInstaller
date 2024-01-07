using BPMInstaller.UI.Model;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace BPMInstaller
{
    public partial class MainWindow : Window
    {
        private readonly string ConfigPath = "Config.json";

        public InstallationConfig Config { get; init; }

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                var deserialized = JsonSerializer.Deserialize<InstallationConfig>(json);
                deserialized?.ActualizeTriggers();
                Config = deserialized ?? new InstallationConfig();
            }
            else
            {
                Config = new InstallationConfig();
            }
            Config.OnModelChanged += SaveConfig;
            DataContext = Config;
           
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            Task.Run(() => new BPMInstaller.Core.Services.InstallationService((string message) => Dispatcher.Invoke(() => Output.Text = message)).StartBasicInstallation(Config.ConvertToCoreModel()));
        }

        private void SaveConfig()
        {
            var json = JsonSerializer.Serialize(Config);
            File.WriteAllText(ConfigPath, json);
        }

    }
}
