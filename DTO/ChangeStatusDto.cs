namespace TaskManagementSystem.DTO
{
    public class ChangeStatusDto
    {
        public int TaskId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
