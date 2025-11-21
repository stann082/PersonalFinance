using PersonalFinance.Domain.Models;

namespace PersonalFinance.UI;

public class UiOptions : ISearchOptions
{
    public string Filter { get; set; } = string.Empty;
    public string PayPeriod { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public DateTime? StartDate { get; set; } = DateTime.MinValue;
    public DateTime? StopDate { get; set; } = DateTime.MaxValue;
}
