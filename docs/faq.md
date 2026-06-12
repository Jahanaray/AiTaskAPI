# FAQ

## General Questions

### What is AiTaskApi?

AiTaskApi is a .NET 10 backend system that combines traditional task management with AI-powered autonomous agent capabilities. It allows users to manage tasks manually or through natural language commands processed by a local LLM (Ollama).

### What technologies does this project use?

| Technology | Purpose |
|------------|---------|
| ASP.NET Core 10 | Web API framework |
| Entity Framework Core 10 | ORM for database access |
| PostgreSQL | Primary database |
| Ollama | Local LLM server (optional) |
| JWT Bearer | Authentication |
| BCrypt.Net-Next | Password hashing |
| Swagger/OpenAPI | API documentation |
| Docker | Containerization |

### Is this production-ready?

**Assumptions documented during analysis:** The system has a functional architecture but several areas need attention before production deployment:

- JWT signing key is hardcoded (should be externalized)
- CORS is limited to localhost origins
- No rate limiting is configured
- No input validation beyond basic DTO constraints
- No audit logging

## AI Features

### How does the AI task creation work?

The system uses a two-phase approach:

1. **Planning phase:** The LLM receives the user's natural language request and generates an execution plan with discrete steps (e.g., "CreateTask", "GetTasks", "DeleteTask")
2. **Execution phase:** Each step in the plan is executed against the database

This is implemented in [`AgentService.RunAsync()`](../AiTaskApi/Services/AgentService.cs:23) and [`AgentService.RunFullPipeline()`](../AiTaskApi/Services/AgentService.cs:154).

### What LLM models are supported?

Any model compatible with Ollama's API. Configure via the `LLM_SERVER` and `OLLAMA_MODEL` environment variables. See [`backend/.env.example`](../backend/.env.example).

Supported models include:
- `gemma4:e4b` (default, lightweight)
- `llama3` (balanced)
- `mistral` (fast)
- `phi3` (compact)
- Any other Ollama-compatible model

### Can I use a cloud LLM instead of Ollama?

The system is designed for local Ollama servers. To use a cloud LLM, you would need to modify [`AiService`](../backend/AiTaskApi/Services/AiService.cs) to call the cloud API instead of Ollama's `/api/generate` endpoint.

### What happens if the AI fails to respond?

The worker service implements retry logic:
- Default max attempts: 4 ([`AgentJob.MaxAttempts`](../backend/AiTaskApi.Shared/Models/Agent/AgentJob.cs:18))
- Retry delay: Exponential backoff starting at 10 seconds ([`Worker.cs`](../backend/AiTaskApi.Worker/Worker.cs:10))
- After max attempts: Job status changes to `Failed`

### What is "Dream Mode"?

Dream Mode is an idle-time feature where the worker automatically analyzes and optimizes the task list when:
1. No pending agent jobs exist
2. Ollama is idle (no models loaded)

It runs a productivity optimization cycle that suggests improvements to the existing task list. Implemented in [`Worker.RunDreamCycle()`](../backend/AiTaskApi.Worker/Worker.cs:108).

### How long do AI requests take?

Simple chat responses typically complete in seconds. Complex agent pipelines with multiple steps may take several minutes depending on:
- LLM model size and speed
- Number of plan steps
- Network latency to Ollama server

## Authentication

### How long do JWT tokens last?

Tokens expire after **2 hours** ([`AuthController.cs`](../backend/AiTaskApi/Controllers/AuthController.cs:58)). Users must re-login to obtain a new token.

### Can I extend token expiration?

Modify the expiration in [`AuthController.CreateToken()`](../backend/AiTaskApi/Controllers/AuthController.cs:58):

```csharp
expires: DateTime.Now.AddHours(24),  // Change from 2 to 24 hours
```

**Security note:** Shorter expiration is recommended for production. Implement token refresh instead of extending expiration.

### Why am I getting 401 Unauthorized errors?

Common causes:
1. Missing `Authorization` header
2. Token expired (check 2-hour limit)
3. Malformed token (extra spaces, quotes)
4. Wrong scheme (use `Bearer`, not `Token`)

**Correct format:**
```
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### Is password storage secure?

Yes. Passwords are hashed using BCrypt.Net-Next with a cost factor of 10 (default). This is the industry standard for password hashing.

## Database

### What database schemas are created?

| Table | Purpose | Key Columns |
|-------|---------|-------------|
| `tasks` | Task storage | id, title, description, priority, status, dueDate, isDone |
| `users` | User accounts | id, username, passwordHash |
| `agentjobs` | Agent job tracking | id, prompt, status, result, createdAt, attemptCount, maxAttempts |

### Can I use a different database?

The project uses PostgreSQL via Npgsql. Switching databases would require:
1. Changing the EF Core provider (e.g., to SqlServer)
2. Updating connection strings
3. Potentially adjusting query syntax for compatibility

### How are migrations handled?

Migrations are applied automatically on application startup via [`Program.cs`](../backend/AiTaskApi/Program.cs:142-146):

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```

### Can I reset the database?

Yes, but data will be lost:

```bash
cd AiTaskApi
dotnet ef database drop --force
dotnet ef database update
```

## Worker Service

### What does the Worker do?

The Worker is a background service that:
1. Polls for pending agent jobs every 3 seconds
2. Executes agent pipelines via the LLM
3. Handles retry logic with exponential backoff
4. Runs Dream Mode cycles during idle time

### Do I need to run the Worker separately?

Yes. The Worker is a separate project (`AiTaskApi.Worker`) that must be started independently:

```bash
cd AiTaskApi.Worker
dotnet run
```

The API project does not include the Worker by default (it's commented out in [`Program.cs`](../backend/AiTaskApi/Program.cs:38)).

### How do I monitor the Worker?

Check agent job status via the API:

```bash
curl http://localhost:5000/api/ai/jobs
```

Or watch worker logs:

```bash
cd AiTaskApi.Worker
dotnet run --verbosity detailed
```

## Deployment

### How do I deploy to production?

1. Set production environment variables (connection string, LLM server, JWT secret)
2. Build release binaries:
   ```bash
   dotnet publish AiTaskApi/AiTaskApi.csproj -c Release -o ./publish
   dotnet publish AiTaskApi.Worker/AiTaskApi.Worker.csproj -c Release -o ./publish-worker
   ```
3. Deploy to your hosting platform (Linux server, Azure, AWS, etc.)

### Can I deploy with Docker Compose?

Yes. Example `docker-compose.yml`:

```yaml
version: '3.8'
services:
  api:
    build: ./AiTaskApi
    ports:
      - "5000:8080"
    environment:
      - ConnectionStrings__Default=Host=db;Database=aitaskapi;Username=postgres;Password=yourpassword
      - LLM_SERVER=http://ollama:11434
    depends_on:
      - db
  
  worker:
    build: ./AiTaskApi.Worker
    environment:
      - ConnectionStrings__Default=Host=db;Database=aitaskapi;Username=postgres;Password=yourpassword
      - LLM_SERVER=http://ollama:11434
    depends_on:
      - db
  
  db:
    image: postgres:16
    environment:
      - POSTGRES_PASSWORD=yourpassword
      - POSTGRES_DB=aitaskapi
    volumes:
      - pgdata:/var/lib/postgresql/data

volumes:
  pgdata:
```

### What environment variables are required in production?

| Variable | Required | Description |
|----------|----------|-------------|
| `ConnectionStrings__Default` | Yes | PostgreSQL connection string |
| `LLM_SERVER` | Optional (AI features) | Ollama server URL |
| `OLLAMA_MODEL` | Optional (AI features) | LLM model name |

**Recommended:** Also set a secure JWT signing key via environment variable (`JWT_SECRET`). See [`backend/.env.example`](../backend/.env.example).

## Performance

### How many concurrent users can this handle?

This depends on your hosting infrastructure. The application itself is stateless and can scale horizontally. Key factors:
- Database connection pool size
- LLM server capacity
- Worker service concurrency (currently sequential)

### How can I improve performance?

| Area | Recommendation |
|------|---------------|
| Database | Add indexes on frequently queried columns |
| AI requests | Use async endpoints (`agent-loop`) instead of sync |
| Connection pooling | Configure PostgreSQL connection pool settings |
| Caching | Add response caching for frequently accessed data |
| Worker | Implement parallel job processing for multiple pending jobs |

## Testing

### How do I run tests?

```bash
dotnet test AiTaskApi.Tests/AiTaskApi.Tests.csproj
```

### What tests are included?

| Test File | Type | Coverage |
|-----------|------|----------|
| `TaskApiIntegrationTests.cs` | Integration | API endpoints |
| `TaskServiceTests.cs` | Unit | TaskService logic |
| `FakeAiService.cs` | Mock | AI service replacement |
| `TestAuthHandler.cs` | Test helper | JWT validation bypass |

### How do I add new tests?

1. Create a test class in `AiTaskApi.Tests/`
2. Use the provided `ApiFactory` for integration tests
3. Use `FakeAiService` to mock AI dependencies

## Common Issues

### Why are my agent jobs stuck in "Pending"?

Check:
1. Worker service is running
2. Database connection is correct in Worker
3. `NextAttemptAt` hasn't been set to a future time
4. Ollama is accessible (required for execution)

### Why does the AI return empty or invalid responses?

Check:
1. Ollama is running and accessible
2. The specified model is pulled and valid
3. The prompt is well-formed
4. Network connectivity to LLM server

### Can I use this with a frontend framework?

Yes. The API supports CORS for `http://localhost:5173` (Vite) and `http://localhost:3000` (React dev). Add your frontend's origin in [`Program.cs`](../backend/AiTaskApi/Program.cs:57-59).

### Is there a mobile SDK?

No dedicated mobile SDK exists. The REST API can be consumed from any platform that supports HTTP requests with JWT authentication.

## Security

### What security measures are in place?

| Measure | Implementation |
|---------|----------------|
| Password hashing | BCrypt.Net-Next |
| Authentication | JWT Bearer tokens |
| HTTPS | HTTPS redirection enabled |
| CORS | Origin whitelist |
| Input validation | DTO constraints |

### What security improvements are recommended for production?

**Assumptions documented during analysis:**

1. **Externalize JWT secret** - Configure via `JWT_SECRET` environment variable (see [`backend/.env.example`](../backend/.env.example))
2. **Add rate limiting** - No rate limiting is currently configured
3. **Add input validation** - Use FluentValidation or similar
4. **Add audit logging** - Track user actions for compliance
5. **Implement token refresh** - Currently tokens expire without refresh mechanism
6. **Add HTTPS certificates** - Use proper certificates, not development certs
7. **Add CORS configuration management** - Make origins configurable
8. **Add request logging** - Use Serilog or similar for production logging

## Migration History

### What database changes have been made?

| Date | Migration | Changes |
|------|-----------|---------|
| 2026-04-19 | `Init` | Initial schema |
| 2026-04-20 | `AddUsers` | User table for authentication |
| 2026-04-21 | `AddTaskFields` | Task model fields |
| 2026-05-05 | `UpdateTasks` | Task model updates |
| 2026-05-05 | `AddAgentJobs` | AgentJob table for async processing |
| 2026-05-05 | `SyncModel` | Model synchronization |
| 2026-05-31 | `UpdateModel` | Model updates |
| 2026-06-09 | `AddAgentJobRetries` | Retry fields on AgentJob |

## Getting Help

If your question isn't answered here:

1. Check the [Troubleshooting](./troubleshooting.md) guide
2. Review the [API Reference](./api-reference.md) for endpoint details
3. Check the [Architecture](./architecture.md) document for system design
4. Examine the source code - it is well-structured and self-documenting
