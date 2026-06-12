# Architecture

## System Overview

AiTaskApi is a microservice-style backend composed of three independent but interconnected projects:

```mermaid
graph TB
    subgraph Client
        Frontend[Frontend App]
    end
    
    subgraph AiTaskApi
        Controllers[Controllers Layer]
        Services[Services Layer]
        DbContext[AppDbContext]
    end
    
    subgraph AiTaskApi.Worker
        Worker[Background Worker]
        AgentService[AgentService]
    end
    
    subgraph Shared
        DTOs[DTOs]
        Models[Domain Models]
    end
    
    subgraph External
        PostgreSQL[(PostgreSQL)]
        Ollama[Ollama LLM Server]
    end
    
    Frontend -->|REST API| Controllers
    Controllers --> Services
    Services --> DbContext
    DbContext --> PostgreSQL
    Services -->|HTTP| Ollama
    Worker --> AgentService
    AgentService -->|HTTP| Ollama
    Worker --> DbContext
    DbContext --> PostgreSQL
    Controllers -.->|Reference| DTOs
    Controllers -.->|Reference| Models
    Worker -.->|Reference| DTOs
    Worker -.->|Reference| Models
```

## Project Responsibilities

### AiTaskApi (REST API Server)

The main web application built on ASP.NET Core 10. Handles all HTTP requests.

**Layers:**

| Layer | Location | Responsibility |
|-------|----------|----------------|
| Controllers | [`Controllers/`](../AiTaskApi/Controllers/) | HTTP request handling, routing |
| Services | [`Services/`](../AiTaskApi/Services/) | Business logic |
| Data | [`Data/`](../AiTaskApi/Data/) | EF Core DbContext and database access |
| Models | [`Models/`](../AiTaskApi/Models/) + [`DTOs/`](../AiTaskApi/DTOs/) | Domain models and request/response DTOs |
| Common | [`Common/`](../AiTaskApi/Common/) | Shared utilities (e.g., ApiResponseHelper) |

**Note:** The `WeatherForecastController.cs` file is a template controller included by ASP.NET Core and is not part of the application's core functionality.

**Key Controllers:**

| Controller | Route | Purpose |
|------------|-------|---------|
| [`AuthController`](../AiTaskApi/Controllers/AuthController.cs) | `/api/auth` | User registration and JWT authentication |
| [`TasksController`](../AiTaskApi/Controllers/TasksController.cs) | `/api/tasks` | Task CRUD operations with pagination |
| [`AiController`](../AiTaskApi/Controllers/AiController.cs) | `/api/ai` | AI chat, task creation, agent management |

### AiTaskApi.Shared (Shared Library)

A .NET class library containing models and DTOs shared between the API and Worker projects.

**Contents:**

| Folder | Contents |
|--------|----------|
| [`DTOs/`](../AiTaskApi.Shared/DTOs/) | `TaskCreateDto`, `TaskReadDto`, `AiTaskResult` |
| [`Models/`](../AiTaskApi.Shared/Models/) | `TaskItem`, `AgentJob`, `AgentMemory`, `AiActionResponse`, `AiPlanResponse` |
| [`Helper/`](../AiTaskApi.Shared/Helper/) | `AgentType` enum, `ServerAddress` static helper |

### AiTaskApi.Worker (Background Service)

A .NET Worker Service that processes AI agent jobs asynchronously.

**Responsibilities:**

1. Polls the database for pending `AgentJob` records
2. Executes agent pipelines via [`AgentService`](../AiTaskApi/Services/AgentService.cs)
3. Implements retry logic with exponential backoff
4. Runs "Dream Mode" cycles when idle and Ollama is available

### AiTaskApi.Tests (Test Project)

Contains integration and unit tests:

| File | Purpose |
|------|---------|
| [`TaskApiIntegrationTests.cs`](../AiTaskApi.Tests/TaskApiIntegrationTests.cs) | API endpoint integration tests |
| [`TaskServiceTests.cs`](../AiTaskApi.Tests/TaskServiceTests.cs) | TaskService unit tests |
| [`FakeAiService.cs`](../AiTaskApi.Tests/FakeAiService.cs) | Mock AI service for testing |
| [`TestAuthHandler.cs`](../AiTaskApi.Tests/TestAuthHandler.cs) | Test JWT authentication handler |

## Data Flow

### Task Creation (Manual)

```mermaid
sequenceDiagram
    participant Client
    participant TasksController
    participant TaskService
    participant AppDbContext
    participant PostgreSQL
    
    Client->>TasksController: POST /api/tasks
    TasksController->>TaskService: Create(dto)
    TaskService->>AppDbContext: Add(TaskItem)
    AppDbContext->>PostgreSQL: INSERT tasks
    PostgreSQL-->>AppDbContext: Success
    AppDbContext-->>TaskService: TaskReadDto
    TaskService-->>TasksController: TaskReadDto
    TasksController-->>Client: 200 OK
```

### AI Task Creation (Async Agent)

```mermaid
sequenceDiagram
    participant Client
    participant AiController
    participant AppDbContext
    participant Worker
    participant AgentService
    participant AiService
    participant Ollama
    
    Client->>AiController: POST /api/ai/agent-loop
    AiController->>AppDbContext: Add AgentJob (Pending)
    AppDbContext-->>AiController: 202 Accepted
    AiController-->>Client: jobId
    
    Note over Worker: Polls every 3 seconds
    Worker->>AppDbContext: Get pending jobs
    Worker->>AgentService: RunFullPipeline(prompt)
    AgentService->>AiService: AskPlanAsync(prompt)
    AiService->>Ollama: POST /api/generate
    Ollama-->>AiService: Plan JSON
    AiService-->>AgentService: AiPlanResponse
    
    loop For each step
        AgentService->>AppDbContext: Execute action
    end
    
    Worker->>AppDbContext: Update job status (Done/Failed)
```

## Authentication Flow

```mermaid
sequenceDiagram
    participant Client
    participant AuthController
    participant AuthService
    participant AppDbContext
    participant PostgreSQL
    
    Note over Client,PostgreSQL: Registration
    Client->>AuthController: POST /api/auth/register
    AuthController->>AuthService: Register(username, password)
    AuthService->>PostgreSQL: Check username exists
    AuthService->>AuthService: BCrypt.HashPassword(password)
    AuthService->>PostgreSQL: INSERT users
    
    Note over Client,PostgreSQL: Login
    Client->>AuthController: POST /api/auth/login
    AuthController->>AuthService: Login(username, password)
    AuthService->>PostgreSQL: SELECT user by username
    AuthService->>AuthService: BCrypt.Verify(password, hash)
    AuthService-->>AuthController: User
    AuthController->>AuthController: Create JWT token
    AuthController-->>Client: { token }
    
    Note over Client,PostgreSQL: Protected Request
    Client->>AuthController: GET /api/tasks [Authorization: Bearer <token>]
    AuthController->>AuthController: Validate JWT
    AuthController->>TaskService: GetAll()
```

## Agent Types

The system supports four agent types defined in [`AgentType`](../AiTaskApi.Shared/Helper/AgentType.cs):

| Type | Description |
|------|-------------|
| `Planner` | Breaks user requests into executable steps |
| `Executor` | Executes individual plan steps |
| `Critic` | Reviews and validates plan results (placeholder) |
| `Dreamer` | Runs idle-time productivity optimization |

## Configuration Architecture

Configuration is resolved in the following priority order (highest to lowest):

1. Environment variables (e.g., `ConnectionStrings__Default`, `LLM_SERVER`)
2. `appsettings.{Environment}.json` files
3. Hardcoded defaults in code

**Key configuration points:**

| Setting | Source | Default |
|---------|--------|---------|
| Database connection | `ConnectionStrings__Default` env or `appsettings.json` | Empty (must be set) |
| LLM server URL | `LLM_SERVER` env or [`ServerAddress.LlmServer`](../AiTaskApi.Shared/Helper/ServerAddress.cs) | `http://host.docker.internal:11434` (Docker) / `http://localhost:11434` (local) |
| Ollama model | `OLLAMA_MODEL` env or code default | `gemma4:e4b` |
| JWT secret | Environment variable `JWT_SECRET` or config `Jwt:Secret` | Required (must be set before running) |
| CORS origins | [`Program.cs`](../AiTaskApi/Program.cs:54-63) | `http://localhost:5173`, `http://localhost:3000` |
| Agent retry delay | Hardcoded in [`Worker.cs`](../AiTaskApi.Worker/Worker.cs:10) | 10 seconds |

## Security Considerations

- The JWT signing key is externalized via `JWT_SECRET` environment variable or `Jwt:Secret` configuration ([`Program.cs`](../AiTaskApi/Program.cs:71-74)).
- Passwords are hashed using BCrypt.Net-Next (industry standard).
- CORS is configured for specific localhost origins; adjust for production frontend domains.
- HTTPS redirection is enabled by default.
