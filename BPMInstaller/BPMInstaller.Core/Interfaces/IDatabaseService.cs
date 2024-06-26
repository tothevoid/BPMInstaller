﻿namespace BPMInstaller.Core.Interfaces
{
    /// <summary>
    /// Типовые операции с БД
    /// </summary>
    public interface IDatabaseService
    {
        /// <summary>
        /// Создание БД
        /// </summary>
        /// <returns>Текст ошибки</returns>
        public string CreateDatabase();

        /// <summary>
        /// Отключение принудительной смены пароля пользователя
        /// </summary>
        /// <param name="userName">Логин пользователя в системе</param>
        /// <returns>Отключение успешно</returns>
        public bool DisableForcePasswordChange(string userName);

        /// <summary>
        /// Обновление Customer id
        /// </summary>
        /// <param name="cId">Идентификатор организации</param>
        /// <returns>Customer id обновлён</returns>
        public bool UpdateCid(long cId);

        /// <summary>
        /// Удаление назначенных лицензий и назначение всех на администратора
        /// </summary>
        /// <param name="userName">Пользователь администратора</param>
        public bool ApplyAdministratorLicenses(string userName);

        public string ValidateConnection();

        void TerminateAllActiveSessions(string databaseConfigDatabaseName);
    }
}
