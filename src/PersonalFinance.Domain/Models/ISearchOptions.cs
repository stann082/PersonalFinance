namespace PersonalFinance.Domain.Models;

public interface ISearchOptions
{
    string Filter { get; }
    string PayPeriod { get; }
    string Query { get; }
    DateTime? StartDate { get; }
    DateTime? StopDate { get; }
}
