using BPMInstaller.Core.Model.Docker;
using System.Diagnostics;
using BPMInstaller.Core.Model;
using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.Core.Utilities;

namespace BPMInstaller.Core.Services
{
    //TODO: separate into SqlServer and Postgres implementation + common logic service
    public class DockerService
    {
        private IInstallationLogger? Logger { get; }

        public DockerService(IInstallationLogger? logger = null)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        //TODO: get rid of that workaround with logger injection implementation
        private void LogMessage(string message) => Logger?.Log(InstallationMessage.Info(message));

        private const string SqlServerDataLocation = "/var/opt/mssql/data";

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
        public bool CopyFileIntoContainer(string filePath, string containerId, string containerFileName, DatabaseType databaseType)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }

            var backupName = GetBackupName(databaseType, containerFileName);
            var output = CallDockerCommand($"cp {filePath} {containerId}:/{backupName}");

            return !string.IsNullOrEmpty(output.StandardOutput) && string.IsNullOrEmpty(output.ErrorOutput);
        }

        private string GetBackupName(DatabaseType dbType, string databaseName)
        {
            switch (dbType)
            {
                case DatabaseType.PostgreSql:
                    return $"{databaseName}.backup";
                case DatabaseType.MsSql:
                    return $"{databaseName}.bak";
                default:
                    throw new NotImplementedException(dbType.ToString());
            }
        }

        public bool RestorePostgresBackup(string containerId, string userName, string databaseName)
        {
            var restorationDockerCommand = GetPostgresRestorationCommand(containerId, userName, databaseName);
            var restorationOutput = CallDockerCommand(restorationDockerCommand);
            return string.IsNullOrEmpty(restorationOutput.ErrorOutput);
        }

        private string GetPostgresRestorationCommand(string containerId, string userName, string databaseName)
        {
            var backupName = GetBackupName(DatabaseType.PostgreSql, databaseName);

            var restorationParameters = new[]
            {
                $"--username={userName}",
                $"--dbname={databaseName}",
                "--no-owner",
                "--no-privileges",
                $"./{backupName}"
            };

            var restorationScript = $"pg_restore {string.Join(" ", restorationParameters)}";
            return FormatDockerExecutableCommand(containerId, restorationScript);
        }

        public bool RestoreMsBackup(string containerId, DatabaseConfig dbConfig)
        {
            var dockerCommand = GetBackupRestorationDockerCommand(containerId, dbConfig);

            return CallDynamicDockerCommand(dockerCommand, message =>
            {
                LogMessage(message);
                return message.Contains("RESTORE DATABASE successfully");
            });
        }

        private string GetBackupRestorationDockerCommand(string containerId, DatabaseConfig dbConfig)
        {
            var backupName = GetBackupName(DatabaseType.MsSql, dbConfig.DatabaseName);
            var backupFiles = GetRestorationList(containerId, dbConfig, dbConfig.DatabaseName).ToList();

            var filesMoveExpressions = backupFiles.Select(file =>
            {
                var newFilePath = $"{SqlServerDataLocation}/{backupName}.{file.Extension}";
                return $"MOVE {TextUtilities.EscapeExpression(file.Name)} TO {TextUtilities.EscapeExpression(newFilePath)}";
            });

            var restorationQuery = $"RESTORE DATABASE {dbConfig.DatabaseName} " +
                        $"FROM DISK = {TextUtilities.EscapeExpression(backupName)} " +
                        $"WITH {string.Join(',', filesMoveExpressions)}, REPLACE";

            var restorationCommand = FormatSqlServerCommand(dbConfig, restorationQuery);
            return FormatDockerExecutableCommand(containerId, restorationCommand, true);
        }

        private IEnumerable<BackupPartFile> GetRestorationList(string containerId, DatabaseConfig dbConfig, string containerBackupFileName)
        {
            var fileListCommand = GetSqlServerFileListDockerCommand(containerId, dbConfig, containerBackupFileName);
            var rawBackupPartsInfo = CallDockerCommand(fileListCommand);

            return ParseRawBackupFileList(rawBackupPartsInfo.StandardOutput);
        }

        private string GetSqlServerFileListDockerCommand(string containerId, DatabaseConfig dbConfig, string containerBackupFileName)
        {
            var query = $"RESTORE FILELISTONLY FROM DISK = {TextUtilities.EscapeExpression(containerBackupFileName)}";
            var command = FormatSqlServerCommand(dbConfig, query);
            var commandOutputOperations = "-Q  | tr -s ' ' | cut -d ' ' -f 1-2";

            return FormatDockerExecutableCommand(containerId, $"{command} {commandOutputOperations}", true);
        }

        private IEnumerable<BackupPartFile> ParseRawBackupFileList(string rawFileList)
        {
            var dataParts = rawFileList
                .Split(Environment.NewLine)
                .Where(part => part.Contains("MsSQL_DATA"));

            return dataParts
                .Select(dataPart => dataPart.Split(" "))
                .Select(dataPart => new BackupPartFile(dataPart[0], dataPart[1].Split(".").Last()));
        }

        private string FormatDockerExecutableCommand(string containerId, string command, bool interactiveMode = false)
        {
            var interactiveFlag = interactiveMode ? "-it" : string.Empty;
            return $"exec {interactiveFlag} {containerId} bash -c \"{command}\"";
        }

        private string FormatSqlServerCommand(DatabaseConfig dbConfig, string query)
        {
            const string sqlServerCliLocation = "/opt/mssql-tools/bin/sqlcmd";
            var args = new[]
            {
                $"-S {dbConfig.Host},{dbConfig.Port}",
                $"-U {dbConfig.AdminUserName}",
                $"-P '{dbConfig.AdminPassword}'",
                $"-Q '{query}'"
            };

            return $"{sqlServerCliLocation} {string.Join(' ', args)}";
        }

        private (string StandardOutput, string ErrorOutput) CallDockerCommand(string command)
        {
            var process = GetBasicProcessConfiguration(command);
            process.Start();
            process.WaitForExit();

            return (process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
        }

        private bool CallDynamicDockerCommand(string command, Func<string, bool> outputHandler)
        {
            var process = GetBasicProcessConfiguration(command);
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

        private class BackupPartFile
        {
            public BackupPartFile(string name, string extension)
            {
                Name = name;
                Extension = extension;
            }

            public string Name { get; init; }

            public string Extension { get; init; }
        }
    }
}
