namespace BPMInstaller.Core.Tests
{
    public static class InfrastructureConstants
    {
        public static class Postgres
        {
            public static string DistributivePath = "C:\\Users\\tothe\\Desktop\\all\\Distr13PG";

            public static string RestoreCliPath = "C:\\Program Files\\PostgreSQL\\16\\bin\\pg_restore.exe";

            public static string DockerImage = "pgsql_db_v2";

            public static ushort LocalDatabasePort = 5432;

            public static ushort DockerDatabasePort = 5433;

            public static string AdminUserName = "postgres";

            public static string AdminPassword = "admin";
        }

        public static class SqlServer
        {
            public static string DistributivePath = "C:\\Users\\tothe\\Desktop\\all\\Distr13MS";

            public static string DockerImage = "mssql";

            public static ushort LocalDatabasePort = 1433;

            public static ushort DockerDatabasePort = 1434;

            public static string AdminUserName = "SA";

            public static string AdminPassword = "Test123!";
        }
    }
}