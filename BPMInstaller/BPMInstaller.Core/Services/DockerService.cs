using BPMInstaller.Core.Model.Docker;
using System.Diagnostics;
using BPMInstaller.Core.Model;
using BPMInstaller.Core.Utilities;
using System.ComponentModel;

namespace BPMInstaller.Core.Services
{
    //TODO: separate into SqlServer and Postgres implementation + common logic service
    public class DockerService
    {
        public Dictionary<string, string> GetActiveContainers()
        {
            var command = "ps --format \"{{.ID}}\t{{.Names}}\"";
            var output = CallDockerCommand(command);

            return output.StandardOutput.Split("\n").Where(command => !string.IsNullOrEmpty(command))
                .Select(part =>
                {
                    var subPart = part.Split("\t");
                    return new DockerContainer { Id = subPart[0], ImageName = subPart[1] };
                })
                .ToDictionary(key => key.Id, value => value.ImageName);
        }

        //TODO: get rid of DatabaseType dependency 
        public bool CopyFileIntoContainer(string filePath, string containerId, string containerFileName, string backupName)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var output = CallDockerCommand($"cp {filePath} {containerId}:/{backupName}");
            return string.IsNullOrEmpty(output.StandardOutput) && string.IsNullOrEmpty(output.ErrorOutput);
        }

        public string FormatDockerExecutableCommand(string containerId, string command, bool interactiveMode = false)
        {
            var interactiveFlag = interactiveMode ? "-it " : string.Empty;

            return $"exec {interactiveFlag}{containerId} bash -c \"{command}\"";
        }

        public (string StandardOutput, string ErrorOutput) CallDockerCommand(string command)
        {
            var process = GetBasicProcessConfiguration(command);
            process.Start();
            process.WaitForExit();

            return (process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
        }

        public (string StandardOutput, string ErrorOutput) ExecuteCommandInContainer(string containerId, string command, bool isInteractive = false)
        {
            var dockerCommand = FormatDockerExecutableCommand(containerId, command, isInteractive);
            var process = GetBasicProcessConfiguration(dockerCommand);
            process.Start();
            process.WaitForExit();

            return (process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
        }

        public bool CallDynamicCommand(string containerId, string command, Func<string, bool> outputHandler)
        {
            var dockerCommand = FormatDockerExecutableCommand(containerId, command, true);
            var process = GetBasicProcessConfiguration(dockerCommand);
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


        private Process GetBasicProcessConfiguration(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "docker";
            process.StartInfo.Arguments = command;
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            return process;
        }
    }
}
