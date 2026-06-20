using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using TaskManagementSystem.DTO;
using TaskManagementSystem.Models;
using TaskManagementSystem.Repositories.Implementation;
using TaskManagementSystem.Repositories.Interface;
using TaskManagementSystem.Services.Interface;

namespace TaskManagementSystem.Services.Implementation
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;

        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
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

        public TaskItemResponseDto? GetTaskByName(string taskName)
        {
            var task = _taskRepository.GetTaskByName(taskName);

            if (task == null)
            {
                return null;
            }
            //return task;
            return MapToResponseDto(task);
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
            if (dto.UserId==0)
            {
                errorMessage = "Task User Id is required.";
                return false;
            }
            /*
            var newTask = new TaskItem
            {
                Title = dto.Title.Trim(),
                Description = dto.Description.Trim(),
                Status = dto.Status.Trim(),
                UserId=dto.UserId
            };
            */
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
            //task = MapToResponseDto(createdTask);
            if (createdTask != null)
            {
                /*
                var mappedTask = new TaskItemResponseDto
                {
                    TaskId = createdTask.TaskId,
                    Title = createdTask.Title,
                    Description = createdTask.Description,
                    Status = createdTask.Status,
                    CreatedDate = createdTask.CreatedDate,
                    UserId = createdTask.UserId,
                    UserName = createdTask.UserName,
                };                                  
                */
                // task = mappedTask;
                task = MapToResponseDto(createdTask);
            }
     
            return true;
            /*
            var createdTask = _taskRepository.Add(newTask);
            //task = createdTask;
            task = MapToTaskItem(createdTask);
            return true;
            */
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

            // 3. Call Repository UpdateStatus passing taskId explicitly
            // 2. Map DTO to Model
            bool isStatusUpdated = _taskRepository.UpdateStatus(taskId,dto.Status );
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

           bool isTaskDeleted = _taskRepository.Delete(taskId);
            return isTaskDeleted;
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
