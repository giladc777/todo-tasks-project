using Microsoft.EntityFrameworkCore;
using Skins.Api.Models;

namespace Skins.Api.Data;

public static class SkinDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SkinDb>();

        await db.Database.EnsureCreatedAsync();

        if (await db.Skins.AnyAsync())
        {
            return;
        }

        db.Skins.AddRange(
            new Skin
            {
                Name = "Ocean",
                PrimaryColor = "#176b87",
                AccentColor = "#f4a261",
                BackgroundColor = "#f2f7f8",
                FontFamily = "Inter",
                IsActive = true
            },
            new Skin
            {
                Name = "Forest",
                PrimaryColor = "#286a4b",
                AccentColor = "#d8a657",
                BackgroundColor = "#f4f7ef",
                FontFamily = "Georgia"
            });

        await db.SaveChangesAsync();
    }
}