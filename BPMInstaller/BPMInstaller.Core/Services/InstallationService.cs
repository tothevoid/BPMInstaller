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

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Запущена установка приложения"));

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
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Исправление принудительной смены пароля"));
              
                databaseService.DisableForcePasswordChange(ApplicationAdministrator.UserName);
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Принудительная смена пароля отключена"));
            }
            
            if (!installationConfig.InstallationWorkflow.StartApplication)
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Установка приложения завершена", true));
                return;
            }

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Проверка наличия запущенного приложения"));
            var closed = appService.CloseActiveApplication(installationConfig.ApplicationConfig.ApplicationPort, 
                installationConfig.ExecutableApplicationPath);
            OnInstallationMessageReceived.Invoke(InstallationMessage.Info(closed ? 
                "Активное прилоение выключено":
                "Активное приложение не найдено"
            ));

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Запуск приложения"));
            appService.RunApplication(installationConfig.ApplicationPath, () =>
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Приложение запущено"));

                if (installationConfig.InstallationWorkflow.InstallLicense)
                {
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Обновление CId"));
                    databaseService.UpdateCid(installationConfig.LicenseConfig.CId);
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info("CId обновлён"));

                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Установка лицензий"));
                    appService.UploadLicenses(installationConfig.ApplicationConfig, installationConfig.LicenseConfig);
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Лицензии установлены"));

                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info($"Назначение лицензий на {ApplicationAdministrator.UserName}"));
                    databaseService.ApplyAdministratorLicenses(ApplicationAdministrator.UserName);
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Лицензии назначены"));

                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info($"Запуск очистки Redis"));
                    redisService.FlushData(installationConfig.RedisConfig);
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Данные приложения в Redis удалены"));
                }

                if (installationConfig.InstallationWorkflow.CompileApplication)
                {
                    OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Запущена компиляция приложения"));
                    appService.RebuildApplication(installationConfig.ApplicationConfig);
                }
              
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Установка приложения завершена", true));
            });       
        }

        public bool InitializeDatabase(DatabaseConfig dbConfig)
        {
            var databaseService = new PostgresDatabaseService(dbConfig);

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info($"Проверка подключения к БД"));
            var exceptionMessage = databaseService.ValidateConnection();
            if (!string.IsNullOrEmpty(exceptionMessage))
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info($"Ошибка валидации подключения к БД"));
                return false;
            }
            OnInstallationMessageReceived.Invoke(InstallationMessage.Info($"Успешное подключение к БД"));

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info($"Сброс активных подключений к БД"));
            databaseService.TerminateAllActiveSessions(dbConfig.DatabaseName);
            OnInstallationMessageReceived.Invoke(InstallationMessage.Info($"Подключения к БД сброшены"));

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info($"Создание БД: {dbConfig.DatabaseName}"));

            var databaseCreationResult = databaseService.CreateDatabase();
            if (!string.IsNullOrEmpty(databaseCreationResult))
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info($"Ошибка создания БД: {databaseCreationResult}", true));
                return false;
            }

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info("БД создана"));
            
           
            return true;
        }

        public bool RestoreDatabase(DatabaseConfig dbConfig, BackupRestorationConfig restorationConfig)
        {
            OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Восстановление БД из бекапа"));

            IDatabaseRestorationService databaseRestorationService = new PostgresRestorationService(restorationConfig, dbConfig);
            if (!databaseRestorationService.Restore())
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Ошибка восстановления БД", true) );
                return false;
            }

            OnInstallationMessageReceived.Invoke(InstallationMessage.Info("БД восстановлена"));
            return true;
        }

        public void SetupDistributive(InstallationWorkflow workflow, InstallationConfig installationConfig)
        {
            var distributiveService = new DistributiveService();

            if (workflow.RestoreDatabaseBackup)
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Актуализация ConnectionStrings"));
                distributiveService.UpdateConnectionStrings(installationConfig, installationConfig.ApplicationPath);
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("ConnectionStrings актулизированы"));
            }

            if (workflow.UpdateApplicationPort)
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Актуализация порта приложения"));
                distributiveService.UpdateApplicationPort(installationConfig.ApplicationConfig, installationConfig.ApplicationPath);
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Порт актуализирован"));
            }

            if (workflow.FixCookies)
            {
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Исправление авторизации"));
                distributiveService.FixAuthorizationCookies(installationConfig.ApplicationPath);
                OnInstallationMessageReceived.Invoke(InstallationMessage.Info("Авторизация исправлена"));
            }
        }
    }
}
