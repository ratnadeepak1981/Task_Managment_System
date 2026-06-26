using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using TaskManagementSystem.DTO.Request;
using TaskManagementSystem.DTO.Response;
using TaskManagementSystem.Models;
using TaskManagementSystem.Repositories.Implementation;
using TaskManagementSystem.Repositories.Interface;
using TaskManagementSystem.Services.Interface;
using static Microsoft.Data.SqlClient.Internal.SqlClientEventSource;

namespace TaskManagementSystem.Services.Implementation
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        readonly IUserRepository _userRepository; // Add your user repository dependency
        public TaskService(ITaskRepository taskRepository, IUserRepository userRepository)
        {
            _taskRepository = taskRepository;
            _userRepository = userRepository;
        }

        public IEnumerable<TaskItemResponseDto> GetAllTasks()
        {
            return _taskRepository.GetAll().Select(MapToResponseDto);
        }
        // FIXED: Changed from IEnumerable to a single nullable object
        public TaskItemResponseDto? GetTaskById(int taskId)
        {
            var task = _taskRepository.GetTaskById(taskId);

            if (task == null)
            {
                return null;
            }
            //return task;
            return MapToResponseDto(task);
        }

        public List<TaskItemResponseDto>? GetTaskByName(string taskName)
        {
            var tasks = _taskRepository.GetTaskByName(taskName);

            if (tasks== null)
            {
                return null;
            }
            //return task;
           return tasks.Select(MapToResponseDto).ToList();
        }
        public bool CreateTask(CreateTaskItemDto dto, out TaskItemResponseDto? task, out string errorMessage)
        {
            task = null;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                errorMessage = "Task Title is required.";
                return false;
            }
            if (dto.Title.Length > 100)
            {
                errorMessage = "Task Titel cannot exeed 100 charcters.";
                return false;
            }
            if (!string.IsNullOrWhiteSpace(dto.Description))
            {
                if (dto.Description.Length > 500)
                {
                    errorMessage = "Task Description cannot exeed 500 charcters.";
                    return false;
                }
            }

            if (string.IsNullOrWhiteSpace(dto.Status))
            {
                errorMessage = "Task Status is required.";
                return false;
            }
            if (dto.UserId==0)
            {
                errorMessage = "Task User Id is required.";
                return false;
            }
            var user = _userRepository.GetUserById(dto.UserId);
           
            if (user == null)
            {
                errorMessage = $"User with ID {dto.UserId} does not exist.";
                return false;
            }
            if (dto.Status is not ("Todo" or "In Progress" or "Done"))
            {
                errorMessage = "Task Status is invalid. It must be 'Todo', 'In Progress', or 'Done'.";
                return false;
            }
            // 2. Map Input DTO to Entity (Separate Method)
            var newTask = MapToTaskItem(dto);

            // 3. Save to Repository
            var createdTask = _taskRepository.Add(newTask);
            
            if (createdTask == null)
            {
                errorMessage = "Failed to save the task to the database.";
                return false;
            }
                
            // 4. Map Saved Entity to Response DTO (Separate Method)
            if (createdTask != null)
            {
                task = MapToResponseDto(createdTask);
            }
     
            return true;
          
        }

        public bool UpdateTask(int taskId, UpdateTaskItemDto dto, out TaskItemResponseDto? task, out string errorMessage)
        {
            errorMessage = string.Empty;
            task = null;
            // 1. Validation Checks
            if (taskId <= 0)
            {
                errorMessage = "A valid Task Id is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(dto.Title))
            {
                errorMessage = "Task Title is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(dto.Description))
            {
                errorMessage = "Task Description is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(dto.Status))
            {
                errorMessage = "Task Status is required.";
                return false;
            }
            if (dto.UserId == 0)
            {
                errorMessage = "Task User Id is required.";
                return false;
            }

            // 2. Map DTO to Model
            var taskToUpdate = MapToTaskItem(dto);

            // 3. Call Repository Update passing taskId explicitly
            bool isUpdated = _taskRepository.Update(taskId, taskToUpdate);

            if (!isUpdated)
            {
                errorMessage = $"Task with ID {taskId} could not be found or updated.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(dto.Status))
            {
                errorMessage = "Task Status is required.";
                return false;
            }
            if (taskToUpdate != null)
            {
                task = MapToResponseDto(taskToUpdate);
            }
            return true;
        }
        public bool UpdateTaskStatus(int taskId, ChangeStatusDto dto, out TaskItemResponseDto? task, out string errorMessage)
        {
            errorMessage = string.Empty;
            task = null;
            if (dto.TaskId <= 0)
            {
                errorMessage = "A valid Task Id is required.";
                return false;
            }
            bool isStatusUpdated = _taskRepository.UpdateStatus(taskId,dto.Status );
          

            var updatedTaskModel = _taskRepository.GetTaskById(taskId);

            if (updatedTaskModel != null)
            {
                 task = MapToResponseDto(updatedTaskModel);
            }

            return isStatusUpdated;
        }
        public bool DeleteTask(int taskId, out TaskItemResponseDto? task, out string errorMessage)
        {
            errorMessage = string.Empty;
            task = null;
            if (taskId <= 0)
            {
                errorMessage = "A valid Task Id is required.";
                return false;
            }
            var taskToDelete = _taskRepository.GetTaskById(taskId);
            if (taskToDelete == null)
            {
                errorMessage = $"Task with ID {taskId} was not found.";
                return false;
            }
            bool isTaskDeleted = _taskRepository.Delete(taskId);
            if (!isTaskDeleted)
            {
                errorMessage = "Failed to delete the task from the database.";
                return false;
            }

            // 3. Map the fetched task entity to your Response DTO
            task = MapToResponseDto(taskToDelete);
            return true;
        }
        private static TaskItemResponseDto MapToResponseDto(TaskItem taskItem)
        {
            return new TaskItemResponseDto
            {
                TaskId = taskItem.TaskId,
                Title = taskItem.Title,
                Description = taskItem.Description,
                Status=taskItem.Status,
                CreatedDate = taskItem.CreatedDate,
                UserId =taskItem.UserId,
                UserName = taskItem.UserName
            };
        }
        
        private static TaskItem MapToTaskItem(CreateTaskItemDto dto)
        {
            if (dto == null)
            {
                return null;
            }

            return new TaskItem
            {
                Title = dto.Title,
                Description = dto.Description,
                Status = dto.Status,
                UserId = dto.UserId
                // TaskId is omitted so the database can auto-increment it
            };
        }

        // Separate mapping helper  for update Task item.
        private static TaskItem ? MapToTaskItem(UpdateTaskItemDto dto)
        {
            if (dto == null) return null;

            return new TaskItem
            {
                // TaskId does not need to be mapped here since it's passed separately to the repo
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                Status = dto.Status.Trim(),
                UserId = dto.UserId
            };
        }
    }
}
