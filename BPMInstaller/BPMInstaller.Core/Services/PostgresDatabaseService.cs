using BPMInstaller.Core.Model;
using Npgsql;
using BPMInstaller.Core.Interfaces;
using System.Diagnostics;

namespace BPMInstaller.Core.Services
{
    public class PostgresDatabaseService : IDatabaseService
    {
        private DatabaseConfig DatabaseConfig { get; }

        public PostgresDatabaseService(DatabaseConfig databaseConfig)
        {
            DatabaseConfig = databaseConfig;
        }

        public bool ValidateConnection()
        {
            try
            {
                using var con = new NpgsqlConnection(GetConnectionString());
                con.Open();

                var sql = "SELECT version()";

                using var cmd = new NpgsqlCommand(sql, con);
                var result = cmd.ExecuteScalar()?.ToString();
                return !string.IsNullOrEmpty(result);
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool CreateDatabase()
        {
            using var con = new NpgsqlConnection(GetConnectionString());
            
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

            return true;
        }

        public void RestoreDatabase()
        {
            if (DatabaseConfig.DatabaseMode == Model.Enums.DatabaseMode.Docker)
            {
                RestoreByDocker();
            } 
            else
            {
                RestoreByCli();
            }
        }

        public void SuperuserPasswordFix(ApplicationConfig appConfig)
        {
            using var con = new NpgsqlConnection(GetConnectionString(DatabaseConfig.DatabaseName));

            con.Open();

            using (var cmd = new NpgsqlCommand("UPDATE \"SysAdminUnit\" SET \"ForceChangePassword\" = '0' WHERE \"Name\"='Supervisor'", con))
            {
                cmd.ExecuteScalar();
            }

            con.Close();

        }

        public void UpdateCid(LicenseConfig licConfig)
        {
            using var con = new NpgsqlConnection(GetConnectionString(DatabaseConfig.DatabaseName));

            con.Open();

            using (var cmd = new NpgsqlCommand($"update \"SysSettingsValue\" set \"TextValue\" = '{licConfig.CId}' where \"SysSettingsId\" in (select \"Id\" from \"SysSettings\" where \"SysSettings\".\"Code\" = 'CustomerId')", con))
            {
                cmd.ExecuteScalar();
            }

            con.Close();
        }

        private bool RestoreByCli()
        {
            Process process = new Process();
            var backupFileName = Path.GetFileName(DatabaseConfig.BackupPath);
            process.StartInfo.WorkingDirectory = DatabaseConfig.BackupPath.Substring(0, DatabaseConfig.BackupPath.Length - backupFileName.Length - 1);
            process.StartInfo.FileName = $"{DatabaseConfig.RestorationCliLocation}/pg_restore.exe";
            process.StartInfo.Arguments = $"--port={DatabaseConfig.Port} --username={DatabaseConfig.UserName} --dbname={DatabaseConfig.DatabaseName} --no-owner --no-privileges ./{backupFileName}";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.EnvironmentVariables["PGPASSWORD"] = DatabaseConfig.Password;

            process.Start();
            process.WaitForExit();
            var output = process.StandardError.ReadToEnd();
            return string.IsNullOrEmpty(output);
        }

        private void RestoreByDocker()
        {
            var interactor = new DockerService();
            var containers = interactor.GetActiveContainers();
            var postgresContainer = containers.FirstOrDefault(container => container.Value.ToLower().Contains("postgres"));
            if (!string.IsNullOrEmpty(postgresContainer.Key))
            {
                var backupName = DateTime.Now.ToString("dd-MM-HH:mm.backup");
                interactor.CopyBackup(DatabaseConfig.BackupPath, postgresContainer.Key, backupName);
                interactor.RestoreBackup(postgresContainer.Key, DatabaseConfig.UserName, DatabaseConfig.DatabaseName, backupName);
            }
        }

        private string GetConnectionString(string database = "postgres") =>
            $"Host={DatabaseConfig.Host};Username={DatabaseConfig.UserName};Password={DatabaseConfig.Password};Database={database};Port={DatabaseConfig.Port}";
        
    }
}
