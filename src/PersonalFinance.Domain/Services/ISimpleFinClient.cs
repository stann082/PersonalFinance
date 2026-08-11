using PersonalFinance.Domain.Models;

namespace PersonalFinance.Domain.Services;

public interface ISimpleFinClient
{
    /// <summary>
    /// Fetches accounts and their transactions for the given window. The bridge caps
    /// ranges at 90 days and expects roughly 24 requests per day, so callers should
    /// issue exactly one of these per sync.
    /// </summary>
    Task<SimpleFinAccountSet> GetAccountsAsync(DateTimeOffset startDate, DateTimeOffset endDate,
        bool includePending, CancellationToken cancellationToken = default);

    /// <summary>
    /// Exchanges a one-time setup token for a durable access URL. The token cannot be
    /// claimed twice.
    /// </summary>
    Task<string> ClaimAsync(string setupToken, CancellationToken cancellationToken = default);
}
