using BPMInstaller.Core.Model.Docker;
using System.Diagnostics;
using System.Text.Json;
using BPMInstaller.Core.Model;
using System.Data.SqlTypes;
using System.Net.Sockets;
using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model.Runtime;

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

        public bool RestorePostgresBackup(string containerId, string userName, string dbName, string containerBackupFileName)
        {
            var script = $"pg_restore --username={userName} --dbname={dbName} --no-owner --no-privileges ./{containerBackupFileName}";
            var output = CallDockerCommand($"exec {containerId} bash -c \"{script}\"");

            return string.IsNullOrEmpty(output.ErrorOutput);
        }

        public bool RestoreMsBackup(string containerId, DatabaseConfig dbConfig, string containerBackupFileName, string backupFullName, IInstallationLogger logger)
        {
            var dataParts = GetRestorationList(containerId, dbConfig, backupFullName);

            var moveExpressions = dataParts.Select(pair =>
                $"MOVE  \\\"{pair.Key}\\\" TO \\\"/var/opt/mssql/data/{containerBackupFileName}.{pair.Value}\\\"");

            var query =
                $"/opt/mssql-tools/bin/sqlcmd -S {dbConfig.Host},{dbConfig.Port} -U {dbConfig.AdminUserName} -P '{dbConfig.AdminPassword}' " +
                $"-Q 'RESTORE DATABASE {dbConfig.DatabaseName} " +
                $"FROM DISK = \\\"/{backupFullName}\\\" WITH {string.Join(',', moveExpressions)}, REPLACE'";

            var output = CallDockerCommand($"exec -it {containerId} bash -c \"{query}\"", (string message) => logger.Log(InstallationMessage.Info(message)));

            return true;
        }
        private Dictionary<string, string> GetRestorationList(string containerId, DatabaseConfig dbConfig, string containerBackupFileName)
        {
            var script = $"/opt/mssql-tools/bin/sqlcmd -S {dbConfig.Host},{dbConfig.Port} -U {dbConfig.AdminUserName} -P '{dbConfig.AdminPassword}' " +
                         $"-Q 'RESTORE FILELISTONLY FROM DISK = \\\"/{containerBackupFileName}\\\"' | tr -s ' ' | cut -d ' ' -f 1-2";
            var output = CallDockerCommand($"exec -it {containerId} bash -c \"{script}\"");
            var parts = output.StandardOutput.Split(Environment.NewLine);
            return parts.Where(part => part.Contains("MsSQL_DATA")).Select(dataPart => dataPart.Split(" "))
                .ToDictionary(k => k[0], v => v[1].Split(".").Last());
        }

        private (string StandardOutput, string ErrorOutput) CallDockerCommand(string command,
            Action<string>? handler = null)
        {
            Process process = new Process();
            process.StartInfo.FileName = "docker";
            process.StartInfo.Arguments = command;
            process.StartInfo.UseShellExecute = false;

            var waitForMessages = handler != null;

            if (handler != null)
            {
                process.OutputDataReceived += (sender, e) =>
                {
                    if (e.Data != null)
                    {
                        handler(e.Data);
                        if (e.Data.Contains("RESTORE DATABASE successfully"))
                        {
                            waitForMessages = false;
                        }
                    }
                };
            }


            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.Start();
            if (handler != null)
            {
                process.BeginOutputReadLine();
            }

            process.WaitForExit();

            while (waitForMessages)
            {
                Thread.Sleep(150);
            }

            return handler == null ? 
                (process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd()):
                (string.Empty, string.Empty);
        }
    }
}
