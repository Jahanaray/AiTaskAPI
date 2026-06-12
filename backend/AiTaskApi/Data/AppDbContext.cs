using AiTaskApi.Models; // adjust if your project name differs
using AiTaskApi.Shared.Models.Agent;
using Microsoft.EntityFrameworkCore;

namespace AiTaskApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<TaskItem> Tasks => Set<TaskItem>();
        public DbSet<User> Users => Set<User>();
        public DbSet<AgentJob> AgentJobs { get; set; }
    }
}