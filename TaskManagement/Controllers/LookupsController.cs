using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskAPI.Data;

namespace TaskAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LookupsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. Categories Get Karne Ki API
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.TaskCategories.ToListAsync();
            return Ok(categories);
        }

        // 2. Statuses Get Karne Ki API
        [HttpGet("statuses")]
        public async Task<IActionResult> GetStatuses()
        {
            var statuses = await _context.AppTaskStatuses.ToListAsync();
            return Ok(statuses);
        }

        // 3. Priorities Get Karne Ki API
        [HttpGet("priorities")]
        public async Task<IActionResult> GetPriorities()
        {
            var priorities = await _context.TaskPriorities.ToListAsync();
            return Ok(priorities);
        }
    }
}
