# API Reference

## Base URL

```
http://localhost:5058
https://localhost:7280  (HTTPS)
```

## Authentication

Most endpoints require JWT Bearer token authentication. Include the token in the `Authorization` header:

```
Authorization: Bearer <your-jwt-token>
```

Tokens expire after **2 hours** ([`AuthController.cs`](../AiTaskApi/Controllers/AuthController.cs:58)).

## Response Format

All responses use a standard envelope format defined in [`ApiResponse<T>`](../AiTaskApi/Models/ApiResponse.cs):

```json
{
  "success": true,
  "data": { ... },
  "message": "Success"
}
```

### Success Response

| Field | Type | Description |
|-------|------|-------------|
| `success` | bool | Always `true` |
| `data` | T | Response payload |
| `message` | string? | Status message |

### Error Response

| Field | Type | Description |
|-------|------|-------------|
| `success` | bool | Always `false` |
| `data` | T? | `null` or default |
| `message` | string | Error description |

## Pagination

List endpoints return [`PagedResult<T>`](../AiTaskApi/Models/PagedResult.cs):

```json
{
  "success": true,
  "data": {
    "items": [ ... ],
    "totalCount": 42,
    "page": 1,
    "pageSize": 10
  },
  "message": "Success"
}
```

| Field | Type | Description |
|-------|------|-------------|
| `items` | T[] | List of results |
| `totalCount` | int | Total matching records |
| `page` | int | Current page number |
| `pageSize` | int | Items per page |

---

## Auth Endpoints

### Register

**POST** `/api/auth/register`

Register a new user account.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `username` | string | Yes | Desired username |
| `password` | string | Yes | Account password |

**Request:**
```json
{
  "username": "newuser",
  "password": "securepass123"
}
```

**Response (200):**
```
Registered
```

**Response (400) - Username exists:**
```
User already exists
```

### Login

**POST** `/api/auth/login`

Authenticate and receive a JWT token.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `username` | string | Yes | Account username |
| `password` | string | Yes | Account password |

**Request:**
```json
{
  "username": "newuser",
  "password": "securepass123"
}
```

**Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
}
```

**Response (401) - Invalid credentials:**
```
Invalid credentials
```

---

## Task Endpoints

### Get All Tasks

**GET** `/api/tasks`

Retrieve paginated list of tasks with optional filtering.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| `isDone` | bool? | null | Filter by completion status |
| `title` | string? | null | Filter by title (contains) |
| `page` | int | 1 | Page number |
| `pageSize` | int | 10 | Items per page |

**Request:**
```
GET /api/tasks?isDone=false&title=urgent&page=1&pageSize=20
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "items": [
      {
        "id": 1,
        "title": "Urgent task",
        "description": "Fix critical bug",
        "priority": "High",
        "status": "Todo",
        "dueDate": "2026-07-01T00:00:00Z"
      }
    ],
    "totalCount": 1,
    "page": 1,
    "pageSize": 20
  },
  "message": "Success"
}
```

### Get Task by ID

**GET** `/api/tasks/{id}`

Retrieve a single task by its ID.

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | int | Task ID |

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": 1,
    "title": "Task title",
    "description": "Task description",
    "priority": "Medium",
    "status": "Todo",
    "dueDate": null
  },
  "message": "Success"
}
```

**Response (404):**
```json
{
  "success": false,
  "data": null,
  "message": "Task not found"
}
```

### Create Task

**POST** `/api/tasks`

Create a new task.

| Field | Type | Required | Default | Description |
|-------|------|----------|---------|-------------|
| `title` | string | Yes | - | Task title |
| `description` | string? | No | null | Task description |
| `priority` | string | No | `"Medium"` | Priority (`Low`, `Medium`, `High`) |
| `status` | string | No | `"Todo"` | Initial status |
| `dueDate` | DateTime? | No | null | Due date (ISO 8601) |

**Request:**
```json
{
  "title": "New task",
  "description": "Task details here",
  "priority": "High",
  "status": "Todo",
  "dueDate": "2026-07-15T00:00:00Z"
}
```

**Response (200):**
```json
{
  "success": true,
  "data": {
    "id": 2,
    "title": "New task",
    "description": "Task details here",
    "priority": "High",
    "status": "Todo",
    "dueDate": "2026-07-15T00:00:00Z"
  },
  "message": "Task created"
}
```

### Update Task

**PUT** `/api/tasks/{id}`

Update an existing task. All fields are optional for partial updates.

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | int | Task ID to update |

**Request:**
```json
{
  "title": "Updated title",
  "description": "Updated description",
  "priority": "Low",
  "status": "Done",
  "dueDate": null
}
```

**Response (200):**
```json
{
  "success": true,
  "data": true,
  "message": "Task updated"
}
```

**Response (404):**
```json
{
  "success": false,
  "data": null,
  "message": "Task not found"
}
```

### Delete Task

**DELETE** `/api/tasks/{id}`

Delete a task permanently.

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | int | Task ID to delete |

**Response (200):**
```json
{
  "success": true,
  "data": true,
  "message": "Task deleted"
}
```

**Response (404):**
```json
{
  "success": false,
  "data": null,
  "message": "Task not found"
}
```

---

## AI Endpoints

### Chat

**POST** `/api/ai/chat`

Send a message to the connected LLM and receive a text response. No authentication required.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | Yes | User input message |

**Request:**
```json
{
  "message": "What is the capital of France?"
}
```

**Response (200):**
```json
{
  "response": "The capital of France is Paris."
}
```

### Create Task from AI

**POST** `/api/ai/create-task`

Send a natural language request. The system creates an agent job asynchronously and returns immediately with a job ID.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | Yes | Natural language task description |

**Request:**
```json
{
  "message": "Create a task to review pull requests by Friday"
}
```

**Response (202 Accepted):**
```json
{
  "id": 1,
  "status": "Pending",
  "maxAttempts": 4
}
```

### Agent (Synchronous)

**POST** `/api/ai/agent`

Execute the full agent pipeline synchronously. Waits for all steps to complete before returning. May take several minutes for complex requests.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | Yes | Natural language request |

**Request:**
```json
{
  "message": "Set up a project with README, LICENSE, and issue templates"
}
```

**Response (200):**
```json
{
  "steps": 3,
  "results": [
    { "id": 1, "title": "Create README.md", ... },
    { "id": 2, "title": "Add LICENSE file", ... },
    { "id": 3, "title": "Create issue templates", ... }
  ]
}
```

### Agent Loop (Asynchronous)

**POST** `/api/ai/agent-loop`

Similar to `agent` but creates an async agent job. Returns immediately with a job ID. The background worker processes the job.

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `message` | string | Yes | Natural language request |

**Request:**
```json
{
  "message": "Organize my task list by priority"
}
```

**Response (200):**
```json
{
  "id": 2,
  "status": "Pending"
}
```

### Get All Jobs

**GET** `/api/ai/jobs`

Retrieve all agent jobs, ordered by creation date (newest first).

**Response (200):**
```json
[
  {
    "id": 2,
    "prompt": "Organize my task list",
    "status": "Done",
    "result": "{...}",
    "createdAt": "2026-06-12T10:00:00Z",
    "completedAt": "2026-06-12T10:00:15Z",
    "type": 0,
    "context": null,
    "attemptCount": 1,
    "maxAttempts": 4,
    "nextAttemptAt": null,
    "lastError": null
  },
  {
    "id": 1,
    "prompt": "Create tasks for sprint",
    "status": "Failed",
    "result": "Connection timeout",
    "createdAt": "2026-06-12T09:00:00Z",
    "completedAt": "2026-06-12T09:05:00Z",
    "type": 0,
    "context": null,
    "attemptCount": 4,
    "maxAttempts": 4,
    "nextAttemptAt": null,
    "lastError": "Connection timeout"
  }
]
```

### Get Job by ID

**GET** `/api/ai/jobs/{id}`

Retrieve a specific agent job.

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | int | Job ID |

**Response (200):** Single [`AgentJob`](../AiTaskApi.Shared/Models/Agent/AgentJob.cs) object.

**Response (404):** Not found.

### Retry Job

**POST** `/api/ai/jobs/{id}/retry`

Reset a failed or completed job to `Pending` status for re-execution.

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | int | Job ID to retry |

**Response (200):** Updated [`AgentJob`](../AiTaskApi.Shared/Models/Agent/AgentJob.cs) object with `Status: "Pending"`.

**Response (404):** Not found.

### Delete Job

**DELETE** `/api/ai/jobs/{id}`

Delete a specific agent job.

| Parameter | Type | Description |
|-----------|------|-------------|
| `id` | int | Job ID to delete |

**Response (200):**
```json
{
  "message": "Deleted"
}
```

### Delete All Jobs

**DELETE** `/api/ai/jobs`

Delete all agent jobs. This operation cannot be undone.

**Response (200):**
```json
{
  "message": "All jobs deleted"
}
```

---

## Data Models Reference

### TaskItem

Defined in [`TaskItem.cs`](../AiTaskApi.Shared/Models/TaskItem.cs).

| Field | Type | Description |
|-------|------|-------------|
| `id` | int | Unique identifier |
| `title` | string | Task title |
| `description` | string? | Task description |
| `priority` | string | Priority level |
| `status` | string | Task status |
| `dueDate` | DateTime? | Due date (UTC) |
| `isDone` | bool | Completion flag |

### TaskCreateDto

Defined in [`TaskCreateDto.cs`](../AiTaskApi.Shared/DTOs/TaskCreateDto.cs).

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `title` | string | `""` | Task title |
| `description` | string? | null | Task description |
| `priority` | string | `"Medium"` | Priority level |
| `status` | string | `"Todo"` | Initial status |
| `dueDate` | DateTime? | null | Due date |

### TaskReadDto

Defined in [`TaskReadDto.cs`](../AiTaskApi.Shared/DTOs/TaskReadDto.cs).

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `id` | int | - | Unique identifier |
| `title` | string | `""` | Task title |
| `description` | string? | null | Task description |
| `priority` | string | `"Medium"` | Priority level |
| `status` | string | `"Todo"` | Task status |
| `dueDate` | DateTime? | null | Due date |

### AgentJob

Defined in [`AgentJob.cs`](../AiTaskApi.Shared/Models/Agent/AgentJob.cs).

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `id` | int | - | Unique identifier |
| `prompt` | string | `""` | User input text |
| `status` | string | `"Pending"` | Job status |
| `result` | string? | null | Serialized result JSON |
| `createdAt` | DateTime | - | Creation timestamp |
| `completedAt` | DateTime? | null | Completion timestamp |
| `type` | AgentType | `Planner` | Agent type enum |
| `context` | string? | null | Execution context |
| `attemptCount` | int | 0 | Number of attempts made |
| `maxAttempts` | int | 4 | Maximum retry attempts |
| `nextAttemptAt` | DateTime? | null | Scheduled next attempt time |
| `lastError` | string? | null | Last error message |

### AgentType Enum

Defined in [`AgentType.cs`](../AiTaskApi.Shared/Helper/AgentType.cs).

| Value | Integer | Description |
|-------|---------|-------------|
| `Planner` | 0 | Breaks requests into steps |
| `Executor` | 1 | Executes individual steps |
| `Critic` | 2 | Reviews results (placeholder) |
| `Dreamer` | 3 | Idle-time optimization |

### PagedResult<T>

Defined in [`PagedResult.cs`](../AiTaskApi/Models/PagedResult.cs).

| Field | Type | Description |
|-------|------|-------------|
| `items` | T[] | List of results |
| `totalCount` | int | Total matching records |
| `page` | int | Current page number |
| `pageSize` | int | Items per page |

### ApiResponse<T>

Defined in [`ApiResponse.cs`](../AiTaskApi/Models/ApiResponse.cs).

| Field | Type | Description |
|-------|------|-------------|
| `success` | bool | Whether the operation succeeded |
| `data` | T? | Response payload |
| `message` | string? | Status or error message |

---

## Error Codes

| HTTP Code | Meaning | Common Causes |
|-----------|---------|---------------|
| 200 | OK | Successful request |
| 202 | Accepted | Async job created (agent endpoints) |
| 400 | Bad Request | Invalid input, missing required fields |
| 401 | Unauthorized | Missing or invalid JWT token |
| 404 | Not Found | Resource not found |
| 500 | Internal Server Error | Server-side error |

## Rate Limits

No rate limiting is configured in the default setup. For production deployments, consider adding rate limiting middleware.

## Swagger UI

Interactive API documentation is available at `/swagger` when running in development mode:

```
https://localhost:7280/swagger
http://localhost:5058/swagger
```

## Base URL Configuration

The default development ports are `5058` (HTTP) and `7280` (HTTPS). These can be changed in [`Properties/launchSettings.json`](../backend/AiTaskApi/Properties/launchSettings.json).
