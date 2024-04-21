using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

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
            process.StartInfo.UseShellExecute = UseShellExecute;

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
            bool waitForMessages = true;

            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null && waitForMessages)
                {
                    waitForMessages = outputHandler(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            while (waitForMessages)
            {
                Thread.Sleep(150);
            }

            // TODO: Add timeout exception
            return true;
        }
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
