using Microsoft.EntityFrameworkCore;
using AiTaskApi.Data;
using AiTaskApi.Shared.DTOs;
using AiTaskApi.Models;

namespace AiTaskApi.Services;

public class TaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    // ---------------- GET ALL ----------------
    public async Task<PagedResult<TaskReadDto>> GetAll(
        bool? isDone,
        string? title,
        int page,
        int pageSize)
    {
        var query = _context.Tasks.AsQueryable();

        if (!string.IsNullOrEmpty(title))
            query = query.Where(t => t.Title.Contains(title));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TaskReadDto
            {
                Id = t.Id,
                Title = t.Title,
                Description = t.Description,
                Priority = t.Priority,
                Status = t.Status,
                DueDate = t.DueDate
            })
            .ToListAsync();

        return new PagedResult<TaskReadDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ---------------- GET BY ID ----------------
    public async Task<TaskReadDto?> GetById(int id)
    {
        var t = await _context.Tasks.FindAsync(id);
        if (t == null) return null;

        return new TaskReadDto
        {
            Id = t.Id,
            Title = t.Title,
            Description = t.Description,
            Priority = t.Priority,
            Status = t.Status,
            DueDate = t.DueDate
        };
    }

    // ---------------- CREATE ----------------
    public async Task<TaskReadDto> Create(TaskCreateDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            Priority = dto.Priority,
            Status = dto.Status,
            DueDate = dto.DueDate
        };

        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();

        return new TaskReadDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            Priority = task.Priority,
            Status = task.Status,
            DueDate = task.DueDate
        };
    }

    // ---------------- UPDATE (FIXED) ----------------
    public async Task<bool> Update(int id, TaskCreateDto dto)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return false;

        task.Title = dto.Title;
        task.Description = dto.Description;

        task.Priority = dto.Priority ?? "Medium";
        task.Status = dto.Status ?? "Todo";
        task.DueDate = dto.DueDate;

        await _context.SaveChangesAsync();
        return true;
    }

    // ---------------- DELETE ----------------
    public async Task<bool> Delete(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }
}