using Microsoft.EntityFrameworkCore;
using Users.Api.Models;

namespace Users.Api.Data;

public sealed class UserDb(DbContextOptions<UserDb> options) : DbContext(options)
{
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<AuditRow> AuditEvents => Set<AuditRow>();
}