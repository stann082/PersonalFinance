using System.Text.Json;
using PersonalFinance.Domain.Models;
using PersonalFinance.Infrastructure.SimpleFin;

namespace PersonalFinance.Infrastructure.Tests;

public class SimpleFinTransactionItemTest
{

    #region Deserialization Tests

    [Test]
    public void Deserialize_ReadsAccountSetFromProtocolPayload()
    {
        // Arrange
        const string json = """
        {
          "errors": ["Connection to Example Bank needs attention"],
          "accounts": [
            {
              "org": { "domain": "mybank.com", "sfin-url": "https://sfin.mybank.com" },
              "id": "ACT-123",
              "name": "Checking",
              "currency": "USD",
              "balance": "1234.56",
              "available-balance": "1200.00",
              "balance-date": 1705363200,
              "transactions": [
                {
                  "id": "TXN-1",
                  "posted": 1705363200,
                  "amount": "-33.95",
                  "description": "Uncle Frank's Bait Shop",
                  "pending": false
                }
              ]
            }
          ]
        }
        """;

        // Act
        var accountSet = JsonSerializer.Deserialize<SimpleFinAccountSet>(json);

        // Assert
        Assert.That(accountSet, Is.Not.Null);
        Assert.That(accountSet!.Errors, Has.Length.EqualTo(1));
        Assert.That(accountSet.Accounts, Has.Length.EqualTo(1));

        var account = accountSet.Accounts[0];
        Assert.That(account.Id, Is.EqualTo("ACT-123"));
        Assert.That(account.Balance, Is.EqualTo("1234.56"));
        Assert.That(account.AvailableBalance, Is.EqualTo("1200.00"));
        Assert.That(account.BalanceDate, Is.EqualTo(1705363200));
        Assert.That(account.Org?.Domain, Is.EqualTo("mybank.com"));
        Assert.That(account.Transactions, Has.Length.EqualTo(1));
        Assert.That(account.Transactions[0].Amount, Is.EqualTo("-33.95"));
    }

    [Test]
    public void Deserialize_HandlesMissingOptionalFields()
    {
        // Arrange
        const string json = """{ "accounts": [] }""";

        // Act
        var accountSet = JsonSerializer.Deserialize<SimpleFinAccountSet>(json);

        // Assert
        Assert.That(accountSet!.Errors, Is.Empty);
        Assert.That(accountSet.ErrList, Is.Null);
        Assert.That(accountSet.Accounts, Is.Empty);
    }

    [Test]
    public void Deserialize_ReadsErrList()
    {
        // Arrange
        const string json = """
        { "errlist": { "auth": ["Login expired for Example Bank"] }, "accounts": [] }
        """;

        // Act
        var accountSet = JsonSerializer.Deserialize<SimpleFinAccountSet>(json);

        // Assert
        Assert.That(accountSet!.ErrList, Is.Not.Null);
        Assert.That(accountSet.ErrList!["auth"], Has.Length.EqualTo(1));
    }

    #endregion

    #region Mapping Tests

    [Test]
    public void Constructor_MapsSourceIdFromAccountAndTransactionIds()
    {
        // Arrange
        var account = new SimpleFinAccount { Id = "ACT-123" };
        var transaction = new SimpleFinTransaction { Id = "TXN-1", Posted = 1705363200 };

        // Act
        var item = new SimpleFinTransactionItem(account, transaction);

        // Assert
        Assert.That(item.SourceId, Is.EqualTo("ACT-123:TXN-1"));
    }

    [Test]
    public void Constructor_LeavesBalanceNullSoTransactionParsesToZero()
    {
        // Arrange
        var account = new SimpleFinAccount { Id = "ACT-123" };
        var transaction = new SimpleFinTransaction { Id = "TXN-1", Amount = "-10.00", Posted = 1705363200 };

        // Act
        var item = new SimpleFinTransactionItem(account, transaction);

        // Assert
        Assert.That(item.Balance, Is.Null);
        Assert.That(new Transaction(item).Balance, Is.EqualTo(0m));
    }

    [Test]
    public void Constructor_PassesAmountThroughUnchanged()
    {
        // Arrange
        var account = new SimpleFinAccount { Id = "ACT-123" };
        var debit = new SimpleFinTransaction { Id = "T1", Amount = "-33.95", Posted = 1705363200 };
        var credit = new SimpleFinTransaction { Id = "T2", Amount = "1200.00", Posted = 1705363200 };

        // Act
        var debitTransaction = new Transaction(new SimpleFinTransactionItem(account, debit));
        var creditTransaction = new Transaction(new SimpleFinTransactionItem(account, credit));

        // Assert
        Assert.That(debitTransaction.Amount, Is.EqualTo(-33.95m));
        Assert.That(debitTransaction.IsDeposit, Is.False);
        Assert.That(creditTransaction.Amount, Is.EqualTo(1200.00m));
        Assert.That(creditTransaction.IsDeposit, Is.True);
    }

    [Test]
    public void Constructor_CollapsesTimestampToLocalMidnight()
    {
        // Arrange — an epoch with a non-zero time component
        var account = new SimpleFinAccount { Id = "ACT-123" };
        var expected = new DateTimeOffset(2024, 1, 15, 18, 42, 17, TimeSpan.Zero);
        var transaction = new SimpleFinTransaction { Id = "T1", Posted = expected.ToUnixTimeSeconds() };

        // Act
        var item = new SimpleFinTransactionItem(account, transaction);
        var parsed = new Transaction(item);

        // Assert — CSV rows carry no time, so synced rows must not either
        Assert.That(parsed.Date.TimeOfDay, Is.EqualTo(TimeSpan.Zero));
        Assert.That(parsed.Date, Is.EqualTo(expected.ToLocalTime().Date));
    }

    [Test]
    public void Constructor_PrefersTransactedAtOverPosted()
    {
        // Arrange
        var account = new SimpleFinAccount { Id = "ACT-123" };
        var transactedAt = new DateTimeOffset(2024, 1, 12, 12, 0, 0, TimeSpan.Zero);
        var posted = new DateTimeOffset(2024, 1, 15, 12, 0, 0, TimeSpan.Zero);
        var transaction = new SimpleFinTransaction
        {
            Id = "T1",
            Posted = posted.ToUnixTimeSeconds(),
            TransactedAt = transactedAt.ToUnixTimeSeconds()
        };

        // Act
        var parsed = new Transaction(new SimpleFinTransactionItem(account, transaction));

        // Assert
        Assert.That(parsed.Date, Is.EqualTo(transactedAt.ToLocalTime().Date));
    }

    [Test]
    public void Constructor_FallsBackToPayeeWhenDescriptionIsBlank()
    {
        // Arrange
        var account = new SimpleFinAccount { Id = "ACT-123" };
        var transaction = new SimpleFinTransaction
        {
            Id = "T1", Posted = 1705363200, Description = "   ", Payee = "Uncle Frank's Bait Shop"
        };

        // Act
        var item = new SimpleFinTransactionItem(account, transaction);

        // Assert
        Assert.That(item.Description, Is.EqualTo("Uncle Frank's Bait Shop"));
    }

    [Test]
    public void Constructor_FallsBackToMemoWhenDescriptionAndPayeeAreBlank()
    {
        // Arrange
        var account = new SimpleFinAccount { Id = "ACT-123" };
        var transaction = new SimpleFinTransaction { Id = "T1", Posted = 1705363200, Memo = "ATM withdrawal" };

        // Act
        var item = new SimpleFinTransactionItem(account, transaction);

        // Assert
        Assert.That(item.Description, Is.EqualTo("ATM withdrawal"));
    }

    [Test]
    public void Constructor_CarriesExternalIdOntoTransaction()
    {
        // Arrange
        var account = new SimpleFinAccount { Id = "ACT-123" };
        var transaction = new SimpleFinTransaction { Id = "TXN-1", Amount = "-5", Posted = 1705363200 };

        // Act
        var parsed = new Transaction(new SimpleFinTransactionItem(account, transaction));

        // Assert
        Assert.That(parsed.ExternalId, Is.EqualTo("ACT-123:TXN-1"));
        Assert.That(parsed.TransactionId, Is.Not.EqualTo("ACT-123:TXN-1"), "the internal handle stays a Guid");
    }

    #endregion

}
