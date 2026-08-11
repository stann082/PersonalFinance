using MongoDB.Driver;
using PersonalFinance.Application.Services;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.Application.Tests;

public class StatementServiceAdditionalTest
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

    #region GetAsync Tests

    [Test]
    public async Task GetAsync_ReturnsEmptyArrayWhenNoTransactions()
    {
        // Arrange
        IStatementService service = new StatementService(_database);

        // Act
        var result = await service.GetAsync();

        // Assert
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task GetAsync_ReturnsAllTransactions()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem { Description = "Item 1", Amount = "100", Date = DateTime.UtcNow.ToShortDateString(), Balance = "500" },
            new MockParsedTransactionItem { Description = "Item 2", Amount = "200", Date = DateTime.UtcNow.ToShortDateString(), Balance = "700" },
            new MockParsedTransactionItem { Description = "Item 3", Amount = "300", Date = DateTime.UtcNow.ToShortDateString(), Balance = "1000" }
        };

        IStatementService service = new StatementService(_database);
        await service.SaveAsync(parsedItems, new MockSaveOptions());

        // Act
        var result = await service.GetAsync();

        // Assert
        Assert.That(result, Has.Length.EqualTo(3));
    }

    [Test]
    public async Task GetAsync_WithNullSearchOptions_ReturnsAllTransactions()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem { Description = "Item 1", Amount = "100", Date = DateTime.UtcNow.ToShortDateString(), Balance = "500" },
            new MockParsedTransactionItem { Description = "Item 2", Amount = "200", Date = DateTime.UtcNow.ToShortDateString(), Balance = "700" }
        };

        IStatementService service = new StatementService(_database);
        await service.SaveAsync(parsedItems, new MockSaveOptions());

        // Act
        var result = await service.GetAsync(null);

        // Assert
        Assert.That(result, Has.Length.EqualTo(2));
    }

    #endregion

    #region DeleteAsync Tests

    [Test]
    public async Task DeleteAsync_DeletesExistingTransaction()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem { Description = "Item 1", Amount = "100", Date = DateTime.UtcNow.ToShortDateString(), Balance = "500" }
        };

        IStatementService service = new StatementService(_database);
        await service.SaveAsync(parsedItems, new MockSaveOptions());
        var allTransactions = await service.GetAsync();
        var transactionToDelete = allTransactions[0];

        // Act
        var deleteResult = await service.DeleteAsync(transactionToDelete.TransactionId);

        // Assert
        Assert.That(deleteResult.DeletedCount, Is.EqualTo(1));
        var remainingTransactions = await service.GetAsync();
        Assert.That(remainingTransactions, Is.Empty);
    }

    [Test]
    public async Task DeleteAsync_WithNonExistentId_DeletesNothing()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem { Description = "Item 1", Amount = "100", Date = DateTime.UtcNow.ToShortDateString(), Balance = "500" }
        };

        IStatementService service = new StatementService(_database);
        await service.SaveAsync(parsedItems, new MockSaveOptions());

        // Act
        var deleteResult = await service.DeleteAsync("non-existent-id");

        // Assert
        Assert.That(deleteResult.DeletedCount, Is.EqualTo(0));
        var remainingTransactions = await service.GetAsync();
        Assert.That(remainingTransactions, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task DeleteAsync_WithNullId_DeletesNothing()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem { Description = "Item 1", Amount = "100", Date = DateTime.UtcNow.ToShortDateString(), Balance = "500" }
        };

        IStatementService service = new StatementService(_database);
        await service.SaveAsync(parsedItems, new MockSaveOptions());

        // Act
        var deleteResult = await service.DeleteAsync(null);

        // Assert
        Assert.That(deleteResult.DeletedCount, Is.EqualTo(0));
    }

    #endregion

    #region SaveAsync Tests

    [Test]
    public async Task SaveAsync_WithEmptyArray_SavesNothing()
    {
        // Arrange
        var parsedItems = Array.Empty<IParsedTransactionItem>();
        IStatementService service = new StatementService(_database);

        // Act
        await service.SaveAsync(parsedItems, new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Is.Empty);
    }

    [Test]
    public async Task SaveAsync_WithDuplicateTransactions_SkipsExisting()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem { Description = "Duplicate", Amount = "100", Date = "01/01/2024", Balance = "500" }
        };

        IStatementService service = new StatementService(_database);
        await service.SaveAsync(parsedItems, new MockSaveOptions());

        // Act
        await service.SaveAsync(parsedItems, new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task SaveAsync_UpdatesRecurringStatusIfChanged()
    {
        // Arrange — use a debit so IsRecurring is meaningful (credits always get null)
        var parsedItem1 = new MockParsedTransactionItem
        {
            Description = "Payment",
            Amount = "-100",
            Date = "01/01/2024",
            Balance = "500",
            IsRecurring = false
        };

        IStatementService service = new StatementService(_database);
        await service.SaveAsync(new[] { parsedItem1 }, new MockSaveOptions());

        var parsedItem2 = new MockParsedTransactionItem
        {
            Description = "Payment",
            Amount = "-100",
            Date = "01/01/2024",
            Balance = "500",
            IsRecurring = true
        };

        // Act
        await service.SaveAsync(new[] { parsedItem2 }, new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result[0].IsRecurring, Is.True);
    }

    [Test]
    public async Task SaveAsync_WithMultipleTransactions_SavesAll()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem { Description = "Item 1", Amount = "100", Date = DateTime.UtcNow.ToShortDateString(), Balance = "500" },
            new MockParsedTransactionItem { Description = "Item 2", Amount = "200", Date = DateTime.UtcNow.ToShortDateString(), Balance = "700" },
            new MockParsedTransactionItem { Description = "Item 3", Amount = "300", Date = DateTime.UtcNow.ToShortDateString(), Balance = "1000" }
        };

        IStatementService service = new StatementService(_database);

        // Act
        await service.SaveAsync(parsedItems, new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result, Has.Length.EqualTo(3));
    }

    [Test]
    public async Task SaveAsync_WithNegativeAmounts_SavesCorrectly()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem { Description = "Debit", Amount = "-100", Date = DateTime.UtcNow.ToShortDateString(), Balance = "400" },
            new MockParsedTransactionItem { Description = "Credit", Amount = "50", Date = DateTime.UtcNow.ToShortDateString(), Balance = "450" }
        };

        IStatementService service = new StatementService(_database);

        // Act
        await service.SaveAsync(parsedItems, new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result[0].Amount, Is.EqualTo(-100m));
        Assert.That(result[1].Amount, Is.EqualTo(50m));
    }

    [Test]
    public async Task SaveAsync_PreservesCategory()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem { Description = "Groceries", Amount = "50", Date = DateTime.UtcNow.ToShortDateString(), Balance = "500", Category = "Food" }
        };

        IStatementService service = new StatementService(_database);

        // Act
        await service.SaveAsync(parsedItems, new MockSaveOptions());

        // Assert
        var result = await service.GetAsync();
        Assert.That(result[0].Category, Is.EqualTo("Food"));
    }

    #endregion

    #region Verbose Output Tests

    [Test]
    public async Task SaveAsync_VerboseMode_PrintsMessages()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem { Description = "Item 1", Amount = "100", Date = DateTime.UtcNow.ToShortDateString(), Balance = "500" }
        };

        IStatementService service = new StatementService(_database);
        var verboseOptions = new MockSaveOptions { Verbose = true };

        // Act & Assert (checks that it doesn't throw)
        Assert.DoesNotThrowAsync(async () => await service.SaveAsync(parsedItems, verboseOptions));
    }

    #endregion

    #region TearDown

    [TearDown]
    public void TearDown()
    {
        _client.DropDatabase("financial_data_test");
        _client.Dispose();
    }

    #endregion

    #region Mock Classes

    private class MockParsedTransactionItem : IParsedTransactionItem
    {
        public string? Description { get; set; }
        public bool? IsRecurring { get; set; }
        public string? Amount { get; set; }
        public string? Category { get; set; }
        public string? Date { get; set; }
        public string? Balance { get; set; }
        public string? SourceId { get; set; }
    }

    private class MockSaveOptions : ISaveOptions
    {
        public bool Review { get; set; } = false;
        public bool Verbose { get; set; } = false;
    }

    #endregion
}
