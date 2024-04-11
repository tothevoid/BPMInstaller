using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.Core.Services;
using BPMInstaller.UI.Desktop.Model;
using BPMInstaller.UI.Desktop.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using BPMInstaller.UI.Desktop.Interfaces;
using BPMInstaller.UI.Desktop.Model.UI;

namespace BPMInstaller.UI.Desktop.ViewModels
{
    public class InstallationConfigurationViewModel
    {
        #region Tools
        private IContextSynchronizer UiSynchronizer { get; }

        private ConfigValidator Validator { get; } = new();

        private ConfigurationSerializer ConfigSerializer { get; set; } = new();
        #endregion

        private InstallationConfigurationModel InstallationConfiguration { get; }

        public InstallationConfigurationViewModel(IContextSynchronizer contextSynchronizer)
        {
            UiSynchronizer = contextSynchronizer;

            InstallationConfiguration = new InstallationConfigurationModel(GetActionHandlers(), 
                ConfigSerializer.LoadLocations().ToList());
        }

        public InstallationConfigurationModel GetModel() => InstallationConfiguration;

        private ActionHandlersConfiguration GetActionHandlers()
        {
            return new ActionHandlersConfiguration()
            {
                SelectBackupFileCommand = new RelayCommand((_) => SelectBackupFile()),
                SelectDistributivePathCommand = new RelayCommand((_) => SelectBackupFile()),
                SelectCliPathCommand = new RelayCommand((_) => SelectBackupFile()),
                SelectLicenseFileCommand = new RelayCommand((_) => SelectBackupFile()),
                StartInstallationCommand = new RelayCommand((_) => SelectBackupFile()),
                ValidateApplicationCommand = new RelayCommand((_) => SelectBackupFile()),
                ValidateDatabaseCommand = new RelayCommand((_) => SelectBackupFile()),
                ValidateRedisCommand = new RelayCommand((_) => SelectBackupFile())
            };
        }

        private void StartInstallation()
        {
            InstallationConfiguration.ControlsSessionState.StartInstallation();

            Task.Run(() =>
            {
                var logger = new InstallationLogger(AddLoggerMessage);
                new InstallationService(logger, InstallationConfiguration.Config.ConvertToCoreModel()).Install();

                UiSynchronizer.InvokeSynced(() =>
                {
                    InstallationConfiguration.ControlsSessionState.InstallationEnded();
                });

            });
        }

        private void AddLoggerMessage(InstallationMessage message)
        {
            UiSynchronizer.InvokeSynced(() => {
                InstallationConfiguration.ControlsSessionState.Output.Add(message);
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
                ConfigSerializer.SaveLocations(InstallationConfiguration.Configurations);
            }

            var initConnectionStrings = pathChanged ||
                InteractionUtilities.ShowConfirmationButton("Выбран идентичный дистрибутив",
                   "Проставить значения строк подключения из конфигурационных файлов?");


            if (initConnectionStrings)
            {
                InstallationConfiguration.UpdateConfiguration(InstallationConfiguration.Config.ApplicationPath);
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
            ValidateConfig(() => Validator.ValidateRedisConnection(InstallationConfiguration.Config.RedisConfig.ToCoreModel()),
                ValidationState.ValidationOperation.Redis, InstallationConfiguration.Config.RedisConfig);
        }

        private void ValidateDatabase()
        {
            ValidateConfig(() => Validator.ValidateDatabaseConnection(InstallationConfiguration.Config.DatabaseConfig.ToCoreModel()),
                ValidationState.ValidationOperation.Database, InstallationConfiguration.Config.DatabaseConfig);
        }
        private void ValidateApplication()
        {
            ValidateConfig(() => Validator.ValidateAppConfig(InstallationConfiguration.Config.ApplicationConfig.ToCoreModel()),
                ValidationState.ValidationOperation.Application, InstallationConfiguration.Config.ApplicationConfig);
        }

        private void ValidateConfig(Func<string> validationHandler, ValidationState.ValidationOperation validationOperation, BaseUIModel model)
        {
            InstallationConfiguration.ValidationState.StartValidation(validationOperation);
            Task.Run(() =>
            {
                var validationResult = validationHandler();
                
                UiSynchronizer.InvokeSynced(() =>
                {
                    InstallationConfiguration.ValidationState.HandleValidationResult(validationOperation, validationResult);
                });
                model.CommitChanges();
            });
        }
        #endregion
    }
}
