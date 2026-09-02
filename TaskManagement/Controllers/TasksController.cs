using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskAPI.DTOs;
using TaskAPI.Services;

namespace TaskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        // 1. GET ALL TASKS
        [HttpGet]
        public async Task<IActionResult> GetAllTasks(
            [FromQuery] string view = "created",
            [FromQuery] string? sortBy = null,
            [FromQuery] string sortOrder = "asc",
            [FromQuery] int? statusId = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized("User token invalid hai.");

            int currentUserId = int.Parse(userIdClaim);

            // Seedha Service ko call kar diya!
            var tasks = await _taskService.GetAllTasksAsync(currentUserId, view, sortBy, sortOrder, statusId, pageNumber, pageSize);
            return Ok(tasks);
        }

        // 2. CREATE NEW TASK
        [HttpPost]
        public async Task<IActionResult> CreateTask(TaskCreateDto request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Unauthorized("User identity verify nahi ho saki.");

            int currentUserId = int.Parse(userIdClaim);

            // Database aur Logging ka saara kaam Service ke andar ho raha hai
            var message = await _taskService.CreateTaskAsync(request, currentUserId);
            return Ok(message);
        }

        // 3. DASHBOARD STATISTICS API
        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats([FromQuery] string view = "created")
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var stats = await _taskService.GetDashboardStatsAsync(currentUserId, view);
            return Ok(stats);
        }

        // 4. QUICK STATUS UPDATE (PATCH)
        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateTaskStatus(int id, [FromBody] TaskStatusUpdateDto request)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            var userRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "User";

            var message = await _taskService.UpdateTaskStatusAsync(id, request.StatusId, currentUserId, userRole);
            return Ok(new { Message = message });
        }

        // 5. ADMIN ONLY - VIEW ALL SYSTEM TASKS
        [HttpGet("admin-all")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllSystemTasksForAdmin(
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] int? statusId = null)
        {
            var tasks = await _taskService.GetAllSystemTasksForAdminAsync(pageNumber, pageSize, statusId);
            return Ok(tasks);
        }
    }
}