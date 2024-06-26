﻿using BPMInstaller.Core.Enums;
using BPMInstaller.Core.Interfaces;
using BPMInstaller.Core.Model;
using BPMInstaller.Core.Model.Runtime;
using BPMInstaller.Core.Utilities;
using System.Diagnostics;
using System.Text;
using RestorationResources = BPMInstaller.Core.Resources.InstallationResources.Database.Restoration;

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

        public string RestoreByDocker()
        {
            var newBackupPath = $"/{GetBackupName()}";

            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.CopyingBackupFile));
            var isCopied = DockerService.CopyFileIntoContainer(BackupRestorationConfig.BackupPath, BackupRestorationConfig.DockerImage, newBackupPath);

            if (!isCopied)
            {
                return RestorationResources.FileIsNotCopiedIntoContainer;
            }
            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.BackupFileCopied));

            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.SqlServer.BackupPartsExtractionStarted));
            var rawFileListResult = ExecuteServerFileListQuery(newBackupPath);
            var fileList = ParseRawBackupFileList(rawFileListResult).ToList();
            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.SqlServer.BackupPartsExtractionEnded));

            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.GeneratingDataMigrationCommand));
            var restorationQuery = CreateMoveQuery(fileList, DockerDataLocation, newBackupPath);
            var restorationCommand = FormatSqlCmdCommand(restorationQuery);
            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.DataMigrationCommandGenerated));

            const string cliSource = "/opt/mssql-tools/bin/sqlcmd";

            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.DataMigrationStarted));
            DockerService.ExecuteCommandInContainerWhile(BackupRestorationConfig.DockerImage, $"{cliSource} {restorationCommand}", message =>
            {
                InstallationLogger.Log(InstallationMessage.Info(message));
                return message.Contains("RESTORE DATABASE successfully");
            });
            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.DataMigrationEnded));

            return string.Empty;
        }

        //TODO: Remove duplication
        public string RestoreByCli()
        {
            var localDataDirectory = GetLocalDataLocation();
            var newBackupPath = Path.Combine(localDataDirectory, GetBackupName());

            File.Copy(BackupRestorationConfig.BackupPath, newBackupPath, true);

            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.SqlServer.BackupPartsExtractionStarted));
            var rawFileListResult = ExecuteServerFileListQuery(newBackupPath);
            var fileList = ParseRawBackupFileList(rawFileListResult).ToList();
            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.SqlServer.BackupPartsExtractionEnded));

            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.GeneratingDataMigrationCommand));
            var restorationQuery = CreateMoveQuery(fileList, localDataDirectory, newBackupPath);
            var restorationCommand = FormatSqlCmdCommand(restorationQuery);
            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.DataMigrationCommandGenerated));

            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.DataMigrationStarted));
            CallDynamicCommand(restorationCommand, message =>
            {
                InstallationLogger.Log(InstallationMessage.Info(message));
                return message.Contains("RESTORE DATABASE successfully");
            });

            InstallationLogger.Log(InstallationMessage.Info(RestorationResources.DataMigrationEnded));

            return string.Empty;
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
            var query = $"RESTORE FILELISTONLY FROM DISK = '{backupPath}'";
            var command = FormatSqlCmdCommand(query);

            var outputParts = new StringBuilder();

            bool HandleMessages(string message)
            {
                InstallationLogger.Log(InstallationMessage.Info(message));
                outputParts.AppendLine(message);
                return message.Contains("rows affected");
            }

            var executionResult = BackupRestorationConfig.RestorationKind == DatabaseDeploymentType.Docker
                ? ExecuteFileListCommandInDocker(command, HandleMessages)
                : ExecuteFileListCommandLocally(command, HandleMessages);

            if (!executionResult)
            {
                throw new ArgumentException(backupPath);
            }

            return outputParts.ToString();
        }

        private bool ExecuteFileListCommandInDocker(string command, Func<string, bool> handler)
        {
            return DockerService.ExecuteCommandInContainerWhile(BackupRestorationConfig.DockerImage, 
                $"/opt/mssql-tools/bin/sqlcmd {command}", handler);
        }

        private bool ExecuteFileListCommandLocally(string command, Func<string, bool> handler)
        {
            return new CommandLineQueryExecutor("sqlcmd")
                .AddParameter(command)
                .ExecuteWhile(handler);
        }


        // TODO: Remove
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

        //TODO: Remove
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

        //TODO: Remove
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
                $"-S {DatabaseConfig.Host}",
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
