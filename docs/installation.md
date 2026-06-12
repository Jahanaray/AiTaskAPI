# Installation

## Prerequisites

### Required

| Software | Version | Purpose |
|----------|---------|---------|
| [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) | 10.0+ | Framework for building and running the application |
| PostgreSQL | 14+ | Primary database |

### Optional (for AI features)

| Software | Version | Purpose |
|----------|---------|---------|
| [Ollama](https://ollama.ai/) | Latest | Local LLM server |
| Docker / Docker Compose | Latest | Containerized deployment |

## Step 1: Clone the Repository

```bash
git clone <repository-url>
cd backend
```

## Step 2: Restore NuGet Packages

```bash
dotnet restore AiTaskApi/AiTaskApi.csproj
dotnet restore AiTaskApi.Shared/AiTaskApi.Shared.csproj
dotnet restore AiTaskApi.Worker/AiTaskApi.Worker.csproj
dotnet restore AiTaskApi.Tests/AiTaskApi.Tests.csproj
```

## Step 3: Configure the Database

### Option A: Local PostgreSQL

1. Install and start PostgreSQL.

2. Create a database:
   ```sql
   CREATE DATABASE aitaskapi;
   ```

3. Update [`AiTaskApi/appsettings.json`](../AiTaskApi/appsettings.json):
   ```json
   {
     "ConnectionStrings": {
       "Default": "Host=localhost;Database=aitaskapi;Username=postgres;Password=yourpassword"
     }
   }
   ```

### Option B: Docker PostgreSQL

```bash
docker run --name aitaskapi-db -e POSTGRES_PASSWORD=yourpassword -e POSTGRES_DB=aitaskapi -p 5432:5432 -d postgres:16
```

Then update the connection string in `appsettings.json` as shown above.

## Step 4: Apply Database Migrations

The application auto-migrates on startup via [`Program.cs`](../AiTaskApi/Program.cs:142-146). However, you can also apply migrations manually:

```bash
cd AiTaskApi
dotnet ef database update
```

### Available Migrations

| Migration | Description | File |
|-----------|-------------|------|
| `Init` | Initial schema | [`20260419162350_Init.cs`](../AiTaskApi/Migrations/20260419162350_Init.cs) |
| `AddUsers` | User table | [`20260420212047_AddUsers.cs`](AiTaskApi/Migrations/20260420212047_AddUsers.cs) |
| `AddTaskFields` | Task model fields | [`20260421040458_AddTaskFields.cs`](AiTaskApi/Migrations/20260421040458_AddTaskFields.cs) |
| `UpdateTasks` | Task updates | [`20260505015619_UpdateTasks.cs`](AiTaskApi/Migrations/20260505015619_UpdateTasks.cs) |
| `AddAgentJobs` | AgentJob table | [`20260505132751_AddAgentJobs.cs`](AiTaskApi/Migrations/20260505132751_AddAgentJobs.cs) |
| `SyncModel` | Model sync | [`20260505233940_SyncModel.cs`](AiTaskApi/Migrations/20260505233940_SyncModel.cs) |
| `UpdateModel` | Model update | [`20260531112248_UpdateModel.cs`](AiTaskApi/Migrations/20260531112248_UpdateModel.cs) |
| `AddAgentJobRetries` | Retry fields on AgentJob | [`20260609000000_AddAgentJobRetries.cs`](AiTaskApi/Migrations/20260609000000_AddAgentJobRetries.cs) |

## Step 5: Configure Ollama (Optional)

### Local Ollama

1. Install [Ollama](https://ollama.ai/).

2. Pull a model:
   ```bash
   ollama pull gemma4:e4b
   ```

3. Ensure Ollama is running on the default port `11434`.

### Remote Ollama

Set the `LLM_SERVER` environment variable:

```bash
# Windows PowerShell
$env:LLM_SERVER="http://your-ollama-server:11434"

# Linux/macOS
export LLM_SERVER="http://your-ollama-server:11434"
```

### Custom Model

Set the `OLLAMA_MODEL` environment variable:

```bash
$env:OLLAMA_MODEL="llama3"
```

## Step 6: Run the Application

### Development (API + Worker)

**Terminal 1 - API:**
```bash
cd AiTaskApi
dotnet run
```

**Terminal 2 - Worker:**
```bash
cd AiTaskApi.Worker
dotnet run
```

### Docker Deployment

**API Container:**
```bash
cd AiTaskApi
docker build -t aitaskapi .
docker run -p 5000:8080 \
  -e ConnectionStrings__Default="Host=db;Database=aitaskapi;Username=postgres;Password=yourpassword" \
  -e LLM_SERVER="http://host.docker.internal:11434" \
  aitaskapi
```

**Worker Container:**
```bash
cd AiTaskApi.Worker
docker build -t aitaskapi-worker .
docker run \
  -e ConnectionStrings__Default="Host=db;Database=aitaskapi;Username=postgres;Password=yourpassword" \
  -e LLM_SERVER="http://host.docker.internal:11434" \
  aitaskapi-worker
```

## Step 7: Verify Installation

1. Open Swagger UI: `https://localhost:7280/swagger` (HTTPS) or `http://localhost:5058/swagger` (HTTP)

2. Register a test user:
   ```bash
   curl -X POST https://localhost:7280/api/auth/register \
     -H "Content-Type: application/json" \
     -d '{"username":"testuser","password":"testpass"}'
   ```

3. Login and get a token:
   ```bash
   curl -X POST https://localhost:7280/api/auth/login \
     -H "Content-Type: application/json" \
     -d '{"username":"testuser","password":"testpass"}'
   ```

4. Create a task (replace `<TOKEN>` with your JWT):
   ```bash
   curl -X POST https://localhost:7280/api/tasks \
     -H "Content-Type: application/json" \
     -H "Authorization: Bearer <TOKEN>" \
     -d '{"title":"Test Task","description":"Verification task"}'
   ```

## Troubleshooting Installation

| Issue | Solution |
|-------|----------|
| `Connection refused` to database | Verify PostgreSQL is running and connection string is correct |
| `Port already in use` | Change port in `Properties/launchSettings.json` or stop conflicting service |
| AI features not working | Verify Ollama is running on the configured URL |
| Migration errors | Run `dotnet ef database drop` (careful!) then `dotnet ef database update` |
| HTTPS certificate errors | Run `dotnet dev-certs https --trust` |
