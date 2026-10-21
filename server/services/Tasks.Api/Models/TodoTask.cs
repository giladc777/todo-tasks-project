namespace Tasks.Api.Models;

public sealed class TodoTask
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Title { get; set; }
    public string? Description { get; set; }
    public bool IsCompleted { get; set; }
    public string Owner { get; set; } = "demo";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}