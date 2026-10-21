namespace Skins.Api.Contracts;

public sealed record SkinRequest
{
    public required string Name { get; init; }
    public required string PrimaryColor { get; init; }
    public required string AccentColor { get; init; }
    public required string BackgroundColor { get; init; }
    public required string FontFamily { get; init; }
}