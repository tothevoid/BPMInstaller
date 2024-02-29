using BPMInstaller.Core.Model;
using Npgsql;
using BPMInstaller.Core.Interfaces;
using System.Diagnostics;
using System.Collections.Generic;
using BPMInstaller.Core.Enums;

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
        public string ValidateConnection()
        {
            /*
             TODO: handle possible cases
             * Not enough permissions
             * Has active connections 
             * Database already exists (also add a checkbox that ignores that)
            */
            try
            {
                using var con = new NpgsqlConnection(GetConnectionString());
                con.Open();
                using var cmd = new NpgsqlCommand("SELECT version()", con);
                var result = cmd.ExecuteNonQuery() != 0;
                con.Close();
                return string.Empty;
            }
            catch (Exception ex)
            {
                return ex.Message;
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
            DatabaseConfig.RestorationKind switch
            {
                DatabaseDeploymentType.Cli => RestoreByCli(),
                DatabaseDeploymentType.Docker => RestoreByDocker()
            };

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

        /// <summary>
        /// Удаление назначенных лицензий и назначение всех на администратора
        /// </summary>
        /// <param name="userName">Пользователь администратора</param>
        public bool ApplyAdministratorLicenses(string userName)
        {
            var result = true;

            using var con = new NpgsqlConnection(GetConnectionString(DatabaseConfig.DatabaseName));

            con.Open();
            using (var removeLicensesCommand = new NpgsqlCommand($"TRUNCATE TABLE \"SysLicUser\"", con))
            {
                removeLicensesCommand.ExecuteScalar();
            }

            var licensesInsertQuery = "INSERT INTO \"SysLicUser\" (\"SysLicPackageId\", \"SysUserId\", \"Active\")" +
                "SELECT \"SysLic\".\"SysLicPackageId\", \"SysAdminUnit\".\"Id\", '1'" +
                "FROM \"SysLic\"" +
                $"inner join \"SysAdminUnit\" on \"SysAdminUnit\".\"Name\" = '{userName}'" +
                "WHERE \"Count\" != 0";

            using (var insertLicensesCommand = new NpgsqlCommand(licensesInsertQuery, con))
            {
                result = insertLicensesCommand.ExecuteNonQuery() != 0;
            }

            con.Close();
            return result;
        }
        
        /// <summary>
        /// Остановка всех активных подключений к БД
        /// </summary>
        /// <param name="databaseName">Название БД</param>
        public void TerminateAllActiveSessions(string databaseName)
        {
            //try
            //{
            //    using var con = new NpgsqlConnection(GetConnectionString(DatabaseConfig.DatabaseName));
            //    var commandText = "select pg_terminate_backend(pid) from pg_stat_activity\n" +
            //        $"where datname = '{databaseName}' and pid != pg_backend_pid() and leader_pid IS NULL";
            //    con.Open();
            //    using var cmd = new NpgsqlCommand(commandText, con);
            //    cmd.ExecuteNonQuery();
            //    con.Close();
            //}
            //catch (Exception ex)
            //{
            //    ;
            //}
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
