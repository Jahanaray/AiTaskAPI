using System;
using System.Collections.Generic;
using System.Text;

namespace AiTaskApi.Shared.Models.Agent
{
    public class AgentJob
    {
        public int Id { get; set; }
        public string Prompt { get; set; } = "";
        public string Status { get; set; } = "Pending"; // Pending | Running | Done | Failed
        public string? Result { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public AgentType Type { get; set; } = AgentType.Planner;
        public string? Context { get; set; }
        public int AttemptCount { get; set; } = 0;
        public int MaxAttempts { get; set; } = 4;
        public DateTime? NextAttemptAt { get; set; }
        public string? LastError { get; set; }
    }
}
