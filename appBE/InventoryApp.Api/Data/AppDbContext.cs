using InventoryApp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryApp.Api.Data;

// Успадковуємось від базового DbContext
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Ця властивість представляє таблицю "InventoryItems" у нашій базі даних
    public DbSet<InventoryItem> InventoryItems { get; set; }
    
    public DbSet<Category> Categories { get; set; }
    
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Username must be unique
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();
    }
}

