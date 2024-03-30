using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.Core.Services;
using BPMInstaller.UI.Desktop.Model;
using BPMInstaller.UI.Desktop.Utilities;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BPMInstaller.UI.Desktop
{
    // ReSharper disable once UnusedMember.Global
    public partial class MainWindow : Window
    {
        public ObservableCollection<string> Configurations { get; } = new ObservableCollection<string>();

        public InstallationConfig Config { get; set; }

        private ConfigValidator Validator { get; } = new ConfigValidator();

        private ConfigurationSerializer ConfigSerializer { get; set; } = new ConfigurationSerializer();

        public ControlsSessionState ControlsSessionState { get; set; } = new ControlsSessionState();

        public MainWindow()
        {
            InitializeComponent();

            var distributiveLocations = ConfigSerializer.LoadLocations().ToList();
            if (distributiveLocations.Any())
            {
                Configurations = new ObservableCollection<string>(distributiveLocations);
                var lastConfiguration = Configurations.LastOrDefault(config => !string.IsNullOrEmpty(config));
                Config = new InstallationConfig { ApplicationPath = lastConfiguration };
                LoadCurrentConfig();
            }

            Config ??= new InstallationConfig();
            DataContext = this;
        }

        private void Install(object sender, RoutedEventArgs e)
        {
            ControlsSessionState.Output.Clear();
            ControlsSessionState.StartButtonVisibility = Visibility.Collapsed;

            Task.Run(() =>
            {
                var logger = new InstallationLogger(AddLoggerMessage);
                new InstallationService(logger, Config.ConvertToCoreModel()).Install();
            });
        }

        private void AddLoggerMessage(InstallationMessage message)
        {
            if (message.IsTerminal)
            {
                ControlsSessionState.StartButtonVisibility = Visibility.Visible;
            }

            Dispatcher.Invoke(() => {
                ControlsSessionState.Output.Add(message);
            });
        }

        private void SelectDistributivePath(object sender, RoutedEventArgs e)
        {
            var selectionResult = InteractionUtilities.ShowFileSystemDialog(true, Config.ApplicationPath);
            if (!selectionResult.IsSelected)
            {
                return;
            }

            var pathChanged = selectionResult.Path != Config.ApplicationPath;
            Config.ApplicationPath = selectionResult.Path;
            
            if (pathChanged && !string.IsNullOrEmpty(Config.ApplicationPath) && !Configurations.Contains(Config.ApplicationPath))
            {
                Configurations.Add(Config.ApplicationPath);
                ConfigSerializer.SaveLocations(Configurations);
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
                .ShowFileSystemDialog(false, Config.DatabaseConfig.BackupPath).Path;
        }

        private void SelectCliPath(object sender, RoutedEventArgs e)
        {
            Config.DatabaseConfig.RestorationCliLocation = InteractionUtilities
                .ShowFileSystemDialog(true, Config.DatabaseConfig.RestorationCliLocation).Path;
        }

        private void SelectLicenseFile(object sender, RoutedEventArgs e)
        {
            Config.LicenseConfig.Path = InteractionUtilities
                .ShowFileSystemDialog(false, Config.LicenseConfig.Path).Path;
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
