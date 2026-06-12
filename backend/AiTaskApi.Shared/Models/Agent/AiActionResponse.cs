using System.Text.Json.Serialization;

namespace AiTaskApi.Shared.Models.Agent;

public class AiActionResponse
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = "";

    [JsonPropertyName("taskId")]
    public int? TaskId { get; set; }

    [JsonPropertyName("data")]
    public AiTaskData? Data { get; set; }
}