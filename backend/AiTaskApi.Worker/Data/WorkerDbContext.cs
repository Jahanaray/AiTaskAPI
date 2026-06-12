using AiTaskApi.Shared.Models;
using AiTaskApi.Shared.Models.Agent;
using Microsoft.EntityFrameworkCore;

namespace AiTaskApi.Worker.Data;

public class WorkerDbContext : DbContext
{
    public WorkerDbContext(DbContextOptions<WorkerDbContext> options)
        : base(options) { }

    public DbSet<AgentJob> AgentJobs => Set<AgentJob>();
}