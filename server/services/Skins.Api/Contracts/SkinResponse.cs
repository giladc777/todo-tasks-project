namespace Skins.Api.Contracts;

public sealed record SkinResponse(
    Guid Id,
    string Name,
    string PrimaryColor,
    string AccentColor,
    string BackgroundColor,
    string FontFamily,
    bool IsActive);