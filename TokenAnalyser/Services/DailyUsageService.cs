using Microsoft.EntityFrameworkCore;
using TokenAnalyser.Data;
using TokenAnalyser.Models;

namespace TokenAnalyser.Services;

public class DailyUsageService(IDbContextFactory<AppDbContext> dbFactory)
{
    public List<DailyUsage> GetAll()
    {
        using var db = dbFactory.CreateDbContext();
        return [.. db.DailyUsages.OrderBy(e => e.Date)];
    }

    public List<DailyUsage> GetByMonth(int year, int month)
    {
        var from = new DateOnly(year, month, 1);
        var to = new DateOnly(year, month, DateTime.DaysInMonth(year, month));
        using var db = dbFactory.CreateDbContext();
        return [.. db.DailyUsages
            .Where(e => e.Date >= from && e.Date <= to)
            .OrderBy(e => e.Date)];
    }

    public DailyUsage? GetById(Guid id)
    {
        using var db = dbFactory.CreateDbContext();
        return db.DailyUsages.Find(id);
    }

    public void Add(DailyUsage entry)
    {
        entry.Id = Guid.NewGuid();
        using var db = dbFactory.CreateDbContext();
        db.DailyUsages.Add(entry);
        db.SaveChanges();
    }

    public void Update(DailyUsage entry)
    {
        using var db = dbFactory.CreateDbContext();
        var existing = db.DailyUsages.Find(entry.Id);
        if (existing is null) return;
        existing.Date = entry.Date;
        existing.Usage = entry.Usage;
        db.SaveChanges();
    }

    public void Delete(Guid id)
    {
        using var db = dbFactory.CreateDbContext();
        var entry = db.DailyUsages.Find(id);
        if (entry is not null)
        {
            db.DailyUsages.Remove(entry);
            db.SaveChanges();
        }
    }
}
