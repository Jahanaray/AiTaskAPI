namespace AiTaskApi.Shared.Helper
{
    public static class ServerAddress
    {
        public static string LlmServer { get; } =
            Environment.GetEnvironmentVariable("LLM_SERVER")
            ?? "http://192.168.10.7:11434";
        //public static string LlmServer { get; } = "http://127.0.0.1:11434";
        //public static string LlmServer { get; } = "http://192.168.10.101:11434";
    }
}

