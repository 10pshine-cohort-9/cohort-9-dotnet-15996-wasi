using System.ComponentModel.DataAnnotations;

namespace TaskAPI.DTOs
{
    public class TaskStatusUpdateDto
    {
        [Required(ErrorMessage = "StatusId dena zaroori hai.")]
        public int StatusId { get; set; }
    }
}