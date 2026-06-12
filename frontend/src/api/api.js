export const BASE_URL = import.meta.env.VITE_API_URL || "/api";

// ----------------------
// Safe fetch helper
// ----------------------
async function safeFetch(url, options = {}) {
  try {
    const res = await fetch(url, options);

    if (!res.ok) {
      console.error("API ERROR:", url, res.status);
      return null;
    }

    const text = await res.text();
    return text ? JSON.parse(text) : true;

  } catch (err) {
    console.error("NETWORK ERROR:", err);
    return null;
  }
}

// ----------------------
// AI Agent
// ----------------------
export function sendPrompt(message) {
  return safeFetch(`${BASE_URL}/ai/agent-loop`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ message }),
  });
}


// ----------------------
// AI Create Task 
// ----------------------
export function sendSimplePrompt(message) {
  return safeFetch(`${BASE_URL}/ai/create-task`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ message }),
  });
}

// ----------------------
// Create task
// ----------------------
export function createTask(task) {
  return safeFetch(`${BASE_URL}/tasks`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify(task),
  });
}

// ----------------------
// Update task (FIXED + SAFE)
// ----------------------
export function updateTask(id, task) {
  if (!id) {
    console.error("updateTask missing id:", task);
    return Promise.resolve(null);
  }

  return safeFetch(`${BASE_URL}/tasks/${id}`, {
    method: "PUT",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({
      title: task.title,
      description: task.description ?? "",
      priority: task.priority ?? "Medium",
      status: task.status ?? "Todo",

      // 🔥 CRITICAL FIX: always send ISO format
      dueDate: task.dueDate
        ? new Date(task.dueDate).toISOString()
        : null
    }),
  });
}

// ----------------------
// Delete task
// ----------------------
export function deleteTask(id) {
  if (!id) {
    console.error("deleteTask missing id");
    return Promise.resolve(null);
  }

  return safeFetch(`${BASE_URL}/tasks/${id}`, {
    method: "DELETE",
  });
}

// ----------------------
// Get all tasks
// ----------------------
export async function getTasks() {
  const data = await safeFetch(`${BASE_URL}/tasks?page=1&pageSize=1000`);

  return data?.data?.items || [];
}

// ----------------------
// Chat
// ----------------------
export function sendChatMessage(message) {
  return safeFetch(`${BASE_URL}/ai/chat`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ message }),
  });
}
