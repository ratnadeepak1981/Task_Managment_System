using TaskManagementSystem.Models;
using TaskManagementSystem.DTO;

namespace TaskManagementSystem.Repositories.Interface
{
    public interface ITaskRepository
    {
        IEnumerable<TaskItem> GetAll();
        TaskItem? GetTaskById(int taskId);
        public TaskItem Add(TaskItem task);
        public bool Update(int taskid,TaskItem task);
        public bool UpdateStatus(int taskId, string status);
        public bool Delete(int taskId);
        public List<TaskItem>? GetTaskByName(string taskName);
    }
}
