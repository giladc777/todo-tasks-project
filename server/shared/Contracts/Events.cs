namespace Todo.Contracts;

public sealed record TaskCreatedEvent(Guid TaskId, string Title, string Owner, DateTimeOffset CreatedAt);
public sealed record SkinUpdatedEvent(Guid SkinId, string Name, string PrimaryColor, string FontFamily, DateTimeOffset UpdatedAt);
public sealed record AuditEvent(Guid Id, string Type, string Message, DateTimeOffset OccurredAt);
