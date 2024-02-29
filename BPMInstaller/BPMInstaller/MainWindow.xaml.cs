using BPMInstaller.Core.Services;
using BPMInstaller.UI.Desktop.Model;
using BPMInstaller.UI.Desktop.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace BPMInstaller.UI.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly string ConfigPath = "configurations.json";

        public ObservableCollection<string> Configurations { get; } = new ObservableCollection<string>();

        public InstallationConfig Config { get; set; }

        private ConfigValidator Validator { get; } = new ConfigValidator();

        public ControlsSessionState ControlsSessionState { get; set; } = new ControlsSessionState();

        public MainWindow()
        {
            InitializeComponent();

            if (File.Exists(ConfigPath))
            {
                var json = File.ReadAllText(ConfigPath);
                var configurations = JsonSerializer.Deserialize<IEnumerable<string>>(json) ?? Enumerable.Empty<string>();
                Configurations = new ObservableCollection<string>(configurations);

                if (Configurations.Any())
                {
                    var lastConfiguration = Configurations.LastOrDefault(config => !string.IsNullOrEmpty(config));
                    Config = new InstallationConfig { ApplicationPath = lastConfiguration };
                    LoadCurrentConfig();
                }
            }
            Config ??= new InstallationConfig();
            DataContext = this;
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            ControlsSessionState.Output.Clear();

            ControlsSessionState.StartButtonVisibility = Visibility.Collapsed;

            var handler = (Core.Model.InstallationMessage message) =>
            {
                if (message.IsTerminal)
                {
                    ControlsSessionState.StartButtonVisibility = Visibility.Visible;
                }

                Dispatcher.Invoke(() => {
                    ControlsSessionState.Output.Add(message);
                });
            };
               
            Task.Run(() => new Core.Services.InstallationService(handler).StartBasicInstallation(Config.ConvertToCoreModel()));
        }

        private void SaveConfig()
        {
            var json = JsonSerializer.Serialize(Configurations);
            File.WriteAllText(ConfigPath, json);
        }
        
        private void SelectDistributivePath(object sender, RoutedEventArgs e)
        {
            var newPath = InteractionUtilities.ShowFileSystemDialog(true, Config.ApplicationPath);
            var pathChanged = newPath != Config.ApplicationPath;
            Config.ApplicationPath = newPath;
            
            if (pathChanged && !string.IsNullOrEmpty(Config.ApplicationPath) && !Configurations.Contains(Config.ApplicationPath))
            {
                Configurations.Add(Config.ApplicationPath);
                SaveConfig();
            }

            var initConnectionStrings = pathChanged || 
                InteractionUtilities.ShowConfirmationButton("Выбран идентичный дистрибутив",
                   "Проставить значения строк подключения из конфигурационных файлов?");

            
            if (initConnectionStrings)
            {
                LoadCurrentConfig();
            }
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

        #region File system selection handlers
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
        #endregion

        #region Validation handlers
        //TODO: Remove duplication
        private void ValidateRedis(object sender, RoutedEventArgs e)
        {
            if (ValidateRedisState.Fill == null || Config?.RedisConfig?.IsChanged == true)
            {
                ValidateConfig(() => Validator.ValidateRedisConnection(Config.RedisConfig.ToCoreModel()),
                   ValidateRedisState, Config.RedisConfig);
            }
        }

        private void ValidateDatabase(object sender, RoutedEventArgs e)
        {
            if (ValidateDatabaseState.Fill == null || Config?.DatabaseConfig?.IsChanged == true)
            {
                ValidateConfig(() => Validator.ValidateDatabaseConnection(Config.DatabaseConfig.ToCoreModel()),
                   ValidateDatabaseState, Config.DatabaseConfig);
            }
        }
        private void ValidateApplication(object sender, RoutedEventArgs e)
        {
            if (ValidateApplicationState.Fill == null || Config?.ApplicationConfig?.IsChanged == true)
            {
                ValidateConfig(() => Validator.ValidateAppConfig(Config.ApplicationConfig.ToCoreModel()), 
                    ValidateApplicationState, Config.ApplicationConfig);
            }
        }

        private void ValidateConfig(Func<string> validationHandler, System.Windows.Shapes.Rectangle stateElement, BaseUIModel model)
        {
            stateElement.Fill = new SolidColorBrush(Colors.Yellow);
            Task.Run(() =>
            {
                var validationResult = validationHandler();
                Dispatcher.Invoke(() =>
                {
                    stateElement.ToolTip = validationResult;
                    stateElement.Fill = new SolidColorBrush(string.IsNullOrEmpty(validationResult) ? Colors.Green : Colors.Red);
                });
                model.CommitChanges();
            });
        }
        #endregion

        private void OpenConfig(object sender, RoutedEventArgs e)
        {
            var pressedButton = sender as Button;
            if (pressedButton?.Tag is string processedButtonTag)
            {
                Config = new InstallationConfig { ApplicationPath = processedButtonTag };
                Config.ActualizeTriggers();
                LoadCurrentConfig();
            }
        }
    }
}
