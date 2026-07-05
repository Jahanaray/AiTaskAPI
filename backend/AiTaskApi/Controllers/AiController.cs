using AiTaskApi.Data;
using AiTaskApi.Models;
using AiTaskApi.Services;
using AiTaskApi.Shared.Models.Agent;
using AiTaskApi.Shared.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]
public class AiController : ControllerBase
{
    private readonly AiService _ai;
    private readonly AppDbContext _context;
    private readonly AgentService _agent;
    private readonly RabbitMqService _rabbit;

    public AiController(AiService ai, AppDbContext context, AgentService agent, RabbitMqService rabbit)
    {
        _ai = ai;
        _context = context;
        _agent = agent;
        _rabbit = rabbit;
    }

    // -----------------------------
    // Simple chat (no DB)
    // -----------------------------
    [HttpPost("chat")]
    public async Task<IActionResult> Chat([FromBody] ChatRequest request)
    {
        var result = await _ai.AskAsync(request.Message);
        return Ok(new { response = result });
    }

    // -----------------------------
    // AI task creation is queued so the UI never waits for the LLM.
    // -----------------------------
    [HttpPost("create-task")]
    public async Task<IActionResult> CreateTaskFromAi([FromBody] ChatRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required");

        var job = new AgentJob
        {
            Prompt = request.Message,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            MaxAttempts = 4
        };

        _context.AgentJobs.Add(job);
        await _context.SaveChangesAsync();
        await _rabbit.PublishAsync("agent-jobs", job.Id);

        return Accepted(new
        {
            job.Id,
            job.Status,
            job.MaxAttempts
        });
    }

    // -----------------------------
    // Agent execution (SYNC - legacy, still works)
    // -----------------------------
    [HttpPost("agent")]
    public async Task<IActionResult> Agent([FromBody] ChatRequest request)
    {
        var plan = await _ai.AskPlanAsync(request.Message);

        if (plan?.Steps == null || plan.Steps.Count == 0)
            return BadRequest("No plan generated");

        var results = new List<object>();

        foreach (var step in plan.Steps)
        {
            switch (step.Action)
            {
                case "CreateTask":
                    {
                        var task = new TaskItem
                        {
                            Title = step.Data?.Title ?? "Untitled",
                            Description = step.Data?.Description,
                            Priority = step.Data?.Priority ?? "Medium",
                            Status = "Todo"
                        };

                        if (DateTime.TryParse(step.Data?.DueDate, out var dt))
                            task.DueDate = DateTime.SpecifyKind(dt, DateTimeKind.Utc);

                        _context.Tasks.Add(task);
                        await _context.SaveChangesAsync();

                        results.Add(task);
                        break;
                    }

                case "GetTasks":
                    {
                        var tasks = await _context.Tasks.ToListAsync();
                        results.Add(tasks);
                        break;
                    }

                case "DeleteTask":
                    {
                        var task = await _context.Tasks.FindAsync(step.TaskId);
                        if (task != null)
                        {
                            _context.Tasks.Remove(task);
                            await _context.SaveChangesAsync();
                            results.Add("Deleted");
                        }
                        else
                        {
                            results.Add("Not found");
                        }
                        break;
                    }

                default:
                    results.Add($"Unknown action: {step.Action}");
                    break;
            }
        }

        return Ok(new
        {
            steps = plan.Steps.Count,
            results
        });
    }
    // -----------------------------
    // NEW: Worker-based async agent system
    // -----------------------------
    [HttpPost("agent-loop")]
    public async Task<IActionResult> RunAgent([FromBody] ChatRequest request)
    {
        var job = new AgentJob
        {
            Prompt = request.Message,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            MaxAttempts = 4
        };

        _context.AgentJobs.Add(job);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            job.Id,
            job.Status
        });
    }

    // -----------------------------
    // Optional alias (remove later if not needed)
    // -----------------------------
    [HttpPost("agent/run")]
    public async Task<IActionResult> RunAgentLegacy([FromBody] ChatRequest request)
    {
        var job = new AgentJob
        {
            Prompt = request.Message,
            Status = "Pending",
            CreatedAt = DateTime.UtcNow,
            MaxAttempts = 4
        };

        //_context.AgentJobs.Add(job);
        //await _context.SaveChangesAsync();
        await _rabbit.PublishAsync("agent-jobs", job);
        return Ok(new { jobId = job.Id });
    }



    // =====================================
    // GET ALL JOBS
    // =====================================
    [HttpGet("jobs")]
    public async Task<IActionResult> GetJobs()
    {
        var jobs = await _context.AgentJobs
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        return Ok(jobs);
    }

    // =====================================
    // GET SINGLE JOB
    // =====================================
    [HttpGet("jobs/{id}")]
    public async Task<IActionResult> GetJob(int id)
    {
        var job = await _context.AgentJobs.FindAsync(id);

        if (job == null)
            return NotFound();

        return Ok(job);
    }

    // =====================================
    // DELETE JOB
    // =====================================
    [HttpDelete("jobs/{id}")]
    public async Task<IActionResult> DeleteJob(int id)
    {
        var job = await _context.AgentJobs.FindAsync(id);

        if (job == null)
            return NotFound();

        _context.AgentJobs.Remove(job);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Deleted"
        });
    }

    // =====================================
    // DELETE ALL JOBS
    // =====================================
    [HttpDelete("jobs")]
    public async Task<IActionResult> DeleteAllJobs()
    {
        var jobs = await _context.AgentJobs.ToListAsync();

        _context.AgentJobs.RemoveRange(jobs);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "All jobs deleted"
        });
    }

    // =====================================
    // RETRY JOB
    // =====================================
    [HttpPost("jobs/{id}/retry")]
    public async Task<IActionResult> RetryJob(int id)
    {
        var job = await _context.AgentJobs.FindAsync(id);

        if (job == null)
            return NotFound();

        job.Status = "Pending";
        job.Result = null;
        job.CompletedAt = null;
        job.AttemptCount = 0;
        job.MaxAttempts = Math.Max(job.MaxAttempts, 4);
        job.NextAttemptAt = null;
        job.LastError = null;

        await _context.SaveChangesAsync();

        return Ok(job);
    }
}

// -----------------------------
// DTO
// -----------------------------
public class ChatRequest
{
    public string Message { get; set; } = "";
}
