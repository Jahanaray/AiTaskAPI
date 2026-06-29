# Troubleshooting

## Common Issues

### Database Connection Failures

**Symptom:** Application fails to start with database connection errors.

**Solutions:**

| Issue | Check | Fix |
|-------|-------|-----|
| PostgreSQL not running | `pg_isready -h localhost -p 5432` | Start PostgreSQL service |
| Wrong connection string | Verify in [`appsettings.json`](../AiTaskApi/appsettings.json) | Update `ConnectionStrings:Default` |
| Database doesn't exist | Check PostgreSQL databases | `CREATE DATABASE aitaskapi;` |
| Wrong credentials | Verify username/password | Update connection string or reset PostgreSQL password |
| Docker networking | Container can't reach host DB | Use `host.docker.internal` instead of `localhost` |
| Password authentication failed for user `postgres` | Existing Docker Postgres volume was initialized with a different password | Keep `DB_PASSWORD` set to the original password, or run `docker compose down -v` to recreate the local database volume |

**Debug command:**
```bash
dotnet run --verbosity detailed 2>&1 | Select-String "Exception"
```

### AI Features Not Working

**Symptom:** Chat and agent endpoints return errors or empty responses.

**Solutions:**

| Issue | Check | Fix |
|-------|-------|-----|
| Ollama not running | `curl http://localhost:11434/api/tags` | Start Ollama service |
| Wrong LLM server URL | Check `LLM_SERVER` env var | Set correct URL via environment variable |
| Model not pulled | Check model exists in Ollama | `ollama pull gemma4:e4b` |
| Network firewall | Verify port 11434 is accessible | Open firewall or update URL |
| Wrong model name | Check `OLLAMA_MODEL` env var | Set valid model name |

**Test Ollama connectivity:**
```bash
curl -X POST http://localhost:11434/api/generate \
  -H "Content-Type: application/json" \
  -d '{"model":"gemma4:e4b","prompt":"Hello","stream":false}'
```

### JWT Authentication Issues

**Symptom:** All protected endpoints return 401 Unauthorized.

**Solutions:**

| Issue | Check | Fix |
|-------|-------|-----|
| Missing token | Verify `Authorization` header | Include `Bearer <token>` |
| Expired token | Tokens expire after 2 hours ([`AuthController.cs`](../AiTaskApi/Controllers/AuthController.cs:58)) | Re-login to get new token |
| Malformed token | Check token format | Ensure no extra spaces or quotes |
| Wrong scheme | Verify `Bearer` (not `Token`) | Use `Authorization: Bearer <token>` |

**Debug - Get current token:**
```bash
# Login and extract token
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"test","password":"test"}'
```

### CORS Errors

**Symptom:** Browser console shows CORS policy errors.

**Solutions:**

| Issue | Check | Fix |
|-------|-------|-----|
| Frontend origin not allowed | Check [`Program.cs`](../AiTaskApi/Program.cs:57-59) | Add origin to `WithOrigins()` |
| Development vs production | Different origins for dev/prod | Configure separate CORS policies |

**Add new CORS origin:**
```csharp
// In Program.cs
policy.WithOrigins(
    "http://localhost:5173",
    "http://localhost:3000",
    "https://your-production-frontend.com"  // Add this
)
```

### Port Already in Use

**Symptom:** Application fails to start with port binding errors.

**Solutions:**

| Platform | Command | Purpose |
|----------|---------|---------|
| Windows PowerShell | `netstat -ano \| findstr :5001` | Find process using the port |
| Windows PowerShell | `Stop-Process -Id <PID> -Force` | Kill the conflicting process |
| All | Edit [`launchSettings.json`](../AiTaskApi/Properties/launchSettings.json) | Change `applicationUrl` port |

### Migration Errors

**Symptom:** Database migration fails on startup.

**Solutions:**

| Issue | Fix |
|-------|-----|
| Outdated migrations | Run `dotnet ef migrations add FixName` then `dotnet ef database update` |
| Corrupted snapshot | Delete [`AppDbContextModelSnapshot.cs`](../AiTaskApi/Migrations/AppDbContextModelSnapshot.cs) and regenerate |
| Schema mismatch | Drop and recreate database, then run migrations fresh |

**Rebuild migrations:**
```bash
cd AiTaskApi
dotnet ef migrations remove
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Worker Not Processing Jobs

**Symptom:** Agent jobs remain in `Pending` status indefinitely.

**Solutions:**

| Issue | Check | Fix |
|-------|-------|-----|
| Worker not running | Verify worker process | Start worker: `cd AiTaskApi.Worker && dotnet run` |
| NextAttemptAt in future | Check job's `NextAttemptAt` field | Wait until scheduled time or retry the job |
| Database connection | Worker needs same DB as API | Verify worker connection string |
| Ollama unavailable | Worker checks Ollama for dream mode | Ensure Ollama is running |

**Debug worker logs:**
```bash
cd AiTaskApi.Worker
dotnet run --verbosity detailed
```

### Docker Issues

**Symptom:** Container fails to start or connect to services.

| Issue | Fix |
|-------|-----|
| Connection string not passed | Use `-e ConnectionStrings__Default="..."` |
| Can't reach host DB | Use `host.docker.internal` in connection string |
| Can't reach Ollama | Use `host.docker.internal:11434` for LLM_SERVER |
| Volume permissions | Ensure Docker has file access to config files |

**Docker debug:**
```bash
docker logs <container-name>
docker exec -it <container-name> dotnet ef database update
```

### High Memory Usage

**Symptom:** Application consumes excessive memory.

**Solutions:**

| Cause | Fix |
|-------|-----|
| Ollama model too large | Use smaller model (e.g., `gemma4:e4b` instead of larger variants) |
| Long keep_alive | AiService uses `keep_alive: "30m"` ([`AiService.cs`](../AiTaskApi/Services/AiService.cs:44)) | Reduce in source if needed |
| No connection pooling | Verify PostgreSQL connection pool settings | Add `Max Pool Size=100` to connection string |

### Slow API Responses

**Symptom:** Endpoints take unusually long to respond.

**Solutions:**

| Cause | Fix |
|-------|-----|
| AI endpoints blocking | Use async endpoints (`agent-loop`) instead of sync (`agent`) |
| Large result sets | Use pagination parameters on task list endpoint |
| No database indexes | Add indexes for frequently queried columns |
| HTTP client timeout | Default is 10 minutes ([`Program.cs`](../AiTaskApi/Program.cs:45)) |

### HTTPS Certificate Errors

**Symptom:** Browser shows certificate warnings or curl fails with SSL errors.

**Solutions:**

```bash
# Trust development certificates
dotnet dev-certs https --trust

# Generate new certificates
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

## Log Levels

Adjust logging in [`appsettings.json`](../AiTaskApi/appsettings.json):

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",           // More verbose
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Information"
    }
  }
}
```

| Level | Use Case |
|-------|----------|
| `Trace` | Maximum verbosity (development only) |
| `Debug` | Detailed debugging information |
| `Information` | Normal operational events (default) |
| `Warning` | Potentially harmful situations (default for Microsoft.*) |
| `Error` | Error events only |
| `Critical` | Critical failures only |
| `None` | Disable logging |

## Health Check Endpoints

While no explicit health check endpoint is configured, you can verify system health:

| Check | Method | Endpoint |
|-------|--------|----------|
| API alive | GET | `/weatherforecast` (or any public endpoint) |
| Database | N/A | Application startup will fail if DB unreachable |
| Ollama | GET | `http://<LLM_SERVER>/api/ps` |
| Worker alive | N/A | Check worker process is running |

## Diagnostic Commands

```bash
# Check database connectivity
dotnet tool install -g dotnet-ef
dotnet ef dbcontext info --project AiTaskApi

# List all migrations
dotnet ef migrations list --project AiTaskApi

# Test Ollama model
curl -X POST http://localhost:11434/api/generate \
  -d '{"model":"gemma4:e4b","prompt":"Test","stream":false}'

# Check running processes
Get-Process -Name "dotnet"    # Windows PowerShell
ps aux \| grep dotnet          # Linux/macOS

# View application logs
Get-Content AiTaskApi/logs/*.log -Tail 100 -Wait   # If file logging configured
```

## Getting Help

If issues persist:

1. Check application logs for detailed error messages
2. Verify all prerequisites are installed and running
3. Test each component individually (database, Ollama, API, Worker)
4. Review the [FAQ](./faq.md) for common questions
5. Check Swagger UI at `/swagger` for API documentation
