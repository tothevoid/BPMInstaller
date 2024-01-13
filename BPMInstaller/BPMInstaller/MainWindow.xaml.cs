using BPMInstaller.UI.Model;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
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
            Config.ApplicationConfig.ApplicationPath = ShowFileSystemDialog(false, Config.ApplicationConfig.ApplicationPath);
        }

        private void SelectBackupFile(object sender, RoutedEventArgs e)
        {
            Config.DatabaseConfig.BackupPath = ShowFileSystemDialog(false, Config.DatabaseConfig.BackupPath);
        }

        private void SelectCliPath(object sender, RoutedEventArgs e)
        {
            Config.DatabaseConfig.RestorationCliLocation = ShowFileSystemDialog(true, Config.DatabaseConfig.RestorationCliLocation);
        }

        private void SelectLicenseFile(object sender, RoutedEventArgs e)
        {
            Config.LicenseConfig.Path = ShowFileSystemDialog(false, Config.LicenseConfig.Path);
        }

        private string? ShowFileSystemDialog(bool isDirectory, string? previousValue)
        {
            var dlg = new CommonOpenFileDialog();
            dlg.Title = isDirectory ? "Выбор директории": "Выбора файла";
            dlg.IsFolderPicker = isDirectory;

            dlg.AllowNonFileSystemItems = false;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;

            return dlg.ShowDialog() == CommonFileDialogResult.Ok && !string.IsNullOrEmpty(dlg?.FileName) ?
                 dlg.FileName:
                 previousValue;
        }
    }
}
