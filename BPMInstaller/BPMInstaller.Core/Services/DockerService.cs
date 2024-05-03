using BPMInstaller.Core.Model.Docker;
using BPMInstaller.Core.Utilities;

namespace BPMInstaller.Core.Services
{
    //TODO: separate into SqlServer and Postgres implementation + common logic service
    public class DockerService
    {
        private const string ProcessName = "docker";

        // TODO: add results validation
        public Dictionary<string, string> GetActiveContainers()
        {
            const string outputFormat = "{{.ID}}\t{{.Names}}\"";

            var commandResult = new CommandLineQueryExecutor(ProcessName)
                .AddParameter("ps")
                .AddParameter("--format", outputFormat)
                .Execute();

            return ProcessRawContainersList(commandResult.Output);
        }

        public bool CopyFileIntoContainer(string sourceFilePath, string containerId, string backupPath)
        {
            if (!File.Exists(sourceFilePath))
            {
                return false;
            }

            string destinationPath = $"{containerId}:{backupPath}";

            var commandResult = new CommandLineQueryExecutor(ProcessName)
                .AddParameter("cp")
                .AddParameter(sourceFilePath)
                .AddParameter(destinationPath)
                .Execute();
            
            return string.IsNullOrEmpty(commandResult.Output) &&
                   string.IsNullOrEmpty(commandResult.ErrorOutput);
        }

        public CommandLineExecutionResult ExecuteCommandInContainer(string containerId, string command, bool isInteractive = false) =>
            GetDockerShellExecutor(containerId, command, isInteractive)
                .Execute();
        
        public bool ExecuteCommandInContainerWhile(string containerId, string command, Func<string, bool> outputHandler) =>
            GetDockerShellExecutor(containerId, command, true)
                .ExecuteWhile(outputHandler);

        private Dictionary<string, string> ProcessRawContainersList(string rawOutput)
        {
            if (string.IsNullOrEmpty(rawOutput))
            {
                return new Dictionary<string, string>();
            }

            return rawOutput
                .Split(Environment.NewLine)
                .Where(command => !string.IsNullOrEmpty(command))
                .Select(part =>
                {
                    var subPart = part.Split("\t");
                    return new DockerContainer { Id = subPart[0], ImageName = subPart[1] };
                })
                .ToDictionary(key => key.Id, value => value.ImageName);
        }

        private CommandLineQueryExecutor GetDockerShellExecutor(string containerId, string command, bool interactiveMode = false)
        {
            var builder = new CommandLineQueryExecutor(ProcessName)
                .AddParameter("exec");

            if (interactiveMode)
            {
                builder.AddParameter("-it");
            }

            return builder
                .AddParameter(containerId)
                .AddParameter("bash")
                .AddParameter("-c", $"\"{command}\"");
        }
    }
}
