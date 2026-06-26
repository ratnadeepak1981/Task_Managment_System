using Microsoft.Data.SqlClient;
using TaskManagementSystem.Models;
using TaskManagementSystem.Repositories.Interface;
using static Microsoft.Data.SqlClient.Internal.SqlClientEventSource;
namespace TaskManagementSystem.Repositories.Implementation
{
    public class UserRepository :IUserRepository
    {
        private readonly string _connectionString;
        public UserRepository(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            _connectionString = connectionString ?? string.Empty;
        }

        public IEnumerable<User> GetAll()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                return new List<User>();
            }

            const string sql = """
            SELECT u.UserId, u.UserName, u.Email
            FROM Users u
            ORDER BY u.UserId
            """;

            var users = new List<User>();

            // SqlConnection opens a connection to SQL Server.
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // SqlCommand holds the SQL text and parameters.
            using var command = new SqlCommand(sql, connection);

            // SqlDataReader reads rows returned by a SELECT query.
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                users.Add(MapUser(reader));
               
            }

            return users;
        }


        public User? GetUserById(int userId)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                return new User();
            }

            const string sql = """
            SELECT u.UserId, u.UserName, u.Email
            FROM Users u
            WHERE u.UserId = @UserId
            """;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = command.ExecuteReader();

            return reader.Read() ? MapUser(reader) : null;
        }

        public UserWithTasks? GetTasksByUserId(int userId)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                return null;
            }

            const string sql = """
            SELECT U.UserId, U.UserName, U.Email, 
                   T.TaskId, T.Title, T.Description, T.Status, T.CreatedDate
            FROM Users U
            LEFT JOIN Tasks T ON U.UserId = T.UserId
            WHERE U.UserId = @UserId
            """;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);

            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = command.ExecuteReader();

            UserWithTasks? userWithTasks=null;

            // Loop through all joined rows(One User to Many Tasks)
             while (reader.Read())
            {
                // Instantiate container exactly once using data from first found row
                if (userWithTasks == null)
                {
                    userWithTasks = MapUserTasksContainer(reader); 
              
                }

                // If user has tasks, parse and append them (safely avoids LEFT JOIN null rows)
                if (!reader.IsDBNull(reader.GetOrdinal("TaskId")))
                {
                    userWithTasks.Tasks.Add(MapUserTaskItem(reader));
                }
            }

            // CORRECTED: Return payload container directly instead of advancing reader
            return userWithTasks;
        }

        public bool GetUserByEmail(string userEmail)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                return false;
            }

            const string sql = "SELECT COUNT(1) FROM Users WHERE Email = @Email";

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@Email", userEmail);

            // ExecuteScalar returns the single number result of COUNT(1)
            int count = Convert.ToInt32(command.ExecuteScalar());

            // Returns true if the count is greater than 0
            return count > 0;
        }

        public User Add(User user)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                return user;
            }

            const string sql = """
            INSERT INTO Users (UserName, Email)
            OUTPUT INSERTED.UserId
            VALUES (@UserName, @UserEmail)
            """;

            //user.CreatedDate = DateTime.Now;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@UserName", user.UserName);
            command.Parameters.AddWithValue("@UserEmail", user.UserEmail);

            //command.Parameters.AddWithValue("@CreatedDate", user.CreatedDate);

            // ExecuteScalar returns the first column of the first row (serId from OUTPUT).
            var newUserId = Convert.ToInt32(command.ExecuteScalar());

            var loadedUser = GetUserById(newUserId);
            if (loadedUser != null)
            {
                return loadedUser;
            }

            user.UserId = newUserId;
            return user;
        }

        private static User MapUser(SqlDataReader reader)
        {
            return new User
            {
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                UserName = reader.GetString(reader.GetOrdinal("UserName")),
                UserEmail = reader.GetString(reader.GetOrdinal("Email")),
            };
        }
       
        // SEPARATE FUNCTION 1: Maps the base User container data
        private static UserWithTasks MapUserTasksContainer(SqlDataReader reader)
        {
            return new UserWithTasks
            {
                // Map fields directly into the inner UserProfile property
                UserProfile = new User
                {
                    UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                    UserName = reader.GetString(reader.GetOrdinal("UserName")),
                    UserEmail = reader.GetString(reader.GetOrdinal("Email"))
                },
                Tasks = new List<TaskItem>()
            };
        }

        // SEPARATE FUNCTION 2: Maps an individual tasks item from the active row
        private static TaskItem MapUserTaskItem(SqlDataReader reader)
        {
            return new TaskItem
            {
                TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                Title = reader.GetString(reader.GetOrdinal("Title")),
                Description = reader.GetString(reader.GetOrdinal("Description")),
                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),
                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                UserName= reader.GetString(reader.GetOrdinal("UserName"))
            };
        }
    }
}
