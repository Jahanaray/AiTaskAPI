# Configuration

## Overview

AiTaskApi uses the standard .NET configuration system with layered priority:

1. **Environment variables** (highest priority)
2. **appsettings.{Environment}.json** files
3. **Hardcoded defaults** in source code (lowest priority)

## Configuration Files

### AiTaskApi/appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "Default": ""
  }
}
```

| Key | Description | Default |
|-----|-------------|---------|
| `Logging:LogLevel:Default` | Default log level | `Information` |
| `Logging:LogLevel:Microsoft.AspNetCore` | ASP.NET Core log level | `Warning` |
| `AllowedHosts` | Host header filter | `*` (all) |
| `ConnectionStrings:Default` | PostgreSQL connection string | Empty (required) |

### AiTaskApi/Properties/launchSettings.json

Contains Kestrel/IIS Express settings including HTTPS port, environment variables for development.

## Environment Variables

### Required

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__Default` | PostgreSQL connection string (overrides appsettings) | `Host=localhost;Database=aitaskapi;Username=postgres;Password=pass` |

### AI Features

| Variable | Description | Default |
|----------|-------------|---------|
| `LLM_SERVER` | Ollama server URL | `http://host.docker.internal:11434` (Docker) or `http://localhost:11434` (local) |
| `OLLAMA_MODEL` | LLM model name | `gemma4:e4b` |

### JWT Configuration

The JWT signing key is configured via environment variable or configuration:

| Source | Priority |
|--------|----------|
| `JWT_SECRET` environment variable | Highest |
| `Jwt:Secret` configuration | Medium |

```csharp
var secretKey = Environment.GetEnvironmentVariable("JWT_SECRET")
    ?? builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException(
        "JWT signing key is not configured. Set the JWT_SECRET environment variable or Jwt:Secret configuration value.");
```

See [`backend/.env.example`](../backend/.env.example) for configuration guidance.

## CORS Configuration

CORS is configured in [`Program.cs`](../AiTaskApi/Program.cs:52-64):

```csharp
options.AddPolicy("AllowFrontend", policy =>
{
    policy
        .WithOrigins(
            "http://localhost:5173", // Vite dev
            "http://localhost:3000"  // optional React dev
        )
        .AllowAnyHeader()
        .AllowAnyMethod();
});
```

To add production origins, modify the `WithOrigins` call.

## HTTP Client Configuration

The AI service HTTP client is configured in [`Program.cs`](../AiTaskApi/Program.cs:43-46):

```csharp
builder.Services.AddHttpClient<AiService>(client =>
{
    client.Timeout = TimeSpan.FromMinutes(10);
});
```

| Setting | Value | Description |
|---------|-------|-------------|
| Timeout | 10 minutes | Maximum time for LLM responses |

## Worker Configuration

### Retry Settings

Defined in [`Worker.cs`](../AiTaskApi.Worker/Worker.cs:10):

```csharp
private const int RetryDelaySeconds = 10;
```

Retry delay uses exponential backoff: `RetryDelaySeconds * AttemptCount`.

### Polling Interval

The worker polls for pending jobs every **3 seconds** ([`Worker.cs`](../AiTaskApi.Worker/Worker.cs:55)):

```csharp
await Task.Delay(3000, stoppingToken);
```

### Dream Mode

Dream mode activates when:
1. No pending agent jobs exist
2. Ollama is idle (no loaded models)

The idle check queries Ollama's `/api/ps` endpoint ([`Worker.cs`](../AiTaskApi.Worker/Worker.cs:137-152)).

## Docker Configuration

### AiTaskApi/Dockerfile

Multi-stage build targeting .NET 10 runtime.

### AiTaskApi.Worker/Dockerfile

Multi-stage build targeting .NET 10 worker runtime.

### Docker Environment Variables

When running in Docker, use double-underscore (`__`) for connection strings:

```bash
-e ConnectionStrings__Default="Host=host.docker.internal;Database=aitaskapi;Username=postgres;Password=yourpassword"
```

## Configuration Examples

### Development (Local)

```json
// appsettings.Development.json
{
  "ConnectionStrings": {
    "Default": "Host=localhost;Database=aitaskapi_dev;Username=postgres;Password=devpass"
  }
}
```

### Production

```bash
export ConnectionStrings__Default="Host=prod-db.internal;Database=aitaskapi;Username=appuser;Password=securepass"
export LLM_SERVER="http://ollama.internal:11434"
export OLLAMA_MODEL="llama3"
```

### Testing

```bash
# Use in-memory database for tests (see AiTaskApi.Tests)
# No appsettings changes needed - tests use FakeAiService
```

## Security Notes

**Assumptions documented during analysis:**

1. **JWT Secret:** The signing key is hardcoded. This is a security risk and should be externalized.
2. **CORS:** Only localhost origins are allowed by default. Update for production.
3. **HTTPS:** HTTPS redirection is enabled. Ensure valid certificates in production.
4. **BCrypt:** Password hashing uses BCrypt.Net-Next with default cost factor (industry standard).
