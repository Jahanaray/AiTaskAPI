# Development Guide

## Developer Onboarding

This guide helps new developers set up their development environment and understand the codebase.

### Prerequisites Checklist

- [ ] [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) installed
- [ ] PostgreSQL 14+ running locally or accessible
- [ ] Ollama installed (for AI features development)
- [ ] Git installed
- [ ] IDE: Visual Studio 2022+, VS Code with C# Dev Kit, or Rider

### Initial Setup

```bash
# 1. Clone the repository
git clone <repository-url>
cd backend

# 2. Restore all projects
dotnet restore

# 3. Create local development database
# In PostgreSQL:
CREATE DATABASE aitaskapi_dev;

# 4. Update connection string in AiTaskApi/appsettings.json
# Set ConnectionStrings:Default to your local database

# 5. Apply migrations
cd AiTaskApi
dotnet ef database update

# 6. Start the API (Terminal 1)
dotnet run

# 7. Start the Worker (Terminal 2)
cd ../AiTaskApi.Worker
dotnet run
```

## Project Structure Deep Dive

### AiTaskApi/

Main REST API project.

```
AiTaskApi/
├── Controllers/              # HTTP request handlers
│   ├── AuthController.cs     # Registration and login
│   ├── TasksController.cs    # Task CRUD operations
│   ├── AiController.cs       # AI chat and agent endpoints
│   └── WeatherForecastController.cs    # Template controller (Swagger demo)
├── Data/
│   └── AppDbContext.cs       # EF Core DbContext
├── DTOs/                     # Request DTOs (input models)
│   ├── LoginDto.cs
│   └── RegisterDto.cs
├── Models/                   # Domain models and response types
│   ├── ApiResponse.cs        # Standard response envelope
│   ├── PagedResult.cs        # Pagination wrapper
│   └── User.cs               # User entity
├── Services/                 # Business logic layer
│   ├── AuthService.cs        # User registration/login logic
│   ├── TaskService.cs        # Task CRUD logic
│   ├── AiService.cs          # Ollama LLM integration
│   └── AgentService.cs       # AI agent pipeline logic
├── Common/
│   └── ApiResponseHelper.cs  # Response factory utilities
├── Migrations/               # EF Core migrations
├── Properties/
│   └── launchSettings.json   # Development server settings
├── appsettings.json          # Configuration
├── Dockerfile                # Container build
└── Program.cs                # Application entry point
```

### AiTaskApi.Shared/

Shared library for models and DTOs used across projects.

```
AiTaskApi.Shared/
├── DTOs/                     # Transfer objects
│   ├── TaskCreateDto.cs      # Task creation input
│   ├── TaskReadDto.cs        # Task response output
│   └── AiTaskResult.cs       # AI-extracted task data
├── Models/                   # Domain models
│   ├── TaskItem.cs           # Task entity
│   └── Agent/                # Agent system models
│       ├── AgentJob.cs       # Async job tracking
│       ├── AgentMemory.cs    # Agent execution memory
│       ├── AiActionResponse.cs  # AI action definition
│       └── AiPlanResponse.cs  # AI plan definition
└── Helper/                   # Utilities
    ├── AgentType.cs          # Agent type enum
    └── ServerAddress.cs      # LLM server address helper
```

### AiTaskApi.Worker/

Background worker service for async agent processing.

```
AiTaskApi.Worker/
├── Worker.cs                 # Background service implementation
├── Data/
│   └── WorkerDbContext.cs    # Worker-specific DbContext (if needed)
├── Properties/
│   └── launchSettings.json
├── appsettings.json          # Configuration
├── Dockerfile                # Container build
└── Program.cs                # Entry point
```

### AiTaskApi.Tests/

Test project for unit and integration tests.

```
AiTaskApi.Tests/
├── TaskApiIntegrationTests.cs  # API endpoint tests
├── TaskServiceTests.cs         # Service layer tests
├── FakeAiService.cs            # Mock AI service
├── TestAuthHandler.cs          # Test JWT handler
└── ApiFactory.cs               # WebApplicationFactory setup
```

## Codebase Conventions

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Classes | PascalCase | `TaskService`, `AgentJob` |
| Methods | PascalCase | `GetAll()`, `RunAsync()` |
| Properties | PascalCase | `Title`, `Description` |
| Private fields | underscore prefix | `_context`, `_ai` |
| DTOs | Suffix with `Dto` | `TaskCreateDto` |
| Services | Suffix with `Service` | `TaskService` |
| Interfaces | Prefix with `I` | `IAiService` |

### Response Pattern

All API responses use the [`ApiResponse<T>`](../AiTaskApi/Models/ApiResponse.cs) envelope:

```csharp
// Success
return Ok(ApiResponseHelper.Success(data, "Message"));

// Failure
return NotFound(ApiResponseHelper.Fail<T>("Error message"));
```

### Service Layer Pattern

Business logic is separated into service classes:

```csharp
// Registration in Program.cs
builder.Services.AddScoped<TaskService>();

// Usage in controller
public class TasksController : ControllerBase
{
    private readonly TaskService _service;
    
    public TasksController(TaskService service)
    {
        _service = service;
    }
}
```

### DTO Pattern

Input and output types are separated:

- **DTOs** (Data Transfer Objects) for input: `TaskCreateDto`, `LoginDto`
- **Read DTOs** for output: `TaskReadDto`
- **Domain models** for persistence: `TaskItem`, `User`

### Agent System Pattern

The agent system follows a planner-executor pattern:

1. **Planner** ([`AiService.AskPlanAsync()`](../backend/AiTaskApi/Services/AiService.cs:59)): LLM generates execution plan
2. **Executor** ([`AgentService.Execute()`](../backend/AiTaskApi/Services/AgentService.cs:101)): Each step is executed
3. **Memory** ([`AgentMemory`](../backend/AiTaskApi.Shared/Models/Agent/AgentMemory.cs)): Tracks execution history

## Adding New Features

### Adding a New Endpoint

1. **Create or update the controller** in [`Controllers/`](../AiTaskApi/Controllers/):

```csharp
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NewController : ControllerBase
{
    private readonly SomeService _service;
    
    public NewController(SomeService service)
    {
        _service = service;
    }
    
    [HttpGet("new-endpoint")]
    public async Task<IActionResult> GetNewData()
    {
        var result = await _service.GetNewData();
        return Ok(ApiResponseHelper.Success(result));
    }
}
```

2. **Add the service** in [`Services/`](../AiTaskApi/Services/):

```csharp
public class SomeService
{
    private readonly AppDbContext _context;
    
    public SomeService(AppDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<NewItem>> GetNewData()
    {
        return await _context.NewItems.ToListAsync();
    }
}
```

3. **Register the service** in [`Program.cs`](../backend/AiTaskApi/Program.cs:30-33):

```csharp
builder.Services.AddScoped<SomeService>();
```

4. **Add migration** (if database changes):

```bash
dotnet ef migrations add AddNewEntity --project AiTaskApi
dotnet ef database update --project AiTaskApi
```

### Adding a New Model

1. **Define the model** in [`backend/AiTaskApi.Shared/Models/`](../backend/AiTaskApi.Shared/Models/):

```csharp
public class NewItem
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}
```

2. **Add to DbContext** in [`backend/AiTaskApi/Data/AppDbContext.cs`](../backend/AiTaskApi/Data/AppDbContext.cs):

```csharp
public DbSet<NewItem> NewItems => Set<NewItem>();
```

3. **Create migration:**

```bash
dotnet ef migrations add AddNewItem --project AiTaskApi
```

### Adding a New Agent Type

1. **Extend the enum** in [`backend/AiTaskApi.Shared/Helper/AgentType.cs`](../backend/AiTaskApi.Shared/Helper/AgentType.cs):

```csharp
public enum AgentType
{
    Planner,
    Executor,
    Critic,
    Dreamer,
    Analyzer     // New type
}
```

2. **Add execution logic** in [`backend/AiTaskApi/Services/AgentService.cs`](../backend/AiTaskApi/Services/AgentService.cs:101):

```csharp
private async Task<object?> Execute(AiActionResponse step)
{
    return step.Action switch
    {
        "CreateTask" => await CreateTask(step),
        "GetTasks" => await _context.Tasks.ToListAsync(),
        "DeleteTask" => await DeleteTask(step),
        "Analyze" => await Analyze(step),  // New case
        _ => $"Unknown: {step.Action}"
    };
}
```

## Running Tests

### Run All Tests

```bash
dotnet test AiTaskApi.Tests/AiTaskApi.Tests.csproj
```

### Run Specific Test Class

```bash
dotnet test --filter "FullyQualifiedName~TaskApiIntegrationTests"
```

### Run with Coverage

```bash
dotnet test /p:CollectCoverage=true
```

### Test Architecture

The test project uses:

- **WebApplicationFactory** for integration testing without starting a real server
- **FakeAiService** to mock LLM responses
- **TestAuthHandler** for fake JWT validation

## Database Migrations

### Create a New Migration

```bash
cd AiTaskApi
dotnet ef migrations add DescriptionOfChange
```

### Apply Migrations

```bash
dotnet ef database update
```

### Remove Last Migration

```bash
dotnet ef migrations remove
```

### List All Migrations

```bash
dotnet ef migrations list
```

### Generate SQL Script

```bash
dotnet ef migrations script --id "InitialCreate"
```

## Debugging Tips

### Enable Detailed Logging

In [`appsettings.Development.json`](../AiTaskApi/appsettings.Development.json):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Information",
      "Microsoft.EntityFrameworkCore": "Debug"
    }
  }
}
```

### Debug Agent Pipeline

Add breakpoints in:

- [`AgentService.RunAsync()`](../backend/AiTaskApi/Services/AgentService.cs:23) - Pipeline entry
- [`AgentService.Execute()`](../backend/AiTaskApi/Services/AgentService.cs:101) - Step execution
- [`Worker.RunJob()`](../backend/AiTaskApi.Worker/Worker.cs:62) - Worker job processing

### Debug AI Responses

The LLM request/response flow:

1. [`AiService.AskAsync()`](../backend/AiTaskApi/Services/AiService.cs:36) - Simple chat
2. [`AiService.AskPlanAsync()`](../backend/AiTaskApi/Services/AiService.cs:59) - Plan generation
3. [`AiService.AskStructuredAsync()`](../backend/AiTaskApi/Services/AiService.cs:95) - Structured extraction

Set breakpoints in these methods to inspect LLM requests and responses.

### Debug Database Queries

EF Core logs all SQL queries when `Microsoft.EntityFrameworkCore` log level is set to `Debug` or `Information`.

## Code Review Checklist

Before submitting changes:

- [ ] Code follows naming conventions
- [ ] New services are registered in [`backend/AiTaskApi/Program.cs`](../backend/AiTaskApi/Program.cs)
- [ ] Database migrations are added (if models changed)
- [ ] Tests are updated or added
- [ ] Swagger documentation is accurate
- [ ] Error handling is appropriate
- [ ] No hardcoded secrets or credentials
- [ ] CORS origins updated (if new frontend domains)

## CI/CD Considerations

### Build Commands

```bash
# Build API
dotnet build AiTaskApi/AiTaskApi.csproj -c Release

# Build Worker
dotnet build AiTaskApi.Worker/AiTaskApi.Worker.csproj -c Release

# Run tests
dotnet test AiTaskApi.Tests/AiTaskApi.Tests.csproj -c Release

# Publish API
dotnet publish AiTaskApi/AiTaskApi.csproj -c Release -o ./publish

# Publish Worker
dotnet publish AiTaskApi.Worker/AiTaskApi.Worker.csproj -c Release -o ./publish-worker
```

### Docker Build

```bash
# Build API image
docker build -f AiTaskApi/Dockerfile -t aitaskapi:latest .

# Build Worker image
docker build -f AiTaskApi.Worker/Dockerfile -t aitaskapi-worker:latest .
```

## Contributing Guidelines

1. Create a feature branch from `main`
2. Make small, focused changes
3. Add tests for new functionality
4. Update documentation if behavior changes
5. Ensure all tests pass before submitting
6. Write clear commit messages

## Common Development Tasks

### Change the LLM Model (Development)

```bash
# Set environment variable
$env:OLLAMA_MODEL="llama3"

# Or modify ServerAddress.cs temporarily
# In backend/AiTaskApi.Shared/Helper/ServerAddress.cs
```

### Switch Database (Development)

```bash
# Update appsettings.Development.json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=aitaskapi_dev;Username=postgres;Password=devpass"
  }
}
```

### Add New CORS Origin

In [`Program.cs`](../AiTaskApi/Program.cs:57-59):

```csharp
policy.WithOrigins(
    "http://localhost:5173",
    "http://localhost:3000",
    "https://new-frontend.example.com"  // Add this
)
```
