namespace PersonalFinance.Domain.Models;

public interface ITrainingModelItem
{
    string? Category { get; set; }
    string? Description { get; set; }
    bool IsRecurring { get; set;  }
}