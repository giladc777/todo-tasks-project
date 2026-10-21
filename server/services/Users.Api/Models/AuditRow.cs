namespace Users.Api.Models;

public sealed class AuditRow
{
    public Guid Id { get; set; }
    public required string Type { get; set; }
    public required string Message { get; set; }
    public DateTimeOffset OccurredAt { get; set; }
}