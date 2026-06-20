namespace TaskManagementSystem.Models
{
    // Domain object matching relational architecture requirements
    public class UserWithTasks
    {
        public User UserProfile { get; set; } = new();
        public List<TaskItem> Tasks { get; set; } = new();
    }
}
