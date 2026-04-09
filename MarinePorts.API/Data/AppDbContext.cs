using MarinePorts.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MarinePorts.API.Data;

/// <summary>
/// Entity Framework Core database context.
/// Registers all entities and applies any necessary table configuration.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // ── Entity sets ───────────────────────────────────────────────────────────
    public DbSet<AppUser> Users    { get; set; }
    public DbSet<Boat>    Boats    { get; set; }
    public DbSet<Mooring> Moorings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Enforce unique email addresses across all registered users.
        modelBuilder.Entity<AppUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Enforce unique registration numbers for boats.
        modelBuilder.Entity<Boat>()
            .HasIndex(b => b.RegistrationNumber)
            .IsUnique();

        // Enforce unique mooring numbers.
        modelBuilder.Entity<Mooring>()
            .HasIndex(m => m.MooringNumber)
            .IsUnique();

        // A user can own many boats; deleting a user cascades to their boats.
        modelBuilder.Entity<Boat>()
            .HasOne(b => b.AppUser)
            .WithMany(u => u.Boats)
            .HasForeignKey(b => b.AppUserId)
            .OnDelete(DeleteBehavior.Cascade);

        // A user can own many moorings; AppUserId is nullable for ArcGIS imports.
        modelBuilder.Entity<Mooring>()
            .HasOne(m => m.AppUser)
            .WithMany(u => u.Moorings)
            .HasForeignKey(m => m.AppUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
