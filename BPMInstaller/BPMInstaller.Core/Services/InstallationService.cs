﻿using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;

namespace BPMInstaller.Core.Services
{
    public class InstallationService
    {
        private event Action<InstallationMessage> OnInstallationMessageReceived;

        public InstallationService(Action<InstallationMessage> messageHandler)
        {
            if (messageHandler == null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            OnInstallationMessageReceived += messageHandler;
        }

        public void StartBasicInstallation(InstallationConfig installationConfig)
        {
            if (installationConfig == null)
            {
                throw new ArgumentException(nameof(installationConfig));
            }

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Запущена установка приложения" });

            if (installationConfig.InstallationWorkflow.RestoreDatabaseBackup)
            {
                if (!InitalizeDatabase(installationConfig.DatabaseConfig))
                {
                    return;
                }
            }

            SetupDistributive(installationConfig.InstallationWorkflow, installationConfig);

            var databaseService = new PostgresDatabaseService(installationConfig.DatabaseConfig);

            var appService = new ApplicationService();

            if (installationConfig.InstallationWorkflow.DisableForcePasswordChange)
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Исправление принудительной смены пароля" });
                databaseService.DisableForcePasswordChange(installationConfig.ApplicationConfig.AdminUserName);
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Принудительная смена пароля отключена" });
            }
            
            if (!installationConfig.InstallationWorkflow.StartApplication)
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Установка приложения завершена", IsTerminal = true });
                return;
            }

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Запуск приложения" });
            appService.RunApplication(installationConfig.ApplicationConfig, () =>
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Приложение запущено" });

                if (installationConfig.InstallationWorkflow.InstallLicense)
                {
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Установка лицензий" });
                    appService.UploadLicenses(installationConfig.ApplicationConfig, installationConfig.LicenseConfig);
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Лицензии установлены" });

                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Обновление CId" });
                    databaseService.UpdateCid(installationConfig.LicenseConfig.CId);
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "CId обновлён" });
                }

                if (installationConfig.InstallationWorkflow.CompileApplication)
                {
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Запущена компиляция приложения" });
                    appService.RebuildApplication(installationConfig.ApplicationConfig);
                }
              
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Установка приложения завершена", IsTerminal = true });
            });       
        }

        public bool InitalizeDatabase(DatabaseConfig dbConfig)
        {
            var databaseService = new PostgresDatabaseService(dbConfig);

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = $"Создание БД: {dbConfig.DatabaseName}" });

            if (!databaseService.CreateDatabase())
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Ошибка создания БД", IsTerminal = true });
                return false;
            }

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "БД создана" });
            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Восстановление БД из бекапа" });

            if (!databaseService.RestoreDatabase())
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Ошибка восстановления БД", IsTerminal = true });
                return false;
            }

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "БД восстановлена" });
           
            return true;
        }

        public void SetupDistributive(InstallationWorkflow workflow, InstallationConfig installationConfig)
        {
            var distributiveService = new DistributiveService();

            if (workflow.RestoreDatabaseBackup)
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Актуализация ConnectionStrings" });
                distributiveService.UpdateConnectionStrings(installationConfig);
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "ConnectionStrings актулизированы" });
            }

            if (workflow.UpdateApplicationPort)
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Актуализация порта приложения" });
                distributiveService.UpdateApplicationPort(installationConfig.ApplicationConfig);
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Порт актуализирован" });
            }

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Исправление авторизации" });
            distributiveService.FixAuthorizationCookies(installationConfig.ApplicationConfig);
            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Авторизация исправлена" });
        }
    }
}
