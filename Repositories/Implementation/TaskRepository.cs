using Microsoft.Data.SqlClient;
using TaskManagementSystem.Models;
using TaskManagementSystem.Repositories.Interface;
using TaskManagementSystem.DTO;
namespace TaskManagementSystem.Repositories.Implementation
{
    public class TaskRepository : ITaskRepository
    {
        private readonly string _connectionString;
        public TaskRepository(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            _connectionString = connectionString ?? string.Empty;
        }

        public IEnumerable<TaskItem> GetAll()
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                return new List<TaskItem>();
            }

            const string sql = """
            SELECT T.TaskId, T.Title, T.Description, T.Status, T.CreatedDate, U.UserId, U.UserName
            FROM Tasks T
            INNER JOIN Users U ON T.UserId = U.UserId
            ORDER BY T.TaskId
            """;

            var tasks = new List<TaskItem>();

            // SqlConnection opens a connection to SQL Server.
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            // SqlCommand holds the SQL text and parameters.
            using var command = new SqlCommand(sql, connection);

            // SqlDataReader reads rows returned by a SELECT query.
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                tasks.Add(MapTask(reader));
            }

            return tasks;
        }

        public TaskItem? GetTaskById(int taskId)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                return null;
            }

            const string sql = """
            SELECT T.TaskId, T.Title, T.Description, T.Status, T.CreatedDate, U.UserId, U.UserName
            FROM Tasks T
            INNER JOIN Users U ON T.UserId = U.UserId
            WHERE T.TaskId = @TaskId
            """;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@TaskId", taskId);

            using var reader = command.ExecuteReader();
            return reader.Read() ? MapTask(reader) : null;
        }
        private static TaskItem MapTask(SqlDataReader reader)
        {
            return new TaskItem
            {
                TaskId = reader.GetInt32(reader.GetOrdinal("TaskId")),
                Title = reader.GetString(reader.GetOrdinal("Title")),

                //Description = reader.GetString(reader.GetOrdinal("Description")),
                // 2. Check for DBNull. If true, assign null (or string.Empty)
                Description = reader.IsDBNull(reader.GetOrdinal("Description")) ? null : reader.GetString(reader.GetOrdinal("Description")),

                Status = reader.GetString(reader.GetOrdinal("Status")),
                CreatedDate = reader.GetDateTime(reader.GetOrdinal("CreatedDate")),

                UserId = reader.GetInt32(reader.GetOrdinal("UserId")),
                UserName = reader.GetString(reader.GetOrdinal("UserName"))

            };
        }

        public List<TaskItem>? GetTaskByName(string taskName)
        {
            List<TaskItem> tasks = new List<TaskItem>();
            if (string.IsNullOrEmpty(_connectionString))
            {
                return null;
            }

            string sql = """
            SELECT T.TaskId, T.Title, T.Description, T.Status, T.CreatedDate, U.UserId, U.UserName
            FROM Tasks T
            INNER JOIN Users U ON T.UserId = U.UserId
            """;

            // Use LIKE operator for partial matching
            if (!string.IsNullOrEmpty(taskName))
            {
                sql += "  WHERE T.Title LIKE @Title";
            }

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);

            if (!string.IsNullOrEmpty(taskName))
            {
                // Wrap search term with wildcard percentages
                command.Parameters.AddWithValue("@Title", $"%{taskName}%");
            }
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                tasks.Add(MapTask(reader));
            }

            return tasks;
        }
      
        public TaskItem Add(TaskItem task)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                return task;
            }

            const string sql = """
            INSERT INTO Tasks (Title, Description, Status, CreatedDate,UserId)
            OUTPUT INSERTED.TaskId
            VALUES (@Title, @Description, @Status, @CreatedDate,@UserId)
            """;

            // Set fallback tracking property if not initialized
            if (task.CreatedDate == default)
            {
                task.CreatedDate = DateTime.Now;
            }

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);
            // FIXED: Safely mapped database parameters directly to the task properties
            command.Parameters.AddWithValue("@Title", task.Title ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Status", task.Status ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@CreatedDate", task.CreatedDate);


            // FIXED: Added the UserId parameter binding
            command.Parameters.AddWithValue("@UserId", task.UserId);

            // ExecuteScalar returns the first column of the first row (TaskId from OUTPUT).
            var newTaskId = Convert.ToInt32(command.ExecuteScalar());

            // FIXED: Refresh object mapping state natively using the retrieved ID
            var loadedTask = GetTaskById(newTaskId);
            if (loadedTask != null)
            {
                return loadedTask;
            }
            // Fallback: If GetTaskById fails for some reason, update the original object's ID and return i
            task.TaskId = newTaskId;
           
            return task;
        }

        public bool Update(int taskId, TaskItem task)
        {
            if (string.IsNullOrEmpty(_connectionString) || task == null)
            {
                return false;
            }

            const string sql = """
                UPDATE Tasks 
                SET Title = @Title, 
                    Description = @Description, 
                    Status = @Status, 
                    UserId = @UserId
                WHERE TaskId = @TaskId
                """;

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            using var command = new SqlCommand(sql, connection);

            // Bind the taskId parameter explicitly from the method argument
            command.Parameters.AddWithValue("@TaskId", taskId);
            command.Parameters.AddWithValue("@Title", task.Title ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Description", task.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@Status", task.Status ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@UserId", task.UserId);

            int rowsAffected = command.ExecuteNonQuery();
            return rowsAffected > 0;
        }

        public bool UpdateStatus(int taskId, string status)
        {
            if (string.IsNullOrEmpty(_connectionString) || status == null)
            {
                return false;
            }

            const string sql = """
                UPDATE Tasks 
                SET Status = @Status 
                WHERE TaskId = @TaskId
                """;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(sql, connection);

                // Bind parameters using AddWithValue
                command.Parameters.AddWithValue("@TaskId", taskId);
                command.Parameters.AddWithValue("@Status", status ?? (object)DBNull.Value);

                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                // Log the error here if needed
                return false;
            }
        }

        public bool Delete(int taskId)
        {
            if (string.IsNullOrEmpty(_connectionString))
            {
                return false;
            }

            const string sql = """
                DELETE FROM Tasks 
                WHERE TaskId = @TaskId
                """;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                connection.Open();

                using var command = new SqlCommand(sql, connection);

                // Bind parameters using AddWithValue
                command.Parameters.AddWithValue("@TaskId", taskId);
                int rowsAffected = command.ExecuteNonQuery();
                return rowsAffected > 0;
            }
            catch (SqlException ex)
            {
                // Log the error here if needed
                return false;
            }
        }

    }
}
