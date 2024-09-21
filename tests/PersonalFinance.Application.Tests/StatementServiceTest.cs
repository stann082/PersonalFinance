using MongoDB.Driver;
using PersonalFinance.Application.Services;
using PersonalFinance.Domain.Models;
using PersonalFinance.Domain.Services;

namespace PersonalFinance.Infrastructure.Tests;

public class BankStatementParserTest
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

    #region Blue Sky Tests

    [Test]
    public async Task BlueSkyTest_SaveAndFetchItems()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new MockParsedTransactionItem { Description = "Test 1", Amount = "100", Date = DateTime.UtcNow.ToShortDateString(), Balance = "500" },
            new MockParsedTransactionItem { Description = "Test 2", Amount = "200", Date = DateTime.UtcNow.ToShortDateString(), Balance = "700" }
        };

        // Act
        IStatementService service = new StatementService(_database);
        await service.SaveAsync(parsedItems, new MockSavedOptions());

        // Assert
        var savedTransactions = await service.GetAsync();
        Assert.That(savedTransactions.Count, Is.EqualTo(2));
    }

    #endregion

    [TearDown]
    public void TearDown()
    {
        _client.DropDatabase("financial_data_test");
    }

    #region Mock Classes

    private class MockParsedTransactionItem : IParsedTransactionItem
    {
        public string? Description { get; set; }
        public bool IsRecurring { get; set; }
        public string? Amount { get; set; }

        public string? Category { get; set; }
        public string? Date { get; set; }

        public string? Balance { get; set; }
    }

    private class MockSavedOptions : ISaveOptions
    {

        public bool Review => false;
        public bool Verbose => false;
    }

    #endregion

}
