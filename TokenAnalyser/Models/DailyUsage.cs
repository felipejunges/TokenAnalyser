namespace TokenAnalyser.Models;

public class DailyUsage
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public DateOnly Date { get; set; }
    public string Model { get; set; } = string.Empty;
    public decimal Usage { get; set; }
}