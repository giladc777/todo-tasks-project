namespace Users.Api.Contracts;

public sealed record CreateUserRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public string Role { get; init; } = "User";
    public string[] Permissions { get; init; } = ["tasks.read", "skins.read"];
}