using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;
using TaskAPI.DTOs;
using TaskAPI.Models;
using Microsoft.Extensions.Logging;

namespace TaskAPI.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TaskService> _logger; // <-- Serilog yahan aagaya

        public TaskService(ApplicationDbContext context, ILogger<TaskService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync(int currentUserId, string view, string? sortBy, string sortOrder, int? statusId, int pageNumber, int pageSize)
        {
            var query = _context.Tasks
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Category)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .AsQueryable();

            if (view.ToLower() == "assigned")
                query = query.Where(t => t.AssignedToUserId == currentUserId && t.CreatedByUserId != currentUserId);
            else
                query = query.Where(t => t.CreatedByUserId == currentUserId);

            if (statusId.HasValue)
                query = query.Where(t => t.StatusId == statusId.Value);

            bool isDesc = sortOrder.ToLower() == "desc";
            if (!string.IsNullOrEmpty(sortBy))
            {
                if (sortBy.ToLower() == "duedate")
                    query = isDesc ? query.OrderByDescending(t => t.DueDate) : query.OrderBy(t => t.DueDate);
                else if (sortBy.ToLower() == "priority")
                    query = isDesc ? query.OrderByDescending(t => t.PriorityId) : query.OrderBy(t => t.PriorityId);
            }
            else
            {
                query = query.OrderByDescending(t => t.CreatedAt);
            }

            int skipCount = (pageNumber - 1) * pageSize;

            return await query
                .Skip(skipCount)
                .Take(pageSize)
                .Select(t => new TaskResponseDto
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    StatusName = t.Status!.StatusName,
                    PriorityName = t.Priority!.PriorityName,
                    CategoryName = t.Category!.CategoryName,
                    AssignedToUser = t.AssignedToUser!.FullName,
                    CreatedByUser = t.CreatedByUser!.FullName
                })
                .ToListAsync();
        }

        public async Task<string> CreateTaskAsync(TaskCreateDto request, int currentUserId)
        {
            var newTask = new TaskItem
            {
                Title = request.Title,
                Description = request.Description,
                DueDate = request.DueDate,
                StatusId = request.StatusId,
                PriorityId = request.PriorityId,
                CategoryId = request.CategoryId,
                AssignedToUserId = request.AssignedToUserId,
                CreatedByUserId = currentUserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Tasks.Add(newTask);
            await _context.SaveChangesAsync();

            // USER ACTIVITY LOGGING VIA SERILOG
            _logger.LogInformation("User ID {UserId} ne ek naya task '{TaskTitle}' banaya.", currentUserId, request.Title);

            return "Task successfully create ho gaya!";
        }

        // 3. DASHBOARD STATS LOGIC
        public async Task<object> GetDashboardStatsAsync(int currentUserId, string view)
        {
            var query = _context.Tasks.AsQueryable();

            if (view.ToLower() == "assigned")
                query = query.Where(t => t.AssignedToUserId == currentUserId && t.CreatedByUserId != currentUserId);
            else
                query = query.Where(t => t.CreatedByUserId == currentUserId);

            var totalTasks = await query.CountAsync();
            var pendingTasks = await query.CountAsync(t => t.StatusId == 1);
            var inProgressTasks = await query.CountAsync(t => t.StatusId == 2);
            var completedTasks = await query.CountAsync(t => t.StatusId == 3);

            return new { Total = totalTasks, Pending = pendingTasks, InProgress = inProgressTasks, Completed = completedTasks };
        }

        // 4. QUICK STATUS UPDATE LOGIC
        public async Task<string> UpdateTaskStatusAsync(int taskId, int newStatusId, int currentUserId, string userRole)
        {
            var task = await _context.Tasks.FindAsync(taskId);
            if (task == null) throw new Exception("Task nahi mila."); // Global handler isey pakar lega

            if (userRole != "Admin" && task.CreatedByUserId != currentUserId && task.AssignedToUserId != currentUserId)
            {
                throw new UnauthorizedAccessException("Access Denied: Aap sirf apne tasks change kar sakte hain.");
            }

            task.StatusId = newStatusId;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Task ID {TaskId} ka status update ho kar {NewStatusId} ho gaya.", taskId, newStatusId);
            return "Task status successfully update ho gaya!";
        }

        // 5. ADMIN ALL TASKS LOGIC
        public async Task<IEnumerable<TaskResponseDto>> GetAllSystemTasksForAdminAsync(int pageNumber, int pageSize, int? statusId)
        {
            var query = _context.Tasks
                .Include(t => t.Status)
                .Include(t => t.Priority)
                .Include(t => t.Category)
                .Include(t => t.AssignedToUser)
                .Include(t => t.CreatedByUser)
                .AsQueryable();

            if (statusId.HasValue) query = query.Where(t => t.StatusId == statusId.Value);

            int skipCount = (pageNumber - 1) * pageSize;

            return await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip(skipCount).Take(pageSize)
                .Select(t => new TaskResponseDto
                {
                    TaskId = t.TaskId,
                    Title = t.Title,
                    Description = t.Description,
                    DueDate = t.DueDate,
                    CreatedAt = t.CreatedAt,
                    StatusName = t.Status!.StatusName,
                    PriorityName = t.Priority!.PriorityName,
                    CategoryName = t.Category!.CategoryName,
                    AssignedToUser = t.AssignedToUser!.FullName,
                    CreatedByUser = t.CreatedByUser!.FullName
                }).ToListAsync();
        }
    }
}