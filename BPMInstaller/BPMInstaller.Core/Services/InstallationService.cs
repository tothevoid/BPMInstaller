using BPMInstaller.Core.Interfaces;
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
            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Запущена установка приложения" });

            if (installationConfig.OptionsConfig.RestoreBackup)
            {
                if (!InitalizeDatabase(installationConfig.DatabaseConfig))
                {
                    return;
                }
            }

            SetupDistributive(installationConfig.OptionsConfig, installationConfig);

            var databaseService = new PostgresDatabaseService(installationConfig.DatabaseConfig);

            var appService = new ApplicationService();

            if (installationConfig.OptionsConfig.DisableForcePasswordChange)
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Исправление принудительной смены пароля" });
                databaseService.DisableForcePasswordChange(installationConfig.ApplicationConfig.AdminUserName);
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Принудительная смена пароля отключена" });
            }
                
            OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Запуск приложения" });
            appService.RunApplication(installationConfig.ApplicationConfig, () =>
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Приложение запущено" });

                if (installationConfig.OptionsConfig.AddLicense)
                {
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Установка лицензий" });
                    appService.UploadLicenses(installationConfig.ApplicationConfig, installationConfig.LicenseConfig);
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Лицензии установлены" });

                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Обновление CId" });
                    databaseService.UpdateCid(installationConfig.LicenseConfig.CId);
                    OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "CId обновлён" });
                }

                if (installationConfig.OptionsConfig.CompileApplication)
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

        public void SetupDistributive(InstallationOptionsConfig optionsConfig, InstallationConfig installationConfig)
        {
            var distributiveService = new DistributiveService();

            if (optionsConfig.RestoreBackup)
            {
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "Актуализация ConnectionStrings" });
                distributiveService.UpdateConnectionStrings(installationConfig);
                OnInstallationMessageReceived.Invoke(new InstallationMessage() { Content = "ConnectionStrings актулизированы" });
            }

            if (installationConfig.ApplicationConfig.ApplicationPort != 0)
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
