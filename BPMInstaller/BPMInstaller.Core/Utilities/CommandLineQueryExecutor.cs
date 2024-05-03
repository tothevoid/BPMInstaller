using System.Diagnostics;
using System.Reflection.Metadata;

namespace BPMInstaller.Core.Utilities
{
    public class CommandLineQueryExecutor
    {
        private string ExecutingFileName { get; }

        private bool RunInForeground { get; }

        //TODO: remove
        private bool UseShellExecute { get; }

        private bool CustomOutputHandler { get; }

        private List<QueryParameter> Parameters { get; } = new List<QueryParameter>();

        private Dictionary<string, string> EnvironmentVariables { get; } = new Dictionary<string, string>();

        private string? InitialDirectory { get; }

        public CommandLineQueryExecutor(string fileName, string? initialDirectory = null, bool customOutputHandler = true, bool useShellExecute = true, 
            bool runInForeground = true)
        {
            ExecutingFileName = fileName ?? throw new ArgumentNullException(fileName);
            RunInForeground = runInForeground;
            UseShellExecute = useShellExecute;
            CustomOutputHandler = customOutputHandler;
            InitialDirectory = initialDirectory;
        }

        public CommandLineQueryExecutor AddParameter(string key, string value, string separator = " ")
        {
            Parameters.Add(new QueryParameter() {Key = key, Value = value, Separator = separator });
            return this;
        }

        public CommandLineQueryExecutor AddEnvironmentVariable(string variable, string value)
        {
            EnvironmentVariables.Add(variable, value);
            return this;
        }

        public CommandLineQueryExecutor AddParameter(string key)
        {
            Parameters.Add(new QueryParameter() { Key = key });
            return this;
        }

        private Process Build()
        {
            Process process = new Process();
            process.StartInfo.FileName = ExecutingFileName;

            process.StartInfo.Arguments = GetArguments();
            process.StartInfo.UseShellExecute = false;

            if (!string.IsNullOrEmpty(InitialDirectory))
            {
                process.StartInfo.WorkingDirectory = InitialDirectory;
            }

            if (RunInForeground)
            {
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            }

            if (CustomOutputHandler)
            {
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
            }

            foreach (var environmentVariables in EnvironmentVariables)
            {
                process.StartInfo.EnvironmentVariables[environmentVariables.Key] = environmentVariables.Value;
            }

            return process;
        }

        private string GetArguments()
        {
            var parameters = Parameters.Select(parameter =>
            {
                return $"{parameter.Key}{parameter.Separator ?? string.Empty}{parameter.Value ?? string.Empty}";
            });

            return string.Join(" ", parameters);
        }

        public CommandLineExecutionResult Execute()
        {
            var process = Build();
            process.Start();
            process.WaitForExit();

            return new CommandLineExecutionResult(process.StandardOutput.ReadToEnd(),
                process.StandardError.ReadToEnd());
        }

        public bool ExecuteWhile(Func<string, bool> outputHandler)
        {
            var process = Build();
            bool shouldExit = false;

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null && !shouldExit)
                {
                    shouldExit = outputHandler(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            //process.WaitForExit();

            while (!shouldExit)
            {
                Thread.Sleep(150);
            }

            //TODO: maybe that call is not needed
            //process.CancelOutputRead();
            process.Close();
            // TODO: Add timeout exception
            return true;
        }

        private class QueryParameter
        {
            public string Key { get; set; }

            public string? Value { get; set; }

            public string? Separator { get; set; }
        }
    }

    public class CommandLineExecutionResult
    {
        public string Output { get; }

        public string ErrorOutput { get; }

        public CommandLineExecutionResult(string output, string errorOutput)
        {
            Output = output;  
            ErrorOutput = errorOutput;
        }
    }
}
