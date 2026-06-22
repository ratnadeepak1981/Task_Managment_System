namespace TaskManagementSystem.DTO.Response
{
    public class TaskItemResponseDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }


        public int UserId { get; set; }              // Added for T-1
        public string UserName { get; set; } = string.Empty; // Added for T-1
    }
}
