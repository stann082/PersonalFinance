namespace PersonalFinance.Domain.Models;

public interface IRemoveOptions
{
    bool DuplicateItems { get; }
    string? Id { get; }
    bool ShowOnly { get; }
}