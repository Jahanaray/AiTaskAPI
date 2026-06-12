using AiTaskApi.Shared.Helper;
using AiTaskApi.Shared.Models.Agent;
using System.Net.Http.Json;
using System.Text.Json;

namespace AiTaskApi.Services;

public class AiService : IAiService
{
    private readonly HttpClient _http;
    private readonly string _llmURL;
    private readonly string _ollamaModel;
    private static readonly object DeepThinkingOptions = new
    {
        temperature = 0.2,
        top_p = 0.9,
        repeat_penalty = 1.08,
        num_ctx = 8192,
        num_predict = 2048
    };

    public AiService(HttpClient http)
    {
        _http = http;
        var url = ServerAddress.LlmServer;
        _llmURL = url + "/api/generate";

        _ollamaModel =
            Environment.GetEnvironmentVariable("OLLAMA_MODEL")
            ?? "gemma4:e4b";
    }

    // =========================
    // CHAT
    // =========================
    public async Task<string> AskAsync(string message)
    {
        var request = new
        {
            model = _ollamaModel,
            prompt = message,
            stream = false,
            options = DeepThinkingOptions,
            keep_alive = "30m"
        };

        var response = await _http.PostAsJsonAsync(_llmURL, request);
        var raw = await response.Content.ReadAsStringAsync();

        var wrapper = JsonSerializer.Deserialize<OllamaResponse>(raw,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        return wrapper?.response ?? "";
    }

    // =========================
    // PLAN
    // =========================
    public async Task<AiPlanResponse?> AskPlanAsync(string prompt)
    {
        var request = new
        {
            model = _ollamaModel,
            prompt,
            stream = false,
            format = "json",
            options = DeepThinkingOptions,
            keep_alive = "30m"
        };

        var response = await _http.PostAsJsonAsync(_llmURL, request);
        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return null;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var direct = JsonSerializer.Deserialize<AiPlanResponse>(raw, options);
        if (direct?.Steps?.Count > 0)
            return direct;

        var wrapper = JsonSerializer.Deserialize<OllamaResponse>(raw, options);

        if (!string.IsNullOrWhiteSpace(wrapper?.response))
            return JsonSerializer.Deserialize<AiPlanResponse>(wrapper.response, options);

        return null;
    }
    

    public async Task<AiTaskResult?> AskStructuredAsync(string message)
    {
        var prompt = $@"
You are a careful task extraction system for a production task manager.
Think deeply before answering. Silently infer the user's actual task intent,
priority, due date, and useful execution context. Do not expose reasoning.

Return ONLY JSON:
{{
  ""title"": ""string"",
  ""description"": ""string"",
  ""dueDate"": ""yyyy-MM-dd or null"",
  ""priority"": ""Low | Medium | High""
}}

User:
{message}
";

        var request = new
        {
            model = _ollamaModel,
            prompt,
            stream = false,
            format = "json",
            options = DeepThinkingOptions,
            keep_alive = "30m"
        };

        var response = await _http.PostAsJsonAsync(_llmURL, request);
        var raw = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            return null;

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        var wrapper = JsonSerializer.Deserialize<OllamaResponse>(raw, options);

        if (string.IsNullOrWhiteSpace(wrapper?.response))
            return null;

        return JsonSerializer.Deserialize<AiTaskResult>(wrapper.response, options);
    }
}

public class OllamaResponse
{
    public string response { get; set; } = "";
}


