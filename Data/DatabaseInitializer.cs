using Microsoft.Data.SqlClient;
namespace TaskManagementSystem.Data
{
    public class DatabaseInitializer
    {
        private readonly IConfiguration _configuration;

        public DatabaseInitializer(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void Initialize()
        {
            var connectionString = _configuration.GetConnectionString("DefaultConnection");
            if (connectionString == null)
            {
                Console.WriteLine("Error: Connection string 'DefaultConnection' is missing.");
                return;
            }

            CreateDatabaseIfNotExists(connectionString);
            RunMigrations(connectionString);
            SeedSampleDataIfEmpty(connectionString);

            Console.WriteLine("Database ready.");
        }

        private void CreateDatabaseIfNotExists(string connectionString)
        {

            // Use SqlConnectionStringBuilder to safely mutate the database target
            var builder = new SqlConnectionStringBuilder(connectionString)
            {
                InitialCatalog = "master"
            };
            var masterConnectionString = builder.ConnectionString;

            // Connect to master first so we can create the application database.
            //var masterConnectionString = connectionString
            //    .Replace("Database=TaskManagementDb;", "Database=master", StringComparison.OrdinalIgnoreCase);

            const string createDatabaseSql = """
            IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'TaskManagementDb')
            BEGIN
                CREATE DATABASE TaskManagementDb;
            END
            """;

            using var connection = new SqlConnection(masterConnectionString);
            using var command = new SqlCommand(createDatabaseSql, connection);

            connection.Open();
            command.ExecuteNonQuery();
        }

        private void RunMigrations(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            ExecuteNonQuery(connection, MigrationSqls.CreateUsersTable);
            ExecuteNonQuery(connection, MigrationSqls.CreateTasksTable);
        }

        private void SeedSampleDataIfEmpty(string connectionString)
        {
            using var connection = new SqlConnection(connectionString);
            connection.Open();

            var usersCount = Convert.ToInt32(ExecuteScalar(connection, MigrationSqls.CountUsers));
            if (usersCount == 0)
            {
                ExecuteNonQuery(connection, MigrationSqls.InsertSampleUsers);
            }

            var taskCount = Convert.ToInt32(ExecuteScalar(connection, MigrationSqls.CountTasks));
            if (taskCount == 0)
            {
                ExecuteNonQuery(connection, MigrationSqls.InsertSampleTasks);
            }
        }

        private static void ExecuteNonQuery(SqlConnection connection, string sql)
        {
            using var command = new SqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private static object? ExecuteScalar(SqlConnection connection, string sql)
        {
            using var command = new SqlCommand(sql, connection);
            return command.ExecuteScalar();
        }
    }
}
