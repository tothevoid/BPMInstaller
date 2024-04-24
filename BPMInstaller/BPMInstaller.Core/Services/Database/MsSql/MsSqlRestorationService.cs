using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;
using System.Diagnostics;
using BPMInstaller.Core.Enums;
using BPMInstaller.Core.Model.Runtime;
using System.Text;
using BPMInstaller.Core.Utilities;

namespace BPMInstaller.Core.Services.Database.MsSql
{
    // TODO: refactor to generic CLI calls
    public class MsSqlRestorationService: IDatabaseRestorationService
    {
        private const string DockerDataLocation = "/var/opt/mssql/data";

        private DatabaseConfig DatabaseConfig { get; }
        private BackupRestorationConfig BackupRestorationConfig { get; }
        private IInstallationLogger InstallationLogger { get; }
        private DockerService DockerService { get; }

        public MsSqlRestorationService(BackupRestorationConfig backupRestorationConfig, DatabaseConfig databaseConfig, IInstallationLogger installationLogger)
        {
            BackupRestorationConfig = backupRestorationConfig ?? throw new ArgumentNullException(nameof(backupRestorationConfig));
            DatabaseConfig = databaseConfig ?? throw new ArgumentNullException(nameof(databaseConfig));
            InstallationLogger = installationLogger ?? throw new ArgumentNullException(nameof(installationLogger));

            DockerService = new DockerService();
        }

        public bool RestoreByDocker()
        {
            var backupPath = $"/{GetBackupName()}";

            var isCopied = DockerService.CopyFileIntoContainer(BackupRestorationConfig.BackupPath, BackupRestorationConfig.DockerImage, backupPath);

            if (!isCopied)
            {
                return false;
            }

            var rawFileListQuery = GetSqlServerFileListQuery(backupPath);
            var outputParts = new StringBuilder();
            DockerService.ExecuteCommandInContainerWhile(BackupRestorationConfig.DockerImage, rawFileListQuery, message =>
            {
                InstallationLogger.Log(InstallationMessage.Info(message));
                outputParts.AppendLine(message);
                return !message.Contains("rows affected");
            });

            var fileList = ParseRawBackupFileList(outputParts.ToString()).ToList();
            var restorationQuery = CreateMoveQuery(fileList, DockerDataLocation, backupPath);
            var restorationCommand = FormatSqlCmdCommand(restorationQuery);

            var cliSource = "/opt/mssql-tools/bin/sqlcmd";

            return DockerService.ExecuteCommandInContainerWhile(BackupRestorationConfig.DockerImage, $"{cliSource} {restorationCommand}", message =>
            {
                InstallationLogger.Log(InstallationMessage.Info(message));
                return message.Contains("RESTORE DATABASE successfully");
            });
        }

        public bool RestoreByCli()
        {
            var localDataDirectory = GetLocalDataLocation();
            var newBackupPath = Path.Combine(localDataDirectory, GetBackupName());

            File.Copy(BackupRestorationConfig.BackupPath, newBackupPath, true);

            var fileListQuery = GetSqlServerFileListQuery(newBackupPath);
            
            //TODO: use synced version
            var outputParts = new StringBuilder();
            CallDynamicCommand(fileListQuery, message =>
            {
                InstallationLogger.Log(InstallationMessage.Info(message));
                outputParts.AppendLine(message);
                return !message.Contains("rows affected");
            });


            var fileList = ParseRawBackupFileList(outputParts.ToString()).ToList();
            var restorationQuery = CreateMoveQuery(fileList, localDataDirectory, newBackupPath);
            var restorationCommand = FormatSqlCmdCommand(restorationQuery);

            return CallDynamicCommand(restorationCommand, message =>
            {
                InstallationLogger.Log(InstallationMessage.Info(message));
                return message.Contains("RESTORE DATABASE successfully");
            });
        }

        private string CreateMoveQuery(IEnumerable<BackupPartFile> fileList, string sqlServerDataPath, string backupFilePath)
        {
            var filesMoveExpressions = fileList.Select(file =>
            {
                var newFilePath = Path.Combine(sqlServerDataPath, $"{DatabaseConfig.DatabaseName}.{file.Extension}");
                return $"MOVE '{file.Name}' TO '{newFilePath}'";
            });

            var queryParts = new[]
            {
                $"RESTORE DATABASE {DatabaseConfig.DatabaseName}",
                $"FROM DISK = '{backupFilePath}'",
                $"WITH {string.Join(',', filesMoveExpressions)}, REPLACE"
            };
            return string.Join(" ", queryParts);
        }

        private string GetLocalDataLocation()
        {
            var command = FormatSqlCmdCommand("select physical_name from sys.database_files where name = 'master'");
            var result = CallCmdCommand(command);

            var backupDirectory = result.Output
                .Split(Environment.NewLine)
                .FirstOrDefault(outputPart => outputPart.Contains("master.mdf"))?.Trim();

            if (string.IsNullOrEmpty(backupDirectory))
            {
                throw new NotImplementedException();
            }

            var backupsDirectory = Directory.GetParent(backupDirectory)?.FullName;

            if (string.IsNullOrEmpty(backupsDirectory))
            {
                throw new NotImplementedException();
            }

            return backupsDirectory;
        }

        private string ExecuteServerFileListQuery(string backupPath)
        {
            var prefix = BackupRestorationConfig.RestorationKind == DatabaseDeploymentType.Docker ?
                "/opt/mssql-tools/bin/sqlcmd " :
                "";

            var query = $"RESTORE FILELISTONLY FROM DISK = '{backupPath}'";
            return $"{prefix}{FormatSqlCmdCommand(query)}";
        }

        private string ExecuteFileListCommandInDocker()
        {
            DockerService.ExecuteCommandInContainerWhile();
        }

        private string ExecuteFileListCommandLocally()
        {
            new CommandLineQueryExecutor("sqlcmd");
        }


        // TODO: Move cli logic into separate class
        private bool CallDynamicCommand(string command, Func<string, bool> outputHandler)
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
            var cliSource = BackupRestorationConfig.RestorationKind == DatabaseDeploymentType.Docker ?
                "/opt/mssql-tools/bin/sqlcmd" :
                "sqlcmd";
            process.StartInfo.FileName = cliSource;
            process.StartInfo.Arguments = command;
            process.StartInfo.UseShellExecute = false;

            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;

            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;

            return process;
        }


        public (string Output, string ErrorOutput) CallCmdCommand(string command)
        {
            var process = GetBasicProcessConfiguration(command);

            process.Start();
            process.WaitForExit();

            return (process.StandardOutput.ReadToEnd(), process.StandardError.ReadToEnd());
        }

        private string FormatSqlCmdCommand(string query)
        {
            var wrappedQuery = BackupRestorationConfig.RestorationKind == DatabaseDeploymentType.Docker ? 
                TextUtilities.EscapeExpression(query) : 
                $"\"{query}\"";

            var args = new[]
            {
                $"-S {DatabaseConfig.Host},{DatabaseConfig.Port}",
                $"-U {DatabaseConfig.AdminUserName}",
                "-C",
                $"-P {DatabaseConfig.AdminPassword}",
                $"-Q {wrappedQuery}",
            };

            return $"{string.Join(' ', args)}";
        }

        private IEnumerable<BackupPartFile> ParseRawBackupFileList(string commandOutput)
        {
            var rows = SqlCmdParser.ParseQueryResponse(commandOutput);
            return rows
                .Select(row => row.Split("\t"))
                .Select(dataPart => new BackupPartFile(dataPart[0], dataPart[1].Split(".").Last()));
        }

        private string GetBackupName() => 
            $"{DatabaseConfig.DatabaseName}.bak";

        private class BackupPartFile
        {
            public BackupPartFile(string name, string extension)
            {
                Name = name;
                Extension = extension;
            }

            public string Name { get; }

            public string Extension { get; }
        }
    }
}
