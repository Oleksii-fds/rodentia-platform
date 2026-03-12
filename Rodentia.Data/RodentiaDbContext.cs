using Microsoft.EntityFrameworkCore;
using Rodentia.Core.Entities;

namespace Rodentia.Data;

public class RodentiaDbContext : DbContext
{
    public RodentiaDbContext(DbContextOptions<RodentiaDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}