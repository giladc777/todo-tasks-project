using Microsoft.EntityFrameworkCore;
using Tasks.Api.Models;

namespace Tasks.Api.Data;

public sealed class TaskDb(DbContextOptions<TaskDb> options) : DbContext(options)
{
    public DbSet<TodoTask> Tasks => Set<TodoTask>();
}