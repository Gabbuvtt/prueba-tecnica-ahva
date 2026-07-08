namespace PruebaTecnicaAhva.Models.Dashboard;

public class DashboardIndicatorViewModel
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Trend { get; set; } = string.Empty;
    public bool IsPositiveTrend { get; set; }
}
