namespace PersonalFinance.Domain.Models;

public interface ISaveOptions
{
    bool Review { get; }
    bool Verbose { get; }
}
