using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Model
{
    /// <summary>
    /// Целевая БД дистрибутива
    /// </summary>
    public enum DatabaseType
    {
        NotSpecified,
        PostgreSql,
        MsSql
    }
}
