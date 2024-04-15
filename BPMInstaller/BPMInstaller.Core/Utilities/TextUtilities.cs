using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BPMInstaller.Core.Utilities
{
    public static class TextUtilities
    {
        public static string EscapeExpression(string expression) => $"\\\"{expression}\\\"";
    }
}
