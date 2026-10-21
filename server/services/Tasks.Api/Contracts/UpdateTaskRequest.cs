namespace Tasks.Api.Contracts;

public sealed record UpdateTaskRequest
{
    public required string Title { get; init; }
    public string? Description { get; init; }
    public bool IsCompleted { get; init; }
}