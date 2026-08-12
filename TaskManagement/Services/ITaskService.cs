using TaskAPI.DTOs;

namespace TaskAPI.Services
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(int currentUserId, string view, string? sortBy, string sortOrder, int? statusId, int pageNumber, int pageSize);
        Task<string> CreateTaskAsync(TaskCreateDto request, int currentUserId);

        // Nayi APIs jo wapas add ki hain
        Task<object> GetDashboardStatsAsync(int currentUserId, string view);
        Task<string> UpdateTaskStatusAsync(int taskId, int newStatusId, int currentUserId, string userRole);
        Task<IEnumerable<TaskResponseDto>> GetAllSystemTasksForAdminAsync(int pageNumber, int pageSize, int? statusId);
    }
}