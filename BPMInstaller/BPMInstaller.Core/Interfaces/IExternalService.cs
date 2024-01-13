using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Interfaces
{
    public interface IExternalService
    { 
        /// <summary>
        /// Проверка возможности подключения
        /// </summary>
        /// <returns>Успешное подключение</returns>
        public bool ValidateConnection();
    }
}
