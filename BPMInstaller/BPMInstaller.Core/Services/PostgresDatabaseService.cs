using BPMInstaller.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using BPMInstaller.Core.Interfaces;
using System.Security.AccessControl;
using System.Xml.Linq;
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
            var process = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.FileName = Path.Combine("PostgreSQL", "postgresql-restore.bat");
            startInfo.Arguments = $@"{DatabaseConfig.Host} {DatabaseConfig.Port} {DatabaseConfig.UserName}:{DatabaseConfig.Password} {DatabaseConfig.DatabaseName} ""{DatabaseConfig.BackupPath}""";
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
            process.Close();
        }

        private string GetConnectionString(string database = "postgres") =>
            $"Host={DatabaseConfig.Host};Username={DatabaseConfig.UserName};Password={DatabaseConfig.Password};Database={database}";
        
    }
}
