using Microsoft.EntityFrameworkCore;
using Tasks.Api.Models;

namespace Tasks.Api.Data;

public static class TaskDbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<TaskDb>();
        await db.Database.EnsureCreatedAsync();

        if (await db.Tasks.AnyAsync(task => task.Owner == "admin"))
        {
            return;
        }

        db.Tasks.AddRange(
            new TodoTask { Title = "Welcome to your ToDo space", Owner = "admin" },
            new TodoTask
            {
                Title = "Try editing the interface skin",
                Owner = "admin",
                IsCompleted = true
            });
        await db.SaveChangesAsync();
    }
}