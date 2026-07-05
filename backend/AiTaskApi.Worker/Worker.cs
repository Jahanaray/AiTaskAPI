using AiTaskApi.Data;
using AiTaskApi.Services;
using AiTaskApi.Shared.Helper;
using AiTaskApi.Shared.Models.Agent;
using AiTaskApi.Worker.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

public class Worker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly RabbitMqConsumer _consumer;

    public Worker(
        IServiceScopeFactory scopeFactory,
        IHttpClientFactory httpClientFactory,
        RabbitMqConsumer consumer)
    {
        _scopeFactory = scopeFactory;
        _httpClientFactory = httpClientFactory;
        _consumer = consumer;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {

        await _consumer.StartAsync(stoppingToken);




        //while (!stoppingToken.IsCancellationRequested)
        //{
        //    using var scope = _scopeFactory.CreateScope();

        //    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        //    var agent = scope.ServiceProvider.GetRequiredService<AgentService>();
        //    var ai = scope.ServiceProvider.GetRequiredService<AiService>();

        //    var job = await db.AgentJobs
        //        .Where(j =>
        //            j.Status == "Pending" &&
        //            (j.NextAttemptAt == null || j.NextAttemptAt <= DateTime.UtcNow))
        //        .OrderBy(j => j.CreatedAt)
        //        .FirstOrDefaultAsync(stoppingToken);

        //    if (job != null)
        //    {
        //        await RunJob(job, db, agent, stoppingToken);
        //        continue;
        //    }

        //    // =========================
        //    // DREAM MODE (idle brain)
        //    // =========================
        //    var isIdle = await IsOllamaIdle(ai);

        //    if (isIdle)
        //    {
        //        await RunDreamCycle(db, ai, stoppingToken);
        //    }

        //    await Task.Delay(3000, stoppingToken);
        //}
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
