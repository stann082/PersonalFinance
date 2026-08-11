using MongoDB.Driver;
using PersonalFinance.Application.Services;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.Application.Tests;

/// <summary>
/// Covers the dedupe paths introduced for SimpleFIN syncing, where transactions arrive with a
/// stable source id but no running balance.
/// </summary>
public class StatementServiceExternalIdTest
{
    private IMongoDatabase _database;
    private MongoClient _client;

    [SetUp]
    public void SetUp()
    {
        _client = new MongoClient("mongodb://localhost:27017");
        _database = _client.GetDatabase("financial_data_test");
        _database.DropCollection("transactions");
    }

    #region ExternalId Matching Tests

    [Test]
    public async Task SaveAsync_WithSourceId_StoresExternalId()
    {
        // Arrange
        var items = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem
            {
                Description = "Coffee", Amount = "-4.50", Date = "01/15/2024", SourceId = "acct-1:txn-1"
            }
        };

        IStatementService service = new StatementService(_database);

        // Act
        await service.SaveAsync(items, new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].ExternalId, Is.EqualTo("acct-1:txn-1"));
    }

    [Test]
    public async Task SaveAsync_RerunWithSameSourceId_DoesNotDuplicate()
    {
        // Arrange
        var items = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem
            {
                Description = "Coffee", Amount = "-4.50", Date = "01/15/2024", SourceId = "acct-1:txn-1"
            }
        };

        IStatementService service = new StatementService(_database);
        await service.SaveAsync(items, new MockSaveOptions());

        // Act — a second sync covering the same window
        await service.SaveAsync(items, new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task SaveAsync_WhenDescriptionChangesButSourceIdMatches_DoesNotDuplicate()
    {
        // Arrange — the bridge sometimes cleans up a description after a transaction settles
        IStatementService service = new StatementService(_database);
        await service.SaveAsync([
            new MockParsedTransactionItem
            {
                Description = "SQ *COFFEE 1234", Amount = "-4.50", Date = "01/15/2024", SourceId = "acct-1:txn-1"
            }
        ], new MockSaveOptions());

        // Act
        await service.SaveAsync([
            new MockParsedTransactionItem
            {
                Description = "Square Coffee Shop", Amount = "-4.50", Date = "01/15/2024", SourceId = "acct-1:txn-1"
            }
        ], new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task SaveAsync_WithSameTransactionIdInDifferentAccounts_KeepsBothSeparate()
    {
        // Arrange — SimpleFIN transaction ids are only unique within an account
        var items = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem
            {
                Description = "Transfer", Amount = "-50", Date = "01/15/2024", SourceId = "checking:txn-9"
            },
            new MockParsedTransactionItem
            {
                Description = "Transfer", Amount = "50", Date = "01/15/2024", SourceId = "savings:txn-9"
            }
        };

        IStatementService service = new StatementService(_database);

        // Act
        await service.SaveAsync(items, new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(2));
    }

    #endregion

    #region Reconciliation Tests

    [Test]
    public async Task SaveAsync_WhenSyncedTransactionMatchesCsvImport_BackfillsExternalIdInsteadOfInserting()
    {
        // Arrange — a transaction that arrived first via CSV, complete with a running balance
        IStatementService service = new StatementService(_database);
        await service.SaveAsync([
            new MockParsedTransactionItem
            {
                Description = "Groceries", Amount = "-82.13", Date = "01/15/2024", Balance = "1200.00"
            }
        ], new MockSaveOptions());

        // Act — the same transaction now shows up in a sync, which carries no balance
        await service.SaveAsync([
            new MockParsedTransactionItem
            {
                Description = "Groceries", Amount = "-82.13", Date = "01/15/2024", SourceId = "acct-1:txn-42"
            }
        ], new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(1), "the sync should have linked to the CSV row, not inserted a second one");
        Assert.That(result[0].ExternalId, Is.EqualTo("acct-1:txn-42"));
        Assert.That(result[0].Balance, Is.EqualTo(1200.00m), "the CSV running balance should survive reconciliation");
    }

    [Test]
    public async Task SaveAsync_AfterReconciling_SubsequentSyncsStillDoNotDuplicate()
    {
        // Arrange
        IStatementService service = new StatementService(_database);
        await service.SaveAsync([
            new MockParsedTransactionItem
            {
                Description = "Groceries", Amount = "-82.13", Date = "01/15/2024", Balance = "1200.00"
            }
        ], new MockSaveOptions());

        var synced = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem
            {
                Description = "Groceries", Amount = "-82.13", Date = "01/15/2024", SourceId = "acct-1:txn-42"
            }
        };
        await service.SaveAsync(synced, new MockSaveOptions());

        // Act
        await service.SaveAsync(synced, new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task SaveAsync_DoesNotReconcileAgainstAnAlreadyLinkedTransaction()
    {
        // Arrange — two identical charges on the same day, one already linked
        IStatementService service = new StatementService(_database);
        await service.SaveAsync([
            new MockParsedTransactionItem
            {
                Description = "Coffee", Amount = "-4.50", Date = "01/15/2024", SourceId = "acct-1:txn-1"
            }
        ], new MockSaveOptions());

        // Act — a genuinely different transaction that happens to look the same
        await service.SaveAsync([
            new MockParsedTransactionItem
            {
                Description = "Coffee", Amount = "-4.50", Date = "01/15/2024", SourceId = "acct-1:txn-2"
            }
        ], new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(2));
        Assert.That(result.Select(t => t.ExternalId), Is.EquivalentTo(new[] { "acct-1:txn-1", "acct-1:txn-2" }));
    }

    #endregion

    #region CSV Behavior Preservation Tests

    [Test]
    public async Task SaveAsync_WithoutSourceId_StillMatchesOnBalance()
    {
        // Arrange — same description, amount and date, different running balance means two
        // distinct transactions under the CSV rules, and that must not change
        var items = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem
            {
                Description = "Coffee", Amount = "-4.50", Date = "01/15/2024", Balance = "500.00"
            },
            new MockParsedTransactionItem
            {
                Description = "Coffee", Amount = "-4.50", Date = "01/15/2024", Balance = "495.50"
            }
        };

        IStatementService service = new StatementService(_database);

        // Act
        await service.SaveAsync(items, new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(2));
        Assert.That(result.All(t => t.ExternalId == null), Is.True);
    }

    [Test]
    public async Task SaveAsync_SyncedTransactionUpdatesRecurringStatusOnMatch()
    {
        // Arrange
        IStatementService service = new StatementService(_database);
        await service.SaveAsync([
            new MockParsedTransactionItem
            {
                Description = "Gym", Amount = "-40", Date = "01/15/2024",
                SourceId = "acct-1:txn-7", IsRecurring = false
            }
        ], new MockSaveOptions());

        // Act
        await service.SaveAsync([
            new MockParsedTransactionItem
            {
                Description = "Gym", Amount = "-40", Date = "01/15/2024",
                SourceId = "acct-1:txn-7", IsRecurring = true
            }
        ], new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(1));
        Assert.That(result[0].IsRecurring, Is.True);
    }

    #endregion

    #region Index Tests

    [Test]
    public async Task EnsureIndexesAsync_IsIdempotent()
    {
        // Arrange
        IStatementService service = new StatementService(_database);

        // Act
        await service.EnsureIndexesAsync();
        await service.EnsureIndexesAsync();

        // Assert
        var collection = ((StatementService)service).GetTransactionsCollection();
        var indexes = await (await collection.Indexes.ListAsync()).ToListAsync();
        Assert.That(indexes.Count(i => i["name"] == "external_id_unique"), Is.EqualTo(1));
    }

    [Test]
    public async Task EnsureIndexesAsync_AllowsMultipleTransactionsWithoutExternalId()
    {
        // Arrange — the index must be sparse, or CSV-only rows would collide on null
        IStatementService service = new StatementService(_database);
        await service.EnsureIndexesAsync();

        // Act
        await service.SaveAsync([
            new MockParsedTransactionItem { Description = "A", Amount = "-1", Date = "01/15/2024", Balance = "10" },
            new MockParsedTransactionItem { Description = "B", Amount = "-2", Date = "01/15/2024", Balance = "8" }
        ], new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(2));
    }

    #endregion

    [TearDown]
    public void TearDown()
    {
        _client.DropDatabase("financial_data_test");
        _client.Dispose();
    }

    #region Mock Classes

    private class MockParsedTransactionItem : IParsedTransactionItem
    {
        public string? Amount { get; set; }
        public string? Balance { get; set; }
        public string? Category { get; set; }
        public string? Date { get; set; }
        public string? Description { get; set; }
        public bool? IsRecurring { get; set; }
        public string? SourceId { get; set; }
    }

    private class MockSaveOptions : ISaveOptions
    {
        public bool Verbose { get; set; } = false;
    }

    #endregion
}
