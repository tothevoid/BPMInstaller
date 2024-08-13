using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.Core.Services;
using BPMInstaller.UI.Desktop.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;
using BPMInstaller.UI.Desktop.Interfaces;
using BPMInstaller.UI.Desktop.Models.Basics;
using BPMInstaller.UI.Desktop.Models.UI;
using BPMInstaller.Core.Model;

namespace BPMInstaller.UI.Desktop.ViewModels
{
    public class InstallationConfigurationViewModel
    {
        #region Tools
        private IContextSynchronizer MainThreadSync { get; }

        private ConfigValidator ConfigurationValidator { get; } = new();

        private ConfigurationSerializer ConfigurationSerializer { get; } = new();
        #endregion

        private InstallationConfigurationModel InstallationConfiguration { get; }

        public InstallationConfigurationViewModel(IContextSynchronizer uiThreadSynchronizer)
        {
            MainThreadSync = uiThreadSynchronizer;

            InstallationConfiguration = new InstallationConfigurationModel(GetActionHandlers(), 
                ConfigurationSerializer.LoadLocations().ToList());
        }

        public InstallationConfigurationModel GetModel() => InstallationConfiguration;

        private ActionHandlersConfiguration GetActionHandlers()
        {
            return new ActionHandlersConfiguration()
            {
                SelectBackupFileCommand = new RelayCommand((_) => SelectBackupFile()),
                SelectDistributivePathCommand = new RelayCommand((_) => SelectDistributivePath()),
                SelectCliPathCommand = new RelayCommand((_) => SelectCliPath()),
                SelectLicenseFileCommand = new RelayCommand((_) => SelectLicenseFile()),
                StartInstallationCommand = new RelayCommand((_) => StartInstallation()),
                ValidateApplicationCommand = new RelayCommand((_) => ValidateApplication()),
                ValidateDatabaseCommand = new RelayCommand((_) => ValidateDatabase()),
                ValidateRedisCommand = new RelayCommand((_) => ValidateRedis())
            };
        }

        private void StartInstallation()
        {
            InstallationConfiguration.ControlsSessionState.StartInstallation();

            Task.Run(() => InstallationConfiguration.ControlsSessionState.StartCounter(MainThreadSync.InvokeSynced));

            Task.Run(() =>
            {
                var logger = new InstallationLogger(AddLoggerMessage);
                new InstallationService(logger, InstallationConfiguration.Config.ConvertToCoreModel(), HandlePassedStep).Install();

                MainThreadSync.InvokeSynced(() =>
                {
                    InstallationConfiguration.ControlsSessionState.InstallationEnded();
                });

            });
        }

        private void AddLoggerMessage(InstallationMessage message)
        {
            MainThreadSync.InvokeSynced(() => {
                InstallationConfiguration.ControlsSessionState.Output.Add(message);
            });
        }

        private void HandlePassedStep(string message)
        {
            MainThreadSync.InvokeSynced(() =>
            {
                InstallationConfiguration.ControlsSessionState.StepsPassed++;
            });
        }

        private void SelectDistributivePath()
        {
            var selectionResult = InteractionUtilities.ShowFileSystemDialog(true, InstallationConfiguration.Config.ApplicationPath);
            if (!selectionResult.IsSelected)
            {
                return;
            }

            var pathChanged = selectionResult.Path != InstallationConfiguration.Config.ApplicationPath;
            InstallationConfiguration.Config.ApplicationPath = selectionResult.Path;

            if (pathChanged && !string.IsNullOrEmpty(InstallationConfiguration.Config.ApplicationPath) && 
                !InstallationConfiguration.Configurations.Contains(InstallationConfiguration.Config.ApplicationPath))
            {
                InstallationConfiguration.Configurations.Add(InstallationConfiguration.Config.ApplicationPath);

                var newDbType = new DistributiveStateService(InstallationConfiguration.Config.ApplicationPath).DatabaseType;
                InstallationConfiguration.Config.DatabaseType = newDbType;

                ConfigurationSerializer.SaveLocations(InstallationConfiguration.Configurations);
            }

            var initConnectionStrings = pathChanged ||
                InteractionUtilities.ShowConfirmationButton("Выбран идентичный дистрибутив",
                   "Проставить значения строк подключения из конфигурационных файлов?");

            if (initConnectionStrings)
            {
                InstallationConfiguration.UpdateConfiguration(InstallationConfiguration.Config.ApplicationPath);
                InstallationConfiguration.ValidationState.Reset();
            }
        }

        #region File system selection handlers
        private void SelectBackupFile()
        {
            InstallationConfiguration.Config.BackupRestorationConfig.BackupPath = InteractionUtilities
                .ShowFileSystemDialog(false, InstallationConfiguration.Config.BackupRestorationConfig.BackupPath).Path;
        }

        private void SelectCliPath()
        {
            InstallationConfiguration.Config.BackupRestorationConfig.RestorationCliLocation = InteractionUtilities
                .ShowFileSystemDialog(true, InstallationConfiguration.Config.BackupRestorationConfig.RestorationCliLocation).Path;
        }

        private void SelectLicenseFile()
        {
            InstallationConfiguration.Config.LicenseConfig.Path = InteractionUtilities
                .ShowFileSystemDialog(false, InstallationConfiguration.Config.LicenseConfig.Path).Path;
        }
        #endregion

        #region Validation handlers
        //TODO: Remove duplication
        private void ValidateRedis()
        {
            ValidateConfig(() => ConfigurationValidator.ValidateRedisConnection(InstallationConfiguration.Config.RedisConfig.ToCoreModel()),
                ValidationState.ValidationOperation.Redis, InstallationConfiguration.Config.RedisConfig);
        }

        private void ValidateDatabase()
        {
            ValidateConfig(() => ConfigurationValidator.ValidateDatabaseConnection(
                    InstallationConfiguration.Config.DatabaseType,
                    InstallationConfiguration.Config.DatabaseConfig.ToCoreModel()),
                ValidationState.ValidationOperation.Database, InstallationConfiguration.Config.DatabaseConfig);
        }
        private void ValidateApplication()
        {
            ValidateConfig(() => ConfigurationValidator.ValidateAppConfig(InstallationConfiguration.Config.ApplicationConfig.ToCoreModel()),
                ValidationState.ValidationOperation.Application, InstallationConfiguration.Config.ApplicationConfig);
        }

        private void ValidateConfig(Func<string> validationHandler, ValidationState.ValidationOperation validationOperation, ResponsiveModel model)
        {
            InstallationConfiguration.ValidationState.StartValidation(validationOperation);
            Task.Run(() =>
            {
                var validationResult = validationHandler();
                
                MainThreadSync.InvokeSynced(() =>
                {
                    InstallationConfiguration.ValidationState.HandleValidationResult(validationOperation, validationResult);
                });
                model.CommitChanges();
            });
        }
        #endregion
    }
}
