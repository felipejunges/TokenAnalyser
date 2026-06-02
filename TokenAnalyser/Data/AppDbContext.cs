using Microsoft.EntityFrameworkCore;
using TokenAnalyser.Models;

namespace TokenAnalyser.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<DailyUsage> DailyUsages => Set<DailyUsage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DailyUsage>(entity =>
        {
            entity.HasKey(e => e.Id);
        });
    }
}
