namespace BPMInstaller.Core.Interfaces
{
    public interface IDatabaseService
    {
        /// <summary>
        /// Проверка возможности подключения к серверу БД
        /// </summary>
        /// <returns>Успешное подключение к БД</returns>
        public bool ValidateConnection();

        /// <summary>
        /// Создание БД
        /// </summary>
        /// <returns>БД создано успешно</returns>
        public bool CreateDatabase();

        /// <summary>
        /// Восстановление БД по бекапу
        /// </summary>
        /// <returns>Бекап успешно восстановлен</returns>
        public bool RestoreDatabase();


        /// <summary>
        /// Отключение принудтилельной смены пароля пользователя
        /// </summary>
        /// <param name="userName">Логин пользователя в системе</param>
        /// <returns>Отключение успешно</returns>
        public bool DisableForcePasswordChange(string userName);


        /// <summary>
        /// Обновление Customer id
        /// </summary>
        /// <param name="cId">Идентификатор оранизации</param>
        /// <returns>Customer id обновлён</returns>
        public bool UpdateCid(long cId);
    }
}
