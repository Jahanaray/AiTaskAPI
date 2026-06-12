namespace AiTaskApi.Shared.Models.Agent
{
    public class AgentMemory
    {
        public List<string> Steps { get; set; } = new();
        public string? LastResult { get; set; }
    }
}
