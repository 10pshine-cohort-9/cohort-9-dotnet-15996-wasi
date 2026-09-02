namespace TaskAPI.DTOs
{
    public class TaskResponseDto
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }

        public string StatusName { get; set; } = string.Empty;
        public string PriorityName { get; set; } = string.Empty;
        public string CategoryName { get; set; } = string.Empty;

        public string AssignedToUser { get; set; } = string.Empty;
        public string CreatedByUser { get; set; } = string.Empty;
    }
}