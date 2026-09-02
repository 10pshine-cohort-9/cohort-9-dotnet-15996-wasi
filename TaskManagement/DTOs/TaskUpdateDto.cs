namespace TaskAPI.DTOs
{
    public class TaskUpdateDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? DueDate { get; set; }

        public int StatusId { get; set; }
        public int PriorityId { get; set; }
        public int CategoryId { get; set; }
        public int AssignedToUserId { get; set; }
    }
}