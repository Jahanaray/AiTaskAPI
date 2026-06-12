using AiTaskApi.Data;
using AiTaskApi.Models;
using AiTaskApi.Shared.Models.Agent;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AiTaskApi.Services;

public class AgentService
{
    private readonly AiService _ai;
    private readonly AppDbContext _context;

    public AgentService(AiService ai, AppDbContext context)
    {
        _ai = ai;
        _context = context;
    }

    // =========================
    // MAIN PIPELINE
    // =========================
    public async Task<object?> RunAsync(string input)
    {
        var memory = new AgentMemory
        {
            Steps = new List<string>()
        };

        var plan = await _ai.AskPlanAsync(BuildPlannerPrompt(input));

        if (plan?.Steps == null || plan.Steps.Count == 0)
            throw new InvalidOperationException("The AI did not return a valid execution plan.");

        foreach (var step in plan.Steps)
        {
            if (string.IsNullOrWhiteSpace(step.Action))
                continue;

            var result = await Execute(step);

            memory.Steps.Add(step.Action);
            memory.LastResult = JsonSerializer.Serialize(result);
        }

        return memory;
    }

    // =========================
    // PLANNER PROMPT
    // =========================
    private string BuildPlannerPrompt(string input)
    {
        return $@"
You are an autonomous senior planning system for a production task manager.
Think carefully and spend extra effort before answering. Silently analyze the
user's intent, dependencies, urgency, missing details, and sensible task
boundaries. Do not expose your reasoning.

Return ONLY valid JSON.

Rules:
- Break broad goals into 3-8 useful steps
- If the user asks for one concrete task, create exactly one strong task
- MAX 10 steps
- Prefer clear, actionable titles
- Write descriptions that include enough context for a developer to execute
- Pick priority based on business urgency, not wording alone
- Use null dueDate when the user gives no date
- Never invent DeleteTask unless the user explicitly asks to delete

Actions:
- CreateTask
- GetTasks
- DeleteTask

FORMAT:
{{
  ""steps"": [
    {{
      ""action"": ""CreateTask"",
      ""taskId"": null,
      ""data"": {{
        ""title"": ""string"",
        ""description"": ""string"",
        ""priority"": ""Low | Medium | High"",
        ""dueDate"": ""yyyy-MM-dd or null""
      }}
    }}
  ]
}}

User:
{input}
";
    }

    // =========================
    // EXECUTOR
    // =========================
    private async Task<object?> Execute(AiActionResponse step)
    {
        return step.Action switch
        {
            "CreateTask" => await CreateTask(step),
            "GetTasks" => await _context.Tasks.ToListAsync(),
            "DeleteTask" => await DeleteTask(step),
            _ => $"Unknown: {step.Action}"
        };
    }

    // =========================
    // CREATE TASK
    // =========================
    private async Task<object> CreateTask(AiActionResponse step)
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

        return task;
    }

    // =========================
    // DELETE TASK
    // =========================
    private async Task<object> DeleteTask(AiActionResponse step)
    {
        if (step.TaskId == null)
            return "Missing TaskId";

        var task = await _context.Tasks.FindAsync(step.TaskId);
        if (task == null) return "Not found";

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();

        return "Deleted";
    }

    // =========================
    // FULL PIPELINE (planner + critic ready)
    // =========================
    public async Task<object> RunFullPipeline(string input)
    {
        var plan = await _ai.AskPlanAsync(BuildPlannerPrompt(input));

        var memory = new AgentMemory();

        if (plan?.Steps == null || plan.Steps.Count == 0)
            throw new InvalidOperationException("The AI did not return a valid execution plan.");

        foreach (var step in plan.Steps)
        {
            var result = await Execute(step);

            memory.Steps.Add(step.Action);
            memory.LastResult = JsonSerializer.Serialize(result);

            // optional future critic hook
        }

        return memory;
    }

    // =========================
    // DREAM MODE
    // =========================
    public async Task RunDreamCycle(CancellationToken token)
    {
        var tasks = await _context.Tasks.ToListAsync(token);

        var prompt = $@"
You are a productivity optimizer.

Improve this task list:
- remove duplicates
- add missing tasks
- reorder priorities
- improve clarity

Tasks:
{JsonSerializer.Serialize(tasks)}
";

        var result = await _ai.AskAsync(prompt);

        Console.WriteLine("💭 DREAM RESULT:");
        Console.WriteLine(result);
    }
}
