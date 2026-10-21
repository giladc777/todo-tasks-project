namespace Users.Api.Contracts;

public sealed record LoginResponse(
    string Token,
    string Username,
    string Role,
    string[] Permissions,
    DateTimeOffset ExpiresAt);