using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Users.Api.Models;

namespace Users.Api.Data;

public static class UserDbInitializer
{
    public static async Task InitializeAsync(
        IServiceProvider services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        await using var scope = services.CreateAsyncScope();
        var scopedServices = scope.ServiceProvider;
        var db = scopedServices.GetRequiredService<UserDb>();
        await db.Database.EnsureCreatedAsync();

        if (await db.Users.AnyAsync())
        {
            return;
        }

        var username = configuration["Bootstrap:Username"]
            ?? (environment.IsDevelopment()
                ? "admin"
                : throw new InvalidOperationException(
                    "Set Bootstrap:Username and Bootstrap:Password before first production startup."));
        var password = configuration["Bootstrap:Password"]
            ?? (environment.IsDevelopment()
                ? "ChangeMe123!"
                : throw new InvalidOperationException(
                    "Set Bootstrap:Username and Bootstrap:Password before first production startup."));

        if (password.Length < 12)
        {
            throw new InvalidOperationException("Bootstrap:Password must contain at least 12 characters.");
        }

        var user = new AppUser
        {
            Username = username,
            Role = "Admin",
            Permissions = "tasks.read,tasks.write,skins.read,skins.write"
        };
        user.PasswordHash = scopedServices.GetRequiredService<IPasswordHasher<AppUser>>()
            .HashPassword(user, password);

        db.Users.Add(user);
        await db.SaveChangesAsync();
    }
}