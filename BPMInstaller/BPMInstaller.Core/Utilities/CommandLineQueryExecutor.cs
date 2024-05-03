using System.Diagnostics;

namespace BPMInstaller.Core.Utilities
{
    public class CommandLineQueryExecutor
    {
        private string ExecutingFileName { get; }

        private bool RunInForeground { get; }

        private bool UseShellExecute { get; }

        private bool CustomOutputHandler { get; }

        private Dictionary<string, string> Parameters { get; } = new Dictionary<string, string>();

        public CommandLineQueryExecutor(string fileName, bool customOutputHandler = true, bool useShellExecute = true, bool runInForeground = true)
        {
            ExecutingFileName = fileName ?? throw new ArgumentNullException(fileName);
            RunInForeground = runInForeground;
            UseShellExecute = useShellExecute;
            CustomOutputHandler = customOutputHandler;
        }

        public CommandLineQueryExecutor AddParameter(string key, string value)
        {
            Parameters.Add(key, value);
            return this;
        }

        public CommandLineQueryExecutor AddParameter(string key)
        {
            Parameters.Add(key, string.Empty);
            return this;
        }

        private Process Build()
        {
            Process process = new Process();
            process.StartInfo.FileName = ExecutingFileName;

            process.StartInfo.Arguments = GetArguments();
            process.StartInfo.UseShellExecute = false;

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

            return process;
        }

        private string GetArguments()
        {
            var parameters = Parameters.Select(parameter =>
            {
                string separator = !string.IsNullOrEmpty(parameter.Value) ? " ": "";
                return $"{parameter.Key}{separator}{parameter.Value}";
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
