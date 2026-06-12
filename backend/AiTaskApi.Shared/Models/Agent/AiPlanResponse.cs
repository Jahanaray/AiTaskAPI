using System.Text.Json.Serialization;

namespace AiTaskApi.Shared.Models.Agent;

public class AiPlanResponse
{
    [JsonPropertyName("steps")]
    public List<AiActionResponse> Steps { get; set; } = new();
}