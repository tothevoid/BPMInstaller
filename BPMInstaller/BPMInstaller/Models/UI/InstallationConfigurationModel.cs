using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using BPMInstaller.Core.Services;
using BPMInstaller.UI.Desktop.Models.Configs;

namespace BPMInstaller.UI.Desktop.Models.UI
{
    public class InstallationConfigurationModel
    {
        public ObservableCollection<string> Configurations { get; }
        public InstallationConfig Config { get; private set; } = new InstallationConfig();
        public ControlsSessionState ControlsSessionState { get; } = new ControlsSessionState();
        public ValidationState ValidationState { get; } = new ValidationState();
        public ActionHandlersConfiguration Actions { get; }

        public InstallationConfigurationModel(ActionHandlersConfiguration actions, List<string>? configurations = null)
        {
            Actions = actions;
            Configurations = new ObservableCollection<string>(configurations ?? Enumerable.Empty<string>());
            var activeConfiguration = Configurations.FirstOrDefault();
            if (activeConfiguration != null)
            {
                UpdateConfiguration(activeConfiguration);
            }
        }

        public void UpdateConfiguration(string? applicationPath)
        {
            if (string.IsNullOrEmpty(applicationPath) || !Directory.Exists(applicationPath))
            {
                Config = new InstallationConfig();
                return;
            }

            var stateLoader = new AppConfigurationStateLoader();
            var state = stateLoader.GetConfig(applicationPath);
            Config.ApplicationPath = applicationPath;
            Config.DatabaseType = new DistributiveService().GetDatabaseType(applicationPath);
            Config.ApplicationConfig.MergeConfig(state.ApplicationConfig);
            Config.DatabaseConfig.MergeConfig(state.DatabaseConfig);
            Config.RedisConfig.MergeConfig(state.RedisConfig);
        }
    }
}
