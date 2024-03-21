namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Конфигурация процесса установки приложения
    /// </summary>
    public class InstallationPipeline
    {
        #region Строки подключения к отдельным сервисам
        /// <summary>
        /// Обновление строки подключения к БД
        /// </summary>
        public bool UpdateDatabaseConnectionString { get; set; }

        /// <summary>
        /// Обновление строки подключения к Redis
        /// </summary>
        public bool UpdateRedisConnectionString { get; set; }
        #endregion

        #region Подготовка приложения
        /// <summary>
        /// Обновление порта приложения (AppSettings.json)
        /// </summary>
        public bool UpdateApplicationPort { get; set; }

        /// <summary>
        /// Удаление сертификата (AppSettings.json)
        /// </summary>
        public bool RemoveCertificate { get; set; }

        /// <summary>
        /// Исправление Cookie для корректной работы авторизации (CookiesSameSiteMode=Lax)
        /// </summary>
        public bool FixCookies { get; set; }
        #endregion

        #region Подготовка данных
        /// <summary>
        /// Восстановление бекапа БД
        /// </summary>
        public bool RestoreDatabaseBackup { get; set; }

        /// <summary>
        /// Установка лицензий
        /// </summary>
        public bool InstallLicense { get; set; }
        #endregion

        #region Операции с готовым приложением
        /// <summary>
        /// Отключить принудительную смену пароля
        /// </summary>
        public bool DisableForcePasswordChange { get; set; }

        /// <summary>
        /// Скомпилировать приложение
        /// </summary>
        public bool CompileApplication { get; set; }

        /// <summary>
        /// Запустить приложение
        /// </summary>
        public bool StartApplication { get; set; }
        #endregion
    }
}
