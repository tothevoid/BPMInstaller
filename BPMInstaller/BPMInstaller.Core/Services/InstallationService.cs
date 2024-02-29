using BPMInstaller.Core.Constants;
using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;
using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.Core.Services.Database.Postgres;

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
                if (!InitializeDatabase(installationConfig.DatabaseConfig))
                {
                    return;
                }
            }

            SetupDistributive(installationConfig.InstallationWorkflow, installationConfig);

            var databaseService = new PostgresDatabaseService(installationConfig.DatabaseConfig);

            var appService = new ApplicationService();
            var redisService = new RedisService();

            if (installationConfig.InstallationWorkflow.DisableForcePasswordChange)
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Исправление принудительной смены пароля" });
              
                databaseService.DisableForcePasswordChange(ApplicationAdministrator.UserName);
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Принудительная смена пароля отключена" });
            }
            
            if (!installationConfig.InstallationWorkflow.StartApplication)
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Установка приложения завершена", IsTerminal = true });
                return;
            }

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Проверка наличия запущенного приложения" });
            var closed = appService.CloseActiveApplication(installationConfig.ApplicationConfig.ApplicationPort, 
                installationConfig.ExecutableApplicationPath);
            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = closed ? 
                "Активное прилоение выключено":
                "Активное приложение не найдено"
            });

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Запуск приложения" });
            appService.RunApplication(installationConfig.ApplicationPath, () =>
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Приложение запущено" });

                if (installationConfig.InstallationWorkflow.InstallLicense)
                {
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Обновление CId" });
                    databaseService.UpdateCid(installationConfig.LicenseConfig.CId);
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "CId обновлён" });

                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Установка лицензий" });
                    appService.UploadLicenses(installationConfig.ApplicationConfig, installationConfig.LicenseConfig);
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Лицензии установлены" });

                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = $"Назначение лицензий на {ApplicationAdministrator.UserName}" });
                    databaseService.ApplyAdministratorLicenses(ApplicationAdministrator.UserName);
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Лицензии назначены" });

                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = $"Запуск очистки Redis" });
                    redisService.FlushData(installationConfig.RedisConfig);
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Данные приложения в Redis удалены" });
                }

                if (installationConfig.InstallationWorkflow.CompileApplication)
                {
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Запущена компиляция приложения" });
                    appService.RebuildApplication(installationConfig.ApplicationConfig);
                }
              
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Установка приложения завершена", IsTerminal = true });
            });       
        }

        public bool InitializeDatabase(DatabaseConfig dbConfig)
        {
            var databaseService = new PostgresDatabaseService(dbConfig);

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = $"Проверка подключения к БД" });
            var exceptionMessage = databaseService.ValidateConnection();
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = $"Ошибка валидации подключения к БД" });
                return false;
            }
            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = $"Успешное подключение к БД" });

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = $"Сброс активных подключений к БД" });
            databaseService.TerminateAllActiveSessions(dbConfig.DatabaseName);
            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = $"Подключения к БД сброшены" });

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = $"Создание БД: {dbConfig.DatabaseName}" });

            var databaseCreationResult = databaseService.CreateDatabase();
            if (!string.IsNullOrEmpty(databaseCreationResult))
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = $"Ошибка создания БД: {databaseCreationResult}", IsTerminal = true });
                return false;
            }

            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "БД создана" });
            
           
            return true;
        }

        public bool RestoreDatabase(DatabaseConfig dbConfig, BackupRestorationConfig restorationConfig)
        {
            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Восстановление БД из бекапа" });

            IDatabaseRestorationService databaseRestorationService = new PostgresRestorationService(restorationConfig, dbConfig);
            if (!databaseRestorationService.Restore())
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
                distributiveService.UpdateConnectionStrings(installationConfig, installationConfig.ApplicationPath);
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "ConnectionStrings актулизированы" });
            }

            if (workflow.UpdateApplicationPort)
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Актуализация порта приложения" });
                distributiveService.UpdateApplicationPort(installationConfig.ApplicationConfig, installationConfig.ApplicationPath);
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Порт актуализирован" });
            }

            if (workflow.FixCookies)
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Исправление авторизации" });
                distributiveService.FixAuthorizationCookies(installationConfig.ApplicationPath);
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Авторизация исправлена" });
            }
        }
    }
}
