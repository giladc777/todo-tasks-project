using Microsoft.EntityFrameworkCore;
using Skins.Api.Models;

namespace Skins.Api.Data;

public sealed class SkinDb(DbContextOptions<SkinDb> options) : DbContext(options)
{
    public DbSet<Skin> Skins => Set<Skin>();
}