using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using Tasks.Api.Contracts;
using Tasks.Api.Data;
using Tasks.Api.Models;
using Todo.Contracts;

namespace Tasks.Api.Services;

public sealed class TaskService(
    TaskDb db,
    IConnectionMultiplexer redis,
    IEventPublisher events,
    IHttpContextAccessor accessor,
    ILogger<TaskService> logger) : ITaskService
{
    private string CurrentOwner => accessor.HttpContext?.User.FindFirstValue(ClaimTypes.Name) ?? "demo";
    private string CacheKey => $"tasks:all:{CurrentOwner}";

    private static TaskResponse ToResponse(TodoTask task) =>
        new(task.Id, task.Title, task.Description, task.IsCompleted, task.Owner, task.CreatedAt);

    public async Task<IReadOnlyList<TaskResponse>> ListAsync(CancellationToken ct)
    {
        try
        {
            var cached = await redis.GetDatabase().StringGetAsync(CacheKey);
            if (cached.HasValue)
            {
                return JsonSerializer.Deserialize<List<TaskResponse>>(cached!) ?? [];
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis read failed; falling back to SQLite");
        }

        var owner = CurrentOwner;
        var entities = await db.Tasks.AsNoTracking()
            .Where(task => task.Owner == owner)
            .ToListAsync(ct);
        var list = entities.OrderByDescending(task => task.CreatedAt).Select(ToResponse).ToList();

        try
        {
            await redis.GetDatabase().StringSetAsync(CacheKey, JsonSerializer.Serialize(list), TimeSpan.FromMinutes(3));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis cache write failed");
        }

        return list;
    }

    public async Task<TaskResponse?> GetAsync(Guid id, CancellationToken ct)
    {
        var owner = CurrentOwner;
        var task = await db.Tasks.AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id && item.Owner == owner, ct);
        return task is null ? null : ToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken ct)
    {
        var owner = CurrentOwner;
        var task = new TodoTask
        {
            Title = request.Title.Trim(),
            Description = request.Description,
            Owner = owner
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);
        await InvalidateCache(owner);

        try
        {
            await events.PublishAsync(
                "task.created",
                new TaskCreatedEvent(task.Id, task.Title, task.Owner, task.CreatedAt),
                ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Task saved; event publish failed for {TaskId}", task.Id);
        }

        logger.LogInformation("Created task {TaskId}", task.Id);
        return ToResponse(task);
    }

    public async Task<TaskResponse?> UpdateAsync(Guid id, UpdateTaskRequest request, CancellationToken ct)
    {
        var owner = CurrentOwner;
        var task = await db.Tasks.FirstOrDefaultAsync(item => item.Id == id && item.Owner == owner, ct);
        if (task is null)
        {
            return null;
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description;
        task.IsCompleted = request.IsCompleted;
        await db.SaveChangesAsync(ct);
        await InvalidateCache(owner);
        logger.LogInformation("Updated task {TaskId}", id);
        return ToResponse(task);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct)
    {
        var owner = CurrentOwner;
        var task = await db.Tasks.FirstOrDefaultAsync(item => item.Id == id && item.Owner == owner, ct);
        if (task is null)
        {
            return false;
        }

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(ct);
        await InvalidateCache(owner);
        logger.LogInformation("Deleted task {TaskId}", id);
        return true;
    }

    private async Task InvalidateCache(string owner)
    {
        try
        {
            await redis.GetDatabase().KeyDeleteAsync($"tasks:all:{owner}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Redis invalidation failed");
        }
    }
}