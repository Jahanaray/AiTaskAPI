using AiTaskApi.Data;
using AiTaskApi.Services;
using AiTaskApi.Shared.Helper;
using AiTaskApi.Shared.Models.Agent;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class Worker : BackgroundService
{
    private const int RetryDelaySeconds = 10;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;

    public Worker(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var agent = scope.ServiceProvider.GetRequiredService<AgentService>();
            var ai = scope.ServiceProvider.GetRequiredService<AiService>();

            var job = await db.AgentJobs
                .Where(j =>
                    j.Status == "Pending" &&
                    (j.NextAttemptAt == null || j.NextAttemptAt <= DateTime.UtcNow))
                .OrderBy(j => j.CreatedAt)
                .FirstOrDefaultAsync(stoppingToken);

            if (job != null)
            {
                await RunJob(job, db, agent, stoppingToken);
                continue;
            }

            // =========================
            // DREAM MODE (idle brain)
            // =========================
            var isIdle = await IsOllamaIdle(ai);

            if (isIdle)
            {
                await RunDreamCycle(db, ai, stoppingToken);
            }

            await Task.Delay(3000, stoppingToken);
        }
    }

    // =========================
    // JOB EXECUTION
    // =========================
    private async Task RunJob(
        AgentJob job,
        AppDbContext db,
        AgentService agent,
        CancellationToken token)
    {
        job.Status = "Running";
        job.AttemptCount += 1;
        job.LastError = null;
        job.NextAttemptAt = null;
        await db.SaveChangesAsync(token);

        try
        {
            var result = await agent.RunFullPipeline(job.Prompt);

            job.Status = "Done";
            job.Result = JsonSerializer.Serialize(result);
            job.CompletedAt = DateTime.UtcNow;
            job.NextAttemptAt = null;
        }
        catch (Exception ex)
        {
            job.LastError = ex.Message;

            if (job.AttemptCount < job.MaxAttempts)
            {
                job.Status = "Pending";
                job.Result = $"Attempt {job.AttemptCount}/{job.MaxAttempts} failed. Retry scheduled.";
                job.NextAttemptAt = DateTime.UtcNow.AddSeconds(RetryDelaySeconds * job.AttemptCount);
            }
            else
            {
                job.Status = "Failed";
                job.Result = ex.Message;
                job.CompletedAt = DateTime.UtcNow;
                job.NextAttemptAt = null;
            }
        }

        await db.SaveChangesAsync(token);
    }

    // =========================
    // DREAM MODE
    // =========================
    private async Task RunDreamCycle(
        AppDbContext db,
        AiService ai,
        CancellationToken token)
    {
        var tasks = await db.Tasks.ToListAsync(token);

        var prompt = $@"
You are a productivity optimization system.

Improve this task list:
- remove duplicates
- improve clarity
- reorder priorities
- suggest missing tasks

Tasks:
{JsonSerializer.Serialize(tasks)}
";

        var result = await ai.AskAsync(prompt);

        Console.WriteLine("💭 DREAM MODE OUTPUT:");
        Console.WriteLine(result);
    }

    // =========================
    // OLLAMA IDLE CHECK
    // =========================
    private async Task<bool> IsOllamaIdle(AiService ai)
    {
        try
        {
            var http = _httpClientFactory.CreateClient();
            var url = ServerAddress.LlmServer + "/api/ps";
            var res = await http.GetAsync(url);
            var json = await res.Content.ReadAsStringAsync();

            return json.Contains("\"models\":[]");
        }
        catch
        {
            return false;
        }
    }
}
