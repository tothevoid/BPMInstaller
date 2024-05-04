namespace BPMInstaller.Core.Tests
{
    public static class InfrastructureConstants
    {
        public static class Postgres
        {
            public static string DistributivePath = "";

            public static string RestoreCliPath = "";

            public static string DockerImage = "pgsql";

            public static ushort LocalDatabasePort = 5433;

            public static ushort DockerDatabasePort = 5432;

            public static string AdminUserName = "postgres";

            public static string AdminPassword = "admin";
        }

        public static class SqlServer
        {
            public static string DistributivePath = "";

            public static string DockerImage = "mssql";

            public static ushort LocalDatabasePort = 1434;

            public static ushort DockerDatabasePort = 1433;

            public static string AdminUserName = "SA";

            public static string AdminPassword = "Test123!";
        }
    }
}
