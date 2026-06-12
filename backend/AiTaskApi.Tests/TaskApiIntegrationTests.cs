using System.Net.Http.Json;
using Xunit;

public class TaskApiIntegrationTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public TaskApiIntegrationTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Create_Task_EndToEnd_Should_Return_Success()
    {
        // Arrange
        var dto = new TaskCreateDto
        {
            Title = "Integration Task",
            Description = "Test API flow"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/tasks", dto);

        // Assert
        response.EnsureSuccessStatusCode();
    }
}