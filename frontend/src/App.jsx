import { useEffect, useMemo, useState } from "react";
import {
  createTask,
  deleteTask,
  getTasks,
  sendChatMessage,
  sendSimplePrompt,
  updateTask,
} from "./api/api";
import "./App.css";

const STATUSES = ["Todo", "Doing", "Done"];
const PRIORITIES = ["Low", "Medium", "High"];

const statusMeta = {
  Todo: { label: "Todo", tone: "neutral" },
  Doing: { label: "Doing", tone: "warning" },
  Done: { label: "Done", tone: "success" },
};

const priorityMeta = {
  Low: { label: "Low", tone: "calm" },
  Medium: { label: "Medium", tone: "info" },
  High: { label: "High", tone: "danger" },
};

const emptyTask = {
  title: "",
  description: "",
  priority: "Medium",
  dueDate: "",
};

function Icon({ name }) {
  const paths = {
    board: "M4 5h7v14H4z M13 5h7v8h-7z M13 15h7v4h-7z",
    chat: "M5 6h14v9H8l-3 3z",
    spark: "M12 3l1.7 5.1L19 10l-5.3 1.9L12 17l-1.7-5.1L5 10l5.3-1.9z",
    plus: "M12 5v14M5 12h14",
    trash: "M6 7h12M10 7V5h4v2M8 9l1 10h6l1-10",
    edit: "M5 17.5V20h2.5L18 9.5 15.5 7z M14.5 8l2.5 2.5",
    send: "M4 12l16-7-7 16-2-7z",
    refresh: "M18 8a6 6 0 1 0 1 4M18 4v4h-4",
    check: "M5 12l4 4L19 6",
  };

  return (
    <svg aria-hidden="true" viewBox="0 0 24 24" className="icon">
      <path d={paths[name]} />
    </svg>
  );
}

function formatDate(value) {
  if (!value) return "No date";
  return new Intl.DateTimeFormat(undefined, {
    month: "short",
    day: "numeric",
    year: "numeric",
  }).format(new Date(value));
}

function App() {
  const [activeView, setActiveView] = useState("tasks");
  const [tasks, setTasks] = useState([]);
  const [taskDraft, setTaskDraft] = useState(emptyTask);
  const [edit, setEdit] = useState(null);
  const [aiCommand, setAiCommand] = useState("");
  const [chatInput, setChatInput] = useState("");
  const [chat, setChat] = useState([]);
  const [statusFilter, setStatusFilter] = useState("All");
  const [priorityFilter, setPriorityFilter] = useState("All");
  const [loadingTasks, setLoadingTasks] = useState(true);
  const [busy, setBusy] = useState(false);
  const [notice, setNotice] = useState("");

  const showNotice = (message) => {
    setNotice(message);
    window.clearTimeout(showNotice.timer);
    showNotice.timer = window.setTimeout(() => setNotice(""), 3200);
  };

  const loadTasks = async () => {
    setLoadingTasks(true);
    const data = await getTasks();
    setTasks(Array.isArray(data) ? data : []);
    setLoadingTasks(false);
  };

  useEffect(() => {
    let ignore = false;

    getTasks().then((data) => {
      if (ignore) return;
      setTasks(Array.isArray(data) ? data : []);
      setLoadingTasks(false);
    });

    return () => {
      ignore = true;
    };
  }, []);

  const metrics = useMemo(() => {
    const byStatus = Object.fromEntries(STATUSES.map((status) => [status, 0]));
    const byPriority = Object.fromEntries(PRIORITIES.map((priority) => [priority, 0]));

    tasks.forEach((task) => {
      byStatus[task.status || "Todo"] = (byStatus[task.status || "Todo"] || 0) + 1;
      byPriority[task.priority || "Medium"] =
        (byPriority[task.priority || "Medium"] || 0) + 1;
    });

    return {
      total: tasks.length,
      done: byStatus.Done,
      active: byStatus.Todo + byStatus.Doing,
      high: byPriority.High,
    };
  }, [tasks]);

  const visibleTasks = useMemo(() => {
    return tasks.filter((task) => {
      const statusMatches =
        statusFilter === "All" || (task.status || "Todo") === statusFilter;
      const priorityMatches =
        priorityFilter === "All" || (task.priority || "Medium") === priorityFilter;
      return statusMatches && priorityMatches;
    });
  }, [priorityFilter, statusFilter, tasks]);

  const groupedTasks = useMemo(() => {
    return Object.fromEntries(
      STATUSES.map((status) => [
        status,
        visibleTasks.filter((task) => (task.status || "Todo") === status),
      ]),
    );
  }, [visibleTasks]);

  const runAI = async () => {
    if (!aiCommand.trim() || busy) return;
    setBusy(true);
    const result = await sendSimplePrompt(aiCommand);
    setBusy(false);

    if (!result) {
      showNotice("AI task creation failed.");
      return;
    }

    setAiCommand("");
    showNotice(`AI job #${result.id} queued. You can submit the next one.`);
  };

  const addTask = async () => {
    if (!taskDraft.title.trim() || busy) return;
    setBusy(true);
    const result = await createTask({
      title: taskDraft.title.trim(),
      description: taskDraft.description.trim(),
      priority: taskDraft.priority,
      status: "Todo",
      dueDate: taskDraft.dueDate ? new Date(taskDraft.dueDate).toISOString() : null,
    });
    setBusy(false);

    if (!result) {
      showNotice("Task could not be created.");
      return;
    }

    setTaskDraft(emptyTask);
    await loadTasks();
    showNotice("Task added.");
  };

  const removeTask = async (id) => {
    const result = await deleteTask(id);
    if (!result) {
      showNotice("Task could not be deleted.");
      return;
    }

    await loadTasks();
    showNotice("Task deleted.");
  };

  const changeStatus = async (task, status) => {
    if ((task.status || "Todo") === status) return;
    const result = await updateTask(task.id, { ...task, status });
    if (!result) {
      showNotice("Status update failed.");
      return;
    }

    await loadTasks();
  };

  const startEdit = (task) => {
    setEdit({
      id: task.id,
      title: task.title,
      description: task.description || "",
      priority: task.priority || "Medium",
      status: task.status || "Todo",
      dueDate: task.dueDate ? task.dueDate.split("T")[0] : "",
    });
  };

  const saveEdit = async () => {
    if (!edit?.title.trim()) return;
    const result = await updateTask(edit.id, {
      ...edit,
      title: edit.title.trim(),
      description: edit.description.trim(),
      dueDate: edit.dueDate ? new Date(edit.dueDate).toISOString() : null,
    });

    if (!result) {
      showNotice("Task update failed.");
      return;
    }

    setEdit(null);
    await loadTasks();
    showNotice("Task updated.");
  };

  const sendChat = async () => {
    if (!chatInput.trim() || busy) return;

    const input = chatInput.trim();
    setChat((items) => [...items, { role: "user", text: input }]);
    setChatInput("");
    setBusy(true);

    const data = await sendChatMessage(input);
    setBusy(false);
    setChat((items) => [
      ...items,
      {
        role: "ai",
        text: data?.response || "No response returned.",
      },
    ]);
  };

  return (
    <main className="app-shell">
      <aside className="sidebar">
        <div className="brand">
          <div className="brand-mark">AI</div>
          <div>
            <strong>TaskOps</strong>
            <span>AI task control</span>
          </div>
        </div>

        <nav className="nav-tabs" aria-label="Main views">
          <button
            className={activeView === "tasks" ? "active" : ""}
            onClick={() => setActiveView("tasks")}
            type="button"
          >
            <Icon name="board" />
            Tasks
          </button>
          <button
            className={activeView === "chat" ? "active" : ""}
            onClick={() => setActiveView("chat")}
            type="button"
          >
            <Icon name="chat" />
            Chat
          </button>
        </nav>

        <section className="panel">
          <div className="panel-title">
            <Icon name="spark" />
            <h2>AI Command</h2>
          </div>
          <textarea
            value={aiCommand}
            onChange={(event) => setAiCommand(event.target.value)}
            onKeyDown={(event) => {
              if (event.key === "Enter" && !event.shiftKey) {
                event.preventDefault();
                runAI();
              }
            }}
            placeholder="Create a high priority task to review Docker setup tomorrow"
            rows={5}
          />
          <button className="primary-action" onClick={runAI} disabled={busy} type="button">
            <Icon name="spark" />
            {busy ? "Working" : "Create with AI"}
          </button>
        </section>

        {activeView === "tasks" && (
          <section className="panel">
            <div className="panel-title">
              <Icon name="plus" />
              <h2>New Task</h2>
            </div>
            <input
              value={taskDraft.title}
              onChange={(event) =>
                setTaskDraft({ ...taskDraft, title: event.target.value })
              }
              onKeyDown={(event) => {
                if (event.key === "Enter") addTask();
              }}
              placeholder="Title"
            />
            <textarea
              value={taskDraft.description}
              onChange={(event) =>
                setTaskDraft({ ...taskDraft, description: event.target.value })
              }
              placeholder="Description"
              rows={3}
            />
            <div className="field-grid">
              <select
                value={taskDraft.priority}
                onChange={(event) =>
                  setTaskDraft({ ...taskDraft, priority: event.target.value })
                }
              >
                {PRIORITIES.map((item) => (
                  <option key={item}>{item}</option>
                ))}
              </select>
              <input
                type="date"
                value={taskDraft.dueDate}
                onChange={(event) =>
                  setTaskDraft({ ...taskDraft, dueDate: event.target.value })
                }
              />
            </div>
            <button className="primary-action" onClick={addTask} disabled={busy} type="button">
              <Icon name="plus" />
              Add Task
            </button>
          </section>
        )}

        {edit && (
          <section className="panel edit-panel">
            <div className="panel-title">
              <Icon name="edit" />
              <h2>Edit Task</h2>
            </div>
            <input
              value={edit.title}
              onChange={(event) => setEdit({ ...edit, title: event.target.value })}
              placeholder="Title"
            />
            <textarea
              value={edit.description}
              onChange={(event) =>
                setEdit({ ...edit, description: event.target.value })
              }
              rows={3}
              placeholder="Description"
            />
            <div className="field-grid">
              <select
                value={edit.priority}
                onChange={(event) => setEdit({ ...edit, priority: event.target.value })}
              >
                {PRIORITIES.map((item) => (
                  <option key={item}>{item}</option>
                ))}
              </select>
              <select
                value={edit.status}
                onChange={(event) => setEdit({ ...edit, status: event.target.value })}
              >
                {STATUSES.map((item) => (
                  <option key={item}>{item}</option>
                ))}
              </select>
            </div>
            <input
              type="date"
              value={edit.dueDate}
              onChange={(event) => setEdit({ ...edit, dueDate: event.target.value })}
            />
            <div className="button-row">
              <button className="ghost-action" onClick={() => setEdit(null)} type="button">
                Cancel
              </button>
              <button className="primary-action" onClick={saveEdit} type="button">
                <Icon name="check" />
                Save
              </button>
            </div>
          </section>
        )}
      </aside>

      <section className="workspace">
        <header className="topbar">
          <div>
            <p className="eyebrow">AI Task System</p>
            <h1>{activeView === "tasks" ? "Delivery Board" : "AI Assistant"}</h1>
          </div>
          <div className="topbar-actions">
            {notice && <span className="notice">{notice}</span>}
            <button className="ghost-action" onClick={loadTasks} type="button">
              <Icon name="refresh" />
              Refresh
            </button>
          </div>
        </header>

        {activeView === "tasks" && (
          <>
            <section className="metric-grid">
              <article>
                <span>Total</span>
                <strong>{metrics.total}</strong>
              </article>
              <article>
                <span>Active</span>
                <strong>{metrics.active}</strong>
              </article>
              <article>
                <span>Done</span>
                <strong>{metrics.done}</strong>
              </article>
              <article>
                <span>High Priority</span>
                <strong>{metrics.high}</strong>
              </article>
            </section>

            <section className="toolbar">
              <div className="segmented">
                {["All", ...STATUSES].map((status) => (
                  <button
                    key={status}
                    className={statusFilter === status ? "selected" : ""}
                    onClick={() => setStatusFilter(status)}
                    type="button"
                  >
                    {status}
                  </button>
                ))}
              </div>
              <select
                value={priorityFilter}
                onChange={(event) => setPriorityFilter(event.target.value)}
                aria-label="Priority filter"
              >
                {["All", ...PRIORITIES].map((priority) => (
                  <option key={priority}>{priority}</option>
                ))}
              </select>
            </section>

            <section className="board">
              {STATUSES.map((status) => (
                <Column
                  key={status}
                  loading={loadingTasks}
                  onEdit={startEdit}
                  onRemove={removeTask}
                  onStatusChange={changeStatus}
                  status={status}
                  tasks={groupedTasks[status]}
                />
              ))}
            </section>
          </>
        )}

        {activeView === "chat" && (
          <section className="chat-layout">
            <div className="chat-thread">
              {chat.length === 0 ? (
                <div className="empty-state">
                  <Icon name="chat" />
                  <h2>Start a conversation</h2>
                  <p>Ask about your tasks, planning, or implementation work.</p>
                </div>
              ) : (
                chat.map((message, index) => (
                  <div className={`message ${message.role}`} key={`${message.role}-${index}`}>
                    <span>{message.role === "user" ? "You" : "AI"}</span>
                    <p>{message.text}</p>
                  </div>
                ))
              )}
            </div>
            <div className="chat-composer">
              <textarea
                value={chatInput}
                onChange={(event) => setChatInput(event.target.value)}
                onKeyDown={(event) => {
                  if (event.key === "Enter" && !event.shiftKey) {
                    event.preventDefault();
                    sendChat();
                  }
                }}
                placeholder="Ask the assistant"
                rows={3}
              />
              <button className="primary-action" onClick={sendChat} disabled={busy} type="button">
                <Icon name="send" />
                Send
              </button>
            </div>
          </section>
        )}
      </section>
    </main>
  );
}

function Column({ loading, onEdit, onRemove, onStatusChange, status, tasks }) {
  return (
    <article className="column">
      <header>
        <div>
          <span className={`status-dot ${statusMeta[status].tone}`} />
          <h2>{statusMeta[status].label}</h2>
        </div>
        <strong>{tasks.length}</strong>
      </header>

      <div className="task-list">
        {loading ? (
          <div className="empty-column">Loading tasks</div>
        ) : tasks.length === 0 ? (
          <div className="empty-column">No tasks</div>
        ) : (
          tasks.map((task) => (
            <TaskCard
              key={task.id}
              onEdit={onEdit}
              onRemove={onRemove}
              onStatusChange={onStatusChange}
              task={task}
            />
          ))
        )}
      </div>
    </article>
  );
}

function TaskCard({ onEdit, onRemove, onStatusChange, task }) {
  const priority = task.priority || "Medium";

  return (
    <article className="task-card">
      <div className="task-card-head">
        <span className={`chip ${priorityMeta[priority]?.tone || "info"}`}>
          {priorityMeta[priority]?.label || priority}
        </span>
        <div className="icon-actions">
          <button onClick={() => onEdit(task)} title="Edit task" type="button">
            <Icon name="edit" />
          </button>
          <button onClick={() => onRemove(task.id)} title="Delete task" type="button">
            <Icon name="trash" />
          </button>
        </div>
      </div>
      <h3>{task.title}</h3>
      <p>{task.description || "No description"}</p>
      <div className="task-footer">
        <span>{formatDate(task.dueDate)}</span>
        <select
          value={task.status || "Todo"}
          onChange={(event) => onStatusChange(task, event.target.value)}
          aria-label="Task status"
        >
          {STATUSES.map((status) => (
            <option key={status}>{status}</option>
          ))}
        </select>
      </div>
    </article>
  );
}

export default App;
