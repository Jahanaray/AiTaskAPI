public class FakeAiService : IAiService
{
    public Task<string> AskAsync(string message)
    {
        return Task.FromResult("Mocked AI response");
    }
}