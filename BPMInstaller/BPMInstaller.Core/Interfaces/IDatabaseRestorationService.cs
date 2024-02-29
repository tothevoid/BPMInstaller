namespace BPMInstaller.Core.Interfaces
{
    /// <summary>
    /// Операции восстановления БД
    /// </summary>
    internal interface IDatabaseRestorationService
    {
        /// <summary>
        /// Восстановление БД с помощью бекапа
        /// </summary>
        /// <returns>Восстановление успешно</returns>
        public bool Restore();
    }
}
