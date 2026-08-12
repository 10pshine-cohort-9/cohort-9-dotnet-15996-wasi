using Microsoft.EntityFrameworkCore;
using TaskAPI.Models; // <-- Yeh 'using' statement chahiye thi

namespace TaskAPI.Data // <-- Yahan sirf Data ka namespace aayega
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        // Sab tables ke DbSets (Jo SQL mein tables banenge)
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<AppTaskStatus> AppTaskStatuses { get; set; }
        public DbSet<TaskPriority> TaskPriorities { get; set; }
        public DbSet<TaskCategory> TaskCategories { get; set; }
        public DbSet<TaskItem> Tasks { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // 0. PRIMARY KEYS CONFIGURATION
            // ==========================================
            modelBuilder.Entity<AppTaskStatus>().HasKey(s => s.StatusId);
            modelBuilder.Entity<TaskPriority>().HasKey(p => p.PriorityId);
            modelBuilder.Entity<TaskCategory>().HasKey(c => c.CategoryId);
            modelBuilder.Entity<TaskItem>().HasKey(t => t.TaskId);
            modelBuilder.Entity<Role>().HasKey(r => r.RoleId);
            modelBuilder.Entity<User>().HasKey(u => u.UserId);



            // ==========================================
            // 1. FLUENT API (Foreign Keys & Relations)
            // ==========================================

            // Multiple Cascade Paths ke error se bachne ke liye relations ko restrict karna
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.AssignedToUser)
                .WithMany()
                .HasForeignKey(t => t.AssignedToUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.CreatedByUser)
                .WithMany()
                .HasForeignKey(t => t.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);


            // ==========================================
            // 2. DATA SEEDING (Default Data dalna)
            // ==========================================

            // Roles
            modelBuilder.Entity<Role>().HasData(
                new Role { RoleId = 1, RoleName = "Admin" },
                new Role { RoleId = 2, RoleName = "User" }
            );

            // Task Statuses
            modelBuilder.Entity<AppTaskStatus>().HasData(
                new AppTaskStatus { StatusId = 1, StatusName = "Pending" },
                new AppTaskStatus { StatusId = 2, StatusName = "InProgress" },
                new AppTaskStatus { StatusId = 3, StatusName = "Completed" }
            );

            // Task Priorities
            modelBuilder.Entity<TaskPriority>().HasData(
                new TaskPriority { PriorityId = 1, PriorityName = "Low" },
                new TaskPriority { PriorityId = 2, PriorityName = "Medium" },
                new TaskPriority { PriorityId = 3, PriorityName = "High" }
            );

            // Task Categories (Aapki batayi hui categories)
            modelBuilder.Entity<TaskCategory>().HasData(
                new TaskCategory { CategoryId = 1, CategoryName = "Work" },
                new TaskCategory { CategoryId = 2, CategoryName = "Personal" },
                new TaskCategory { CategoryId = 3, CategoryName = "Urgent Fixes" },
                new TaskCategory { CategoryId = 4, CategoryName = "Meetings" },
                new TaskCategory { CategoryId = 5, CategoryName = "Documentation" }
            );
        }
    }
}