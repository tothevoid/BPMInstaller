using BPMInstaller.Core.Services;
using BPMInstaller.UI.Desktop.Model;
using BPMInstaller.UI.Desktop.Utilities;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Configuration;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace BPMInstaller.UI.Desktop
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
                if (!string.IsNullOrEmpty(deserialized?.ApplicationPath) && deserialized.ValidateApplicationPath() != null)
                {
                    deserialized.ApplicationPath = string.Empty;
                }

                deserialized?.ActualizeTriggers();
                Config = deserialized ?? new InstallationConfig();
            }
            else
            {
                Config = new InstallationConfig();
            }
            Config.OnModelChanged += SaveConfig;
            DataContext = this;
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            ApplicationTabs.TabIndex = 1;

            Config.ControlsSessionState.Output.Clear();

            Config.ControlsSessionState.StartButtonVisibility = Visibility.Collapsed;

            var handler = (Core.Model.InstallationMessage message) =>
            {
                if (message.IsTerminal)
                {
                    Config.ControlsSessionState.StartButtonVisibility = Visibility.Visible;
                }

                Dispatcher.Invoke(() => {
                    Config.ControlsSessionState.Output.Add(message);
                });
            };
               
            Task.Run(() => new Core.Services.InstallationService(handler).StartBasicInstallation(Config.ConvertToCoreModel()));
        }

        private void SaveConfig()
        {
            var json = JsonSerializer.Serialize(Config);
            File.WriteAllText(ConfigPath, json);
        }
        
        private void SelectDistributivePath(object sender, RoutedEventArgs e)
        {
            var newPath = InteractionUtilities.ShowFileSystemDialog(true, Config.ApplicationPath);
            var initConnectionStrings = newPath == Config.ApplicationPath || 
                InteractionUtilities.ShowConfirmationButton("Выбран идентичный дистрибутив",
                   "Проставить значения строк подключения из конфигурационных файлов?");

            if (initConnectionStrings)
            {
                LoadCurrentConfig();
            }

            Config.ApplicationPath = newPath;
        }


        private void LoadCurrentConfig()
        {
            if (string.IsNullOrEmpty(Config?.ApplicationPath))
            {
                return;
            }

            var stateLoader = new AppConfigurationStateLoaded();
            var state = stateLoader.GetConfig(Config.ApplicationPath);
            Config.ApplicationConfig.MergeConfig(state.ApplicationConfig);
            Config.DatabaseConfig.MergeConfig(state.DatabaseConfig);
            Config.RedisConfig.MergeConfig(state.RedisConfig);
        }

        private void SelectBackupFile(object sender, RoutedEventArgs e)
        {
            Config.DatabaseConfig.BackupPath = InteractionUtilities
                .ShowFileSystemDialog(false, Config.DatabaseConfig.BackupPath);
        }

        private void SelectCliPath(object sender, RoutedEventArgs e)
        {
            Config.DatabaseConfig.RestorationCliLocation = InteractionUtilities
                .ShowFileSystemDialog(true, Config.DatabaseConfig.RestorationCliLocation);
        }

        private void SelectLicenseFile(object sender, RoutedEventArgs e)
        {
            Config.LicenseConfig.Path = InteractionUtilities
                .ShowFileSystemDialog(false, Config.LicenseConfig.Path);
        }
    }
}
