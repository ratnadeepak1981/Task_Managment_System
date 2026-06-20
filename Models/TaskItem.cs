namespace TaskManagementSystem.Models
{
    public class TaskItem
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }

        // FIXED: Added Foreign Key property to link this Task to a User
        public int UserId { get; set; }
        public string UserName { get; set; }= string.Empty;
    }
}
