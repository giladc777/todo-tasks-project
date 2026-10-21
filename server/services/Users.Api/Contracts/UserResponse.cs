namespace Users.Api.Contracts;

public sealed record UserResponse(Guid Id, string Username, string Role, string[] Permissions);