namespace Skins.Api.Models;

public sealed class Skin
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public required string Name { get; set; }
    public string PrimaryColor { get; set; } = "#176b87";
    public string AccentColor { get; set; } = "#f4a261";
    public string BackgroundColor { get; set; } = "#f2f7f8";
    public string FontFamily { get; set; } = "Inter";
    public bool IsActive { get; set; }
}