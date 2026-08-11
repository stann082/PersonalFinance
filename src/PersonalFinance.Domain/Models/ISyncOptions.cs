namespace PersonalFinance.Domain.Models;

public interface ISyncOptions : ISaveOptions
{
    int Days { get; }
    bool IncludePending { get; }
    bool DryRun { get; }
}
