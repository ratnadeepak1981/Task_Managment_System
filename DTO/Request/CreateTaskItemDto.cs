namespace TaskManagementSystem.DTO.Request
{
    public class CreateTaskItemDto
    {
        public string Title { get; set; } = string.Empty;
        public string ? Description { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        // Added to fulfill the relational table constraint
        public int UserId { get; set; }

    }
}
