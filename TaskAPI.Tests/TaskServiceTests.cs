using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using TaskAPI.Data;
using TaskAPI.DTOs;
using TaskAPI.Models;
using TaskAPI.Services;
using Xunit;

namespace TaskAPI.Tests
{
    public class TaskServiceTests
    {
        // Fake (In-Memory) Database banane ka helper method
        private ApplicationDbContext GetFakeDatabase()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        [Fact]
        public async Task GetAllTasksAsync_ShouldReturn_Tasks_CreatedBySpecificUser()
        {
            // -----------------------------
            // 1. ARRANGE (Taiyari)
            // -----------------------------
            var dbContext = GetFakeDatabase();

            // YAHAN NAAM CHANGE KIYE HAIN: AppTaskStatus, TaskPriority, TaskCategory
            var dummyStatus = new AppTaskStatus { StatusId = 1, StatusName = "Pending" };
            var dummyPriority = new TaskPriority { PriorityId = 1, PriorityName = "High" };
            var dummyCategory = new TaskCategory { CategoryId = 1, CategoryName = "Work" };
            var user1 = new User { UserId = 1, FullName = "Wasi Hassan" };
            var user2 = new User { UserId = 2, FullName = "Ali" };

            // Database mein 2 Dummy Tasks daal rahe hain
            dbContext.Tasks.Add(new TaskItem
            {
                TaskId = 1,
                Title = "User 1 Ka Task",
                CreatedByUserId = 1,
                AssignedToUserId = 1,
                Status = dummyStatus,
                Priority = dummyPriority,
                Category = dummyCategory,
                CreatedByUser = user1,
                AssignedToUser = user1
            });

            dbContext.Tasks.Add(new TaskItem
            {
                TaskId = 2,
                Title = "User 2 Ka Task",
                CreatedByUserId = 2,
                AssignedToUserId = 2,
                Status = dummyStatus,
                Priority = dummyPriority,
                Category = dummyCategory,
                CreatedByUser = user2,
                AssignedToUser = user2
            });

            await dbContext.SaveChangesAsync();

            var mockLogger = new Mock<ILogger<TaskService>>();
            var taskService = new TaskService(dbContext, mockLogger.Object);

            // -----------------------------
            // 2. ACT
            // -----------------------------
            var result = await taskService.GetAllTasksAsync(
                currentUserId: 1,
                view: "created",
                sortBy: null,
                sortOrder: "asc",
                statusId: null,
                pageNumber: 1,
                pageSize: 10);

            // -----------------------------
            // 3. ASSERT
            // -----------------------------
            var taskList = result.ToList();

            Assert.Single(taskList);
            Assert.Equal("User 1 Ka Task", taskList[0].Title);
        }

        [Fact]
        public async Task CreateTaskAsync_ShouldAddTask_ToDatabase()
        {
            // -----------------------------
            // 1. ARRANGE
            // -----------------------------
            var dbContext = GetFakeDatabase();
            var mockLogger = new Mock<ILogger<TaskService>>();
            var taskService = new TaskService(dbContext, mockLogger.Object);

            var newTaskDto = new TaskCreateDto
            {
                Title = "Test C# Logic",
                Description = "Unit testing with In-Memory DB",
                StatusId = 1,
                PriorityId = 1,
                CategoryId = 1,
                AssignedToUserId = 1
            };

            // -----------------------------
            // 2. ACT
            // -----------------------------
            var message = await taskService.CreateTaskAsync(newTaskDto, currentUserId: 1);

            // -----------------------------
            // 3. ASSERT
            // -----------------------------
            var tasksInDb = await dbContext.Tasks.ToListAsync();

            Assert.Single(tasksInDb);
            Assert.Equal("Test C# Logic", tasksInDb[0].Title);
            Assert.Equal(1, tasksInDb[0].CreatedByUserId);
            Assert.Equal("Task successfully create ho gaya!", message);
        }

        [Fact]
        public async Task GetDashboardStatsAsync_ShouldReturn_CorrectCounts()
        {
            // 1. ARRANGE
            var dbContext = GetFakeDatabase();

            // 2 tasks Pending (StatusId = 1), 1 task Completed (StatusId = 3)
            dbContext.Tasks.Add(new TaskItem { TaskId = 1, Title = "T1", CreatedByUserId = 1, StatusId = 1 });
            dbContext.Tasks.Add(new TaskItem { TaskId = 2, Title = "T2", CreatedByUserId = 1, StatusId = 1 });
            dbContext.Tasks.Add(new TaskItem { TaskId = 3, Title = "T3", CreatedByUserId = 1, StatusId = 3 });
            await dbContext.SaveChangesAsync();

            var mockLogger = new Mock<ILogger<TaskService>>();
            var taskService = new TaskService(dbContext, mockLogger.Object);

            // 2. ACT
            // User 1 ki stats mangwa rahe hain
            var result = await taskService.GetDashboardStatsAsync(currentUserId: 1, view: "created");

            // 3. ASSERT
            // Dynamic object ko string mein convert kar ke verify kar rahe hain
            var statsString = result.ToString();
            Assert.Contains("Total = 3", statsString);
            Assert.Contains("Pending = 2", statsString);
            Assert.Contains("Completed = 1", statsString);
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_ShouldUpdate_Status_WhenUserIsAuthorized()
        {
            // 1. ARRANGE
            var dbContext = GetFakeDatabase();

            // Task shuru mein Pending (StatusId = 1) hai
            var task = new TaskItem { TaskId = 1, Title = "Update Test", CreatedByUserId = 1, StatusId = 1 };
            dbContext.Tasks.Add(task);
            await dbContext.SaveChangesAsync();

            var mockLogger = new Mock<ILogger<TaskService>>();
            var taskService = new TaskService(dbContext, mockLogger.Object);

            // 2. ACT
            // User 1 ne apni task ka status 1 se change karke 3 (Completed) kar diya
            var message = await taskService.UpdateTaskStatusAsync(taskId: 1, newStatusId: 3, currentUserId: 1, userRole: "User");

            // 3. ASSERT
            var taskInDb = await dbContext.Tasks.FindAsync(1);
            Assert.Equal(3, taskInDb!.StatusId); // Status 3 hona chahiye
            Assert.Equal("Task status successfully update ho gaya!", message);
        }

        [Fact]
        public async Task GetAllSystemTasksForAdminAsync_ShouldReturn_AllTasks_FromMultipleUsers()
        {
            // 1. ARRANGE
            var dbContext = GetFakeDatabase();

            var dummyStatus = new AppTaskStatus { StatusId = 1, StatusName = "Pending" };
            var dummyPriority = new TaskPriority { PriorityId = 1, PriorityName = "High" };
            var dummyCategory = new TaskCategory { CategoryId = 1, CategoryName = "Work" };

            // 2 mukhtalif users ke 2 tasks
            dbContext.Tasks.Add(new TaskItem
            {
                TaskId = 1,
                Title = "User 1 Task",
                CreatedByUserId = 1,
                AssignedToUserId = 1,
                Status = dummyStatus,
                Priority = dummyPriority,
                Category = dummyCategory,
                CreatedByUser = new User { UserId = 1, FullName = "U1" },
                AssignedToUser = new User { UserId = 1, FullName = "U1" }
            });

            dbContext.Tasks.Add(new TaskItem
            {
                TaskId = 2,
                Title = "User 2 Task",
                CreatedByUserId = 2,
                AssignedToUserId = 2,
                Status = dummyStatus,
                Priority = dummyPriority,
                Category = dummyCategory,
                CreatedByUser = new User { UserId = 2, FullName = "U2" },
                AssignedToUser = new User { UserId = 2, FullName = "U2" }
            });

            await dbContext.SaveChangesAsync();

            var mockLogger = new Mock<ILogger<TaskService>>();
            var taskService = new TaskService(dbContext, mockLogger.Object);

            // 2. ACT
            // Admin paginated API call kar raha hai
            var result = await taskService.GetAllSystemTasksForAdminAsync(pageNumber: 1, pageSize: 10, statusId: null);

            // 3. ASSERT
            var taskList = result.ToList();

            Assert.Equal(2, taskList.Count); // Admin ko dono tasks milne chahiye
            // Check kar rahe hain ke sorting theek hui (Latest pehle aaya)
            Assert.Equal("User 2 Task", taskList[0].Title);
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_ShouldThrowException_WhenTaskNotFound()
        {
            // 1. ARRANGE
            var dbContext = GetFakeDatabase(); // Database bilkul khali hai

            var mockLogger = new Mock<ILogger<TaskService>>();
            var taskService = new TaskService(dbContext, mockLogger.Object);

            // 2 & 3. ACT & ASSERT
            // Hum check kar rahe hain ke agar Task ID 99 (jo exist nahi karta) bhejain, tou kya Exception aayegi?
            var exception = await Assert.ThrowsAsync<Exception>(() =>
                taskService.UpdateTaskStatusAsync(taskId: 99, newStatusId: 3, currentUserId: 1, userRole: "User"));

            // Check: Kya Exception ka message wahi hai jo humne code mein likha tha?
            Assert.Equal("Task nahi mila.", exception.Message);
        }

        [Fact]
        public async Task UpdateTaskStatusAsync_ShouldThrowUnauthorizedException_WhenUserIsNotAuthorized()
        {
            // 1. ARRANGE
            var dbContext = GetFakeDatabase();

            // Database mein Task User ID 1 ne banaya hai
            dbContext.Tasks.Add(new TaskItem { TaskId = 1, Title = "Secret Task", CreatedByUserId = 1, AssignedToUserId = 1 });
            await dbContext.SaveChangesAsync();

            var mockLogger = new Mock<ILogger<TaskService>>();
            var taskService = new TaskService(dbContext, mockLogger.Object);

            // 2 & 3. ACT & ASSERT
            // ACT: User ID 2 (jo na admin hai na task owner) isko change karne ki koshish kar raha hai
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
                taskService.UpdateTaskStatusAsync(taskId: 1, newStatusId: 3, currentUserId: 2, userRole: "User"));

            // ASSERT: System ko lazmi Security Exception throw karni chahiye
            Assert.Equal("Access Denied: Aap sirf apne tasks change kar sakte hain.", exception.Message);
        }

    }
}