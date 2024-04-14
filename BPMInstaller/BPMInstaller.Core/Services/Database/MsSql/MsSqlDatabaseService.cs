using BPMInstaller.Core.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;
using BPMInstaller.Core.Model;

namespace BPMInstaller.Core.Services.Database.MsSql
{
    public class MsSqlDatabaseService: IDatabaseService
    {
        private DatabaseConfig DatabaseConfig { get; }

        public MsSqlDatabaseService(DatabaseConfig dbConfig)
        {
            DatabaseConfig = dbConfig;
        }

        public string CreateDatabase()
        {
            using var con = new SqlConnection(GetConnectionString("master"));

            try
            {
                con.Open();
                using (var cmd = new SqlCommand($"DROP DATABASE IF EXISTS {DatabaseConfig.DatabaseName}", con))
                {
                    //TODO: Fix database active connection exception
                    cmd.ExecuteScalar();
                }

                using (var cmd = new SqlCommand($"CREATE DATABASE {DatabaseConfig.DatabaseName}", con))
                {
                    cmd.ExecuteScalar();
                }
                con.Close();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            return string.Empty;
        }

        public bool DisableForcePasswordChange(string userName)
        {
            using var con = new SqlConnection(GetConnectionString(DatabaseConfig.DatabaseName));
            con.Open();
            using var cmd = new SqlCommand($"UPDATE SysAdminUnit SET ForceChangePassword = '0' WHERE Name='{userName}'", con);
            var result = cmd.ExecuteNonQuery() != 0;
            con.Close();
            return result;
        }

        public bool UpdateCid(long cId)
        {
            using var con = new SqlConnection(GetConnectionString(DatabaseConfig.DatabaseName));

            var commandText = $"update SysSettingsValue set TextValue = '{cId}' " +
                              $"where SysSettingsId in (select Id from SysSettings where SysSettings.Code = 'CustomerId')";
            con.Open();
            using var cmd = new SqlCommand(commandText, con);
            var result = cmd.ExecuteNonQuery() != 0;
            con.Close();

            return result;
        }

        public bool ApplyAdministratorLicenses(string userName)
        {
            var result = true;

            using var con = new SqlConnection(GetConnectionString(DatabaseConfig.DatabaseName));

            con.Open();
            using (var removeLicensesCommand = new SqlCommand($"TRUNCATE TABLE SysLicUser", con))
            {
                removeLicensesCommand.ExecuteScalar();
            }

            var licensesInsertQuery = "INSERT INTO SysLicUser (SysLicPackageId, SysUserId, Active)" +
                                      "SELECT SysLic.SysLicPackageId, SysAdminUnit.Id, '1'" +
                                      "FROM SysLic" +
                                      $"inner join SysAdminUnit on SysAdminUnit.Name = '{userName}'" +
                                      "WHERE Count != 0";

            using (var insertLicensesCommand = new SqlCommand(licensesInsertQuery, con))
            {
                result = insertLicensesCommand.ExecuteNonQuery() != 0;
            }

            con.Close();
            return result;
        }

        public string ValidateConnection()
        {
            SqlConnection connection = new SqlConnection(GetConnectionString("master"));
            try
            {
                connection.Open();
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (connection.State == ConnectionState.Open)
                {
                    connection.Close();
                }
            }

            return string.Empty;
        }

        private string GetConnectionString(string databaseName) =>
            $"Server={DatabaseConfig.Host},{DatabaseConfig.Port};" +
            $"User Id={DatabaseConfig.AdminUserName};" +
            $"Password={DatabaseConfig.AdminPassword};" +
            $"Database={databaseName};" +
            "TrustServerCertificate=True";


        public void TerminateAllActiveSessions(string databaseConfigDatabaseName)
        {
            return;
        }
    }
}
