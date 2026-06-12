using System.Text.Json.Serialization;

namespace AiTaskApi.Shared.Models.Agent
{
    public class AiTaskData
    {
        [JsonPropertyName("title")]
        public string? Title { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("priority")]
        public string? Priority { get; set; }

        [JsonPropertyName("dueDate")]
        public string? DueDate { get; set; }
    }
}
