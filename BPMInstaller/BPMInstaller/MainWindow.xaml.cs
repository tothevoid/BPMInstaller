using BPMInstaller.UI.Model;
using System;
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
            Config.InstallationState.StartButtonVisibility = Visibility.Collapsed;

            var handler = (Core.Model.InstallationMessage message) => 
            {
                if (message.IsTerminal)
                {
                    Config.InstallationState.StartButtonVisibility = Visibility.Visible;
                }
                Dispatcher.Invoke(() => Output.Text += $"{Environment.NewLine} {message.Content}");
            };
               
            Task.Run(() => new Core.Services.InstallationService(handler).StartBasicInstallation(Config.ConvertToCoreModel()));
        }

        private void SaveConfig()
        {
            var json = JsonSerializer.Serialize(Config);
            File.WriteAllText(ConfigPath, json);
        }

    }
}
