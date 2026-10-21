namespace Tasks.Api.Contracts;

public sealed record TaskResponse(
    Guid Id,
    string Title,
    string? Description,
    bool IsCompleted,
    string Owner,
    DateTimeOffset CreatedAt);