# AiTaskApi

A .NET 10 AI-powered task management API with autonomous agent capabilities.

## Overview

AiTaskApi is a backend system that combines traditional CRUD task management with an AI-driven autonomous agent. The system integrates with Ollama (local LLM server) to automatically parse natural language input, generate execution plans, and perform actions such as creating, reading, or deleting tasks.

The architecture consists of three main projects:

| Project | Description |
|---------|-------------|
| [`AiTaskApi`](AiTaskApi/) | REST API server (ASP.NET Core 10) |
| [`AiTaskApi.Shared`](AiTaskApi.Shared/) | Shared DTOs, models, and helpers |
| [`AiTaskApi.Worker`](AiTaskApi.Worker/) | Background worker service for async agent execution |
| [`AiTaskApi.Tests`](AiTaskApi.Tests/) | Integration and unit tests |

## Key Features

- **JWT Authentication** - Register and login endpoints with Bearer token auth
- **Task CRUD** - Full create, read, update, delete operations with pagination and filtering
- **AI Chat** - Simple chat endpoint connected to Ollama LLM
- **AI Task Creation** - Natural language to structured task conversion via LLM
- **Autonomous Agent** - Planner-based agent that breaks user requests into executable steps
- **Async Agent Jobs** - Background worker processes agent jobs with retry logic
- **Dream Mode** - Idle-time productivity optimization cycle
- **Swagger UI** - Auto-generated API documentation (development mode)
- **PostgreSQL** - Primary database via Entity Framework Core
- **Docker Support** - Containerized deployment for API and Worker

## Project Structure

```
.
├── AiTaskApi/                      # Main REST API
│   ├── Controllers/                # API endpoints
│   ├── Data/                       # EF Core DbContext
│   ├── DTOs/                       # Request DTOs
│   ├── Models/                     # Domain models
│   ├── Services/                   # Business logic
│   ├── Common/                     # Shared utilities
│   ├── Migrations/                 # EF Core migrations
│   └── Properties/                 # Launch settings
├── AiTaskApi.Shared/               # Shared library
│   ├── DTOs/                       # Transfer objects
│   ├── Models/                     # Domain models (TaskItem, AgentJob, etc.)
│   └── Helper/                     # Helpers (AgentType, ServerAddress)
├── AiTaskApi.Worker/               # Background worker
│   ├── Data/                       # Worker-specific DbContext
│   └── Properties/                 # Launch settings
├── AiTaskApi.Tests/                # Test project
└── docs/                           # Documentation
```

## Quick Start

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- [PostgreSQL](https://www.postgresql.org/) database
- [Ollama](https://ollama.ai/) (optional, for AI features)
- [Docker](https://www.docker.com/) (optional, for containerized deployment)

### Configuration

1. Edit [`AiTaskApi/appsettings.Development.json`](AiTaskApi/appsettings.Development.json) or set environment variables:
    ```json
    {
      "ConnectionStrings": {
        "Default": "Host=localhost;Database=aitaskapi;Username=postgres;Password=yourpassword"
      }
    }
    ```

2. Set the JWT secret (required):
    ```bash
    # PowerShell
    $env:JWT_SECRET="your-super-secret-jwt-key-minimum-32-chars-long"

    # Bash
    export JWT_SECRET="your-super-secret-jwt-key-minimum-32-chars-long"
    ```

3. Optional environment variables:
    - `LLM_SERVER` - Ollama server URL (default: `http://localhost:11434`)
    - `OLLAMA_MODEL` - LLM model name (default: `gemma4:e4b`)
    - `ConnectionStrings__Default` - Database connection string override

### Run the API

```bash
cd AiTaskApi
dotnet restore
dotnet run
```

API is available at `https://localhost:5001` (or configured port).
Swagger UI at `https://localhost:5001/swagger`.

### Run the Worker

```bash
cd AiTaskApi.Worker
dotnet restore
dotnet run
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and receive JWT token |
| GET | `/api/tasks` | Get all tasks (paginated, filterable) |
| GET | `/api/tasks/{id}` | Get a specific task |
| POST | `/api/tasks` | Create a new task |
| PUT | `/api/tasks/{id}` | Update a task |
| DELETE | `/api/tasks/{id}` | Delete a task |
| POST | `/api/ai/chat` | Simple AI chat |
| POST | `/api/ai/create-task` | AI-powered task creation (async) |
| POST | `/api/ai/agent` | Synchronous agent execution |
| POST | `/api/ai/agent-loop` | Async agent loop |
| GET | `/api/ai/jobs` | Get all agent jobs |
| GET | `/api/ai/jobs/{id}` | Get a specific agent job |
| POST | `/api/ai/jobs/{id}/retry` | Retry a failed job |
| DELETE | `/api/ai/jobs/{id}` | Delete a job |
| DELETE | `/api/ai/jobs` | Delete all jobs |

## License

This project is licensed under the [MIT License](../../LICENSE).
