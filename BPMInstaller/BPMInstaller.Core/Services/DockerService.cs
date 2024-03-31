using BPMInstaller.Core.Model.Docker;
using System.Diagnostics;
using System.Text.Json;

namespace BPMInstaller.Core.Services
{
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

        public bool CopyBackup(string backupPath, string containerId, string backupDockerName)
        {
            if (!File.Exists(backupPath))
            {
                return false;
            }

            var output = CallDockerCommand($"cp {backupPath} {containerId}:/{backupDockerName}");

            return !string.IsNullOrEmpty(output.StandardOutput) && string.IsNullOrEmpty(output.ErrorOutput);
        }

        public bool RestoreBackup(string containerId, string userName, string dbName, string containerBackupFileName)
        {
            var script = $"pg_restore --username={userName} --dbname={dbName} --no-owner --no-privileges ./{containerBackupFileName}";
            var output = CallDockerCommand($"exec {containerId} bash -c \"{script}\"");

            return string.IsNullOrEmpty(output.ErrorOutput);
        }

        private (string StandardOutput, string ErrorOutput) CallDockerCommand(string command)
        {
            Process process = new Process();
            process.StartInfo.FileName = "docker";
            process.StartInfo.Arguments = command;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            process.WaitForExit();

            return (process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
        }
    }

   
}
