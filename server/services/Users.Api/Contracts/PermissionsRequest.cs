namespace Users.Api.Contracts;

public sealed record PermissionsRequest
{
    public required string[] Permissions { get; init; }
}