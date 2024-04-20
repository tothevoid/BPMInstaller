using System.Text.RegularExpressions;

namespace BPMInstaller.Core.Services.Database.MsSql
{
    public static class SqlCmdParser
    {
        /*
         * Ответ формируется в следующем виде:
         * Колонки результата
         * Строка-разделитель из символа _
         * N-строк ответов
         * Пустая строка
         * Количество строк
         * Пустая строка
         */
        public static IEnumerable<string> ParseQueryResponse(string output)
        {
            var outputParts = output.Split(Environment.NewLine); 
            return outputParts
                .Skip(2)
                .Take(outputParts.Length - 5)
                .Select(row => Regex.Replace(row, @"\s{2,}", "\t"));
        }
    }
}
