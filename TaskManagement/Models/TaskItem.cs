namespace TaskAPI.Models
{
    public class TaskItem
    {
        public int TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // Lookup Foreign Keys
        public int StatusId { get; set; }
        public AppTaskStatus? Status { get; set; }

        public int PriorityId { get; set; }
        public TaskPriority? Priority { get; set; }

        public int CategoryId { get; set; }
        public TaskCategory? Category { get; set; }

        public DateTime? DueDate { get; set; }

        // User Foreign Keys (Assigned To & Created By)
        public int AssignedToUserId { get; set; }
        public User? AssignedToUser { get; set; }

        public int CreatedByUserId { get; set; }
        public User? CreatedByUser { get; set; }

        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}