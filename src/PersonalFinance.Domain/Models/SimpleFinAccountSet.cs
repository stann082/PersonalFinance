using System.Text.Json.Serialization;

namespace PersonalFinance.Domain.Models;

/// <summary>
/// Response body of GET {accessUrl}/accounts. Field names follow the SimpleFIN
/// protocol (https://www.simplefin.org/protocol.html) rather than C# conventions.
/// </summary>
public class SimpleFinAccountSet
{
    /// <summary>Deprecated in protocol v2.0.0 but still emitted by the beta bridge.</summary>
    [JsonPropertyName("errors")]
    public string[] Errors { get; set; } = [];

    [JsonPropertyName("errlist")]
    public Dictionary<string, string[]>? ErrList { get; set; }

    [JsonPropertyName("accounts")]
    public SimpleFinAccount[] Accounts { get; set; } = [];
}

public class SimpleFinAccount
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("balance")]
    public string? Balance { get; set; }

    [JsonPropertyName("available-balance")]
    public string? AvailableBalance { get; set; }

    [JsonPropertyName("balance-date")]
    public long BalanceDate { get; set; }

    [JsonPropertyName("org")]
    public SimpleFinOrganization? Org { get; set; }

    [JsonPropertyName("transactions")]
    public SimpleFinTransaction[] Transactions { get; set; } = [];
}

public class SimpleFinOrganization
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("domain")]
    public string? Domain { get; set; }

    [JsonPropertyName("sfin-url")]
    public string? SfinUrl { get; set; }
}

public class SimpleFinTransaction
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    /// <summary>Unix epoch seconds. When the transaction posted.</summary>
    [JsonPropertyName("posted")]
    public long Posted { get; set; }

    /// <summary>Unix epoch seconds. When the transaction actually occurred, if known.</summary>
    [JsonPropertyName("transacted_at")]
    public long? TransactedAt { get; set; }

    /// <summary>Numeric string. Positive is a deposit.</summary>
    [JsonPropertyName("amount")]
    public string? Amount { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("payee")]
    public string? Payee { get; set; }

    [JsonPropertyName("memo")]
    public string? Memo { get; set; }

    [JsonPropertyName("pending")]
    public bool Pending { get; set; }
}
