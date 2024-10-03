namespace PersonalFinance.Domain.Models;

public interface ISearchOptions
{
    string Filter { get; }
    string Query { get; }
}
