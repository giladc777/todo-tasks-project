using Microsoft.EntityFrameworkCore;
using Skins.Api.Contracts;
using Skins.Api.Data;
using Skins.Api.Models;
using Todo.Contracts;

namespace Skins.Api.Services;

public sealed class SkinService(SkinDb db, ISkinEvents events, ILogger<SkinService> logger) : ISkinService
{
    private static SkinResponse Map(Skin skin) => new(
        skin.Id,
        skin.Name,
        skin.PrimaryColor,
        skin.AccentColor,
        skin.BackgroundColor,
        skin.FontFamily,
        skin.IsActive);

    public async Task<IReadOnlyList<SkinResponse>> ListAsync(CancellationToken ct) =>
        await db.Skins.AsNoTracking()
            .Select(skin => new SkinResponse(
                skin.Id,
                skin.Name,
                skin.PrimaryColor,
                skin.AccentColor,
                skin.BackgroundColor,
                skin.FontFamily,
                skin.IsActive))
            .ToListAsync(ct);

    public async Task<SkinResponse> ActiveAsync(CancellationToken ct)
    {
        var skin = await db.Skins.AsNoTracking().FirstOrDefaultAsync(item => item.IsActive, ct)
            ?? await db.Skins.AsNoTracking().FirstAsync(ct);
        return Map(skin);
    }

    public async Task<SkinResponse?> SaveAsync(Guid? id, SkinRequest request, CancellationToken ct)
    {
        var skin = id is null
            ? new Skin { Name = request.Name }
            : await db.Skins.FindAsync([id.Value], ct);

        if (skin is null)
        {
            return null;
        }

        skin.Name = request.Name.Trim();
        skin.PrimaryColor = request.PrimaryColor;
        skin.AccentColor = request.AccentColor;
        skin.BackgroundColor = request.BackgroundColor;
        skin.FontFamily = request.FontFamily;

        if (id is null)
        {
            db.Skins.Add(skin);
        }

        await db.SaveChangesAsync(ct);

        try
        {
            await events.PublishAsync(
                "skin.updated",
                new SkinUpdatedEvent(skin.Id, skin.Name, skin.PrimaryColor, skin.FontFamily, DateTimeOffset.UtcNow),
                ct);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Skin saved; event publish failed for {SkinId}", skin.Id);
        }

        logger.LogInformation("Saved skin {SkinId}", skin.Id);
        return Map(skin);
    }

    public async Task<SkinResponse?> ActivateAsync(Guid id, CancellationToken ct)
    {
        var skin = await db.Skins.FindAsync([id], ct);
        if (skin is null)
        {
            return null;
        }

        foreach (var item in db.Skins)
        {
            item.IsActive = item.Id == id;
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Activated skin {SkinId}", id);
        return Map(skin);
    }
}