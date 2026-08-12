using System.ComponentModel.DataAnnotations;


namespace TaskAPI.DTOs
{
    public class TaskCreateDto
    {
        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateTime? DueDate { get; set; }

        [Required(ErrorMessage = "StatusId dena zaroori hai.")]
        public int StatusId { get; set; }

        [Required(ErrorMessage = "PriorityId dena zaroori hai.")]
        public int PriorityId { get; set; }

        [Required(ErrorMessage = "CategoryId dena zaroori hai.")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "AssignedToUserId dena zaroori hai.")]
        public int AssignedToUserId { get; set; }
    }
}