using BPMInstaller.Core.Model;
using Npgsql;
using BPMInstaller.Core.Interfaces;
using System.Diagnostics;

namespace BPMInstaller.Core.Services
{
    /// <summary>
    /// Сервис взаимодействия с бд Postgres
    /// </summary>
    public class PostgresDatabaseService : IDatabaseService
    {
        private DatabaseConfig DatabaseConfig { get; }

        public PostgresDatabaseService(DatabaseConfig databaseConfig)
        {
            DatabaseConfig = databaseConfig;
        }

        /// <inheritdoc cref="IDatabaseService.ValidateConnection"/>
        public bool ValidateConnection()
        {
            try
            {
                using var con = new NpgsqlConnection(GetConnectionString());
                con.Open();
                using var cmd = new NpgsqlCommand("SELECT version()", con);
                var result = cmd.ExecuteNonQuery() != 0;
                con.Close();
                return result;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <inheritdoc cref="IDatabaseService.CreateDatabase"/>
        public string CreateDatabase()
        {
            using var con = new NpgsqlConnection(GetConnectionString());

            try
            {
                con.Open();
                using (var cmd = new NpgsqlCommand($"DROP DATABASE IF EXISTS {DatabaseConfig.DatabaseName}", con))
                {
                    //TODO: Fix database active connection exception
                    cmd.ExecuteScalar();
                }

                using (var cmd = new NpgsqlCommand($"CREATE DATABASE {DatabaseConfig.DatabaseName}", con))
                {
                    cmd.ExecuteScalar();
                }
                con.Close();
            }
            catch (PostgresException ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }

        /// <inheritdoc cref="IDatabaseService.RestoreDatabase"/>
        public bool RestoreDatabase() =>
            DatabaseConfig.HostedByDocker ?
                RestoreByDocker() :
                RestoreByCli();

        /// <inheritdoc cref="IDatabaseService.DisableForcePasswordChange(string)"/>
        public bool DisableForcePasswordChange(string userName)
        {
            using var con = new NpgsqlConnection(GetConnectionString(DatabaseConfig.DatabaseName));
            con.Open();
            using var cmd = new NpgsqlCommand($"UPDATE \"SysAdminUnit\" SET \"ForceChangePassword\" = '0' WHERE \"Name\"='{userName}'", con);
            var result = cmd.ExecuteNonQuery() != 0;
            con.Close();

            return result;
        }

        /// <inheritdoc cref="IDatabaseService.UpdateCid(long)"/>
        public bool UpdateCid(long cId)
        {
            using var con = new NpgsqlConnection(GetConnectionString(DatabaseConfig.DatabaseName));

            var commandText = $"update \"SysSettingsValue\" set \"TextValue\" = '{cId}' " +
                $"where \"SysSettingsId\" in (select \"Id\" from \"SysSettings\" where \"SysSettings\".\"Code\" = 'CustomerId')";
            con.Open();
            using var cmd = new NpgsqlCommand(commandText, con);
            var result = cmd.ExecuteNonQuery() != 0;
            con.Close();

            return result;
        }

        private bool RestoreByCli()
        {
            Process process = new Process();
            var backupFileName = Path.GetFileName(DatabaseConfig.BackupPath);
            process.StartInfo.WorkingDirectory = DatabaseConfig.BackupPath.Substring(0, DatabaseConfig.BackupPath.Length - backupFileName.Length - 1);
            process.StartInfo.FileName = $"{DatabaseConfig.RestorationCliLocation}/pg_restore.exe";
            process.StartInfo.Arguments = $"--port={DatabaseConfig.Port} --username={DatabaseConfig.AdminUserName} --dbname={DatabaseConfig.DatabaseName} --no-owner --no-privileges ./{backupFileName}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.EnvironmentVariables["PGPASSWORD"] = DatabaseConfig.AdminPassword;

            process.Start();
            process.WaitForExit();
            var output = process.StandardError.ReadToEnd();
            return string.IsNullOrEmpty(output);
        }

        private bool RestoreByDocker()
        {
            var interactor = new DockerService();
            var containers = interactor.GetActiveContainers();
            var postgresContainer = containers.FirstOrDefault(container => container.Value.ToLower().Contains("postgres"));

            if (string.IsNullOrEmpty(postgresContainer.Key))
            {
                return false;
            }
            
            var backupName = DateTime.Now.ToString("dd-MM-HH:mm.backup");
            interactor.CopyBackup(DatabaseConfig.BackupPath, postgresContainer.Key, backupName);
            interactor.RestoreBackup(postgresContainer.Key, DatabaseConfig.AdminUserName, DatabaseConfig.DatabaseName, backupName);
            return true;
        }

        private string GetConnectionString(string database = "postgres") =>
            $"Host={DatabaseConfig.Host};Username={DatabaseConfig.AdminUserName};Password={DatabaseConfig.AdminPassword};Database={database};Port={DatabaseConfig.Port}";
        
    }
}
