namespace Users.Api.Contracts;

public sealed record AuditEventResponse(Guid Id, string Type, string Message, DateTimeOffset OccurredAt);