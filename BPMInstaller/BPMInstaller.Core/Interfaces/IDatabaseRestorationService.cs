namespace BPMInstaller.Core.Interfaces
{
    /// <summary>
    /// Операции восстановления БД
    /// </summary>
    internal interface IDatabaseRestorationService
    {
        /// <summary>
        /// Восстановление через Docker
        /// </summary>
        /// <returns>Восстановление успешно</returns>
        public bool RestoreByDocker();

        /// <summary>
        /// Восстановление через CLI
        /// </summary>
        /// <returns>Восстановление успешно</returns>
        public bool RestoreByCli();
    }
}
