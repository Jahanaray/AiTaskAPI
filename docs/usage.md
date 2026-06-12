# Usage

## Quick Start Workflow

This guide walks through the most common workflow: register, login, create tasks, and use AI features.

### 1. Register a User

```bash
curl -X POST http://localhost:5058/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"username":"john","password":"secret123"}'
```

**Response:**
```
Registered
```

### 2. Login and Get JWT Token

```bash
curl -X POST http://localhost:5058/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"john","password":"secret123"}'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

### 3. Create a Task (Manual)

```bash
curl -X POST http://localhost:5058/api/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{
    "title": "Buy groceries",
    "description": "Milk, eggs, bread",
    "priority": "High",
    "status": "Todo",
    "dueDate": "2026-07-01"
  }'
```

**Response:**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Buy groceries",
    "description": "Milk, eggs, bread",
    "priority": "High",
    "status": "Todo",
    "dueDate": "2026-07-01T00:00:00"
  },
  "message": "Task created"
}
```

### 4. List All Tasks

```bash
curl http://localhost:5058/api/tasks \
  -H "Authorization: Bearer <TOKEN>"
```

**Response:**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "title": "Buy groceries",
        "description": "Milk, eggs, bread",
        "priority": "High",
        "status": "Todo",
        "dueDate": "2026-07-01T00:00:00"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 10
  },
  "message": "Success"
}
```

### 5. Filter Tasks

```bash
# Filter by title
curl "http://localhost:5058/api/tasks?title=groceries" \
  -H "Authorization: Bearer <TOKEN>"

# Filter by completion status
curl "http://localhost:5058/api/tasks?isDone=false" \
  -H "Authorization: Bearer <TOKEN>"

# Paginated results
curl "http://localhost:5058/api/tasks?page=1&pageSize=5" \
  -H "Authorization: Bearer <TOKEN>"
```

### 6. Update a Task

```bash
curl -X PUT http://localhost:5058/api/tasks/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{
    "title": "Buy groceries (updated)",
    "description": "Milk, eggs, bread, cheese",
    "priority": "Medium",
    "status": "InProgress",
    "dueDate": "2026-07-02"
  }'
```

### 7. Delete a Task

```bash
curl -X DELETE http://localhost:5058/api/tasks/1 \
  -H "Authorization: Bearer <TOKEN>"
```

## AI Features

### Simple Chat

Send a message to the LLM and receive a response:

```bash
curl -X POST http://localhost:5058/api/ai/chat \
  -H "Content-Type: application/json" \
  -d '{"message":"Hello, what can you do?"}'
```

**Response:**
```json
{
  "response": "I can help you manage tasks..."
}
```

### AI-Powered Task Creation (Async)

Send a natural language request. The system creates an agent job and returns immediately:

```bash
curl -X POST http://localhost:5058/api/ai/create-task \
  -H "Content-Type: application/json" \
  -d '{"message":"Create a task to review pull requests by Friday"}'
```

**Response (202 Accepted):**
```json
{
  "id": 1,
  "status": "Pending",
  "maxAttempts": 4
}
```

Check job status:
```bash
curl http://localhost:5058/api/ai/jobs/1
```

### Synchronous Agent Execution

Send a request and wait for the full agent pipeline to complete:

```bash
curl -X POST http://localhost:5058/api/ai/agent \
  -H "Content-Type: application/json" \
  -d '{"message":"Create tasks for the weekly sprint planning"}'
```

**Response:**
```json
{
  "steps": 3,
  "results": [
    { "id": 2, "title": "Prepare sprint backlog", ... },
    { "id": 3, "title": "Schedule sprint review", ... },
    { "id": 4, "title": "Update project board", ... }
  ]
}
```

### Agent Loop (Async with Retry)

Similar to `create-task` but uses the full agent loop with retry logic:

```bash
curl -X POST http://localhost:5058/api/ai/agent-loop \
  -H "Content-Type: application/json" \
  -d '{"message":"Set up a project management workflow"}'
```

### Manage Agent Jobs

**List all jobs:**
```bash
curl http://localhost:5058/api/ai/jobs
```

**Get a specific job:**
```bash
curl http://localhost:5058/api/ai/jobs/1
```

**Retry a failed job:**
```bash
curl -X POST http://localhost:5058/api/ai/jobs/1/retry
```

**Delete a job:**
```bash
curl -X DELETE http://localhost:5058/api/ai/jobs/1
```

**Delete all jobs:**
```bash
curl -X DELETE http://localhost:5058/api/ai/jobs
```

## Task Model Reference

### TaskItem Fields

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `id` | int | Auto | - | Unique identifier |
| `title` | string | Yes | `""` | Task title |
| `description` | string? | No | `null` | Task description |
| `priority` | string | No | `"Medium"` | Priority level (`Low`, `Medium`, `High`) |
| `status` | string | No | `"Todo"` | Status (`Todo`, `InProgress`, `Done`, etc.) |
| `dueDate` | DateTime? | No | `null` | Due date (UTC) |
| `isDone` | bool | No | `false` | Completion flag |

### AgentJob Fields

| Field | Type | Description |
|-------|------|-------------|
| `id` | int | Unique identifier |
| `prompt` | string | User input text |
| `status` | string | Job status (`Pending`, `Running`, `Done`, `Failed`) |
| `result` | string? | Serialized result JSON |
| `createdAt` | DateTime | Creation timestamp |
| `completedAt` | DateTime? | Completion timestamp |
| `type` | AgentType | Agent type (`Planner`, `Executor`, `Critic`, `Dreamer`) |
| `context` | string? | Execution context |
| `attemptCount` | int | Number of attempts made |
| `maxAttempts` | int | Maximum retry attempts (default: 4) |
| `nextAttemptAt` | DateTime? | Scheduled next attempt time |
| `lastError` | string? | Last error message |

## Common Workflows

### Workflow 1: Daily Task Management

```bash
# Morning: List pending tasks
curl http://localhost:5058/api/tasks?isDone=false \
  -H "Authorization: Bearer <TOKEN>"

# Complete a task (update status)
curl -X PUT http://localhost:5058/api/tasks/1 \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <TOKEN>" \
  -d '{"title":"Review docs","description":"","priority":"Medium","status":"Done","dueDate":null}'

# Add new tasks via AI
curl -X POST http://localhost:5058/api/ai/create-task \
  -H "Content-Type: application/json" \
  -d '{"message":"Prepare presentation for Monday meeting"}'
```

### Workflow 2: Bulk Task Creation with AI

```bash
# Ask AI to create multiple tasks from a description
curl -X POST http://localhost:5058/api/ai/agent-loop \
  -H "Content-Type: application/json" \
  -d '{"message":"Set up a new software project: initialize repo, add README, configure CI/CD, create issue templates"}'
```

### Workflow 3: Task Audit

```bash
# Get all tasks with high priority
curl "http://localhost:5058/api/tasks?title=&page=1&pageSize=100" \
  -H "Authorization: Bearer <TOKEN>"

# Check agent job history
curl http://localhost:5058/api/ai/jobs
```

## Swagger UI

When running in development mode, access the interactive API documentation:

```
https://localhost:5001/swagger
```

Features:
- Browse all endpoints
- Test API calls directly from the browser
- View request/response schemas
- Authenticate via "Authorize" button (paste JWT token)

## CLI Examples with PowerShell

```powershell
# Register
Invoke-RestMethod -Uri "http://localhost:5058/api/auth/register" -Method Post -ContentType "application/json" -Body '{"username":"admin","password":"admin123"}'

# Login
$response = Invoke-RestMethod -Uri "http://localhost:5058/api/auth/login" -Method Post -ContentType "application/json" -Body '{"username":"admin","password":"admin123"}'
$token = $response.token

# Create task
Invoke-RestMethod -Uri "http://localhost:5058/api/tasks" -Method Post -ContentType "application/json" -Headers @{Authorization="Bearer $token"} -Body '{"title":"New task","description":"Description"}'

# List tasks
Invoke-RestMethod -Uri "http://localhost:5058/api/tasks" -Headers @{Authorization="Bearer $token"}
```
