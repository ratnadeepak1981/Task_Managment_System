using TaskManagementSystem.DTO;

namespace TaskManagementSystem.Services.Interface
{
    public interface ITaskService
    {
        // 1. Lightweight list for overview pages
        public IEnumerable<TaskItemResponseDto> GetAllTasks();
        public TaskItemResponseDto? GetTaskById(int taskId);

        public TaskItemResponseDto? GetTaskByName(string taskName);
        // 2. Creation usually returns the basic profile
        bool CreateTask(CreateTaskItemDto dto, out TaskItemResponseDto? user, out string errorMessage);

        bool UpdateTask(int taskId,UpdateTaskItemDto dto, out TaskItemResponseDto? user, out string errorMessage);

        public bool UpdateTaskStatus(int taskId, ChangeStatusDto dto, out TaskItemResponseDto? task, out string errorMessage);

        public bool DeleteTask(int taskId, out TaskItemResponseDto? task, out string errorMessage);

        
    }
}
