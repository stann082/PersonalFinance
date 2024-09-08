using MongoDB.Driver;
using PersonalFinance.Application.Models;
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

    [Test]
    public async Task BlueSkyTest_SaveAndFetchItems()
    {
        // Arrange
        var parsedItems = new IParsedTransactionItem[]
        {
            new ParsedTransactionItemMock { Description = "Test 1", Amount = 100, Date = DateTime.UtcNow, Balance = 500 },
            new ParsedTransactionItemMock { Description = "Test 2", Amount = 200, Date = DateTime.UtcNow, Balance = 700 }
        };

        // Act
        IStatementService service = new StatementService(_database);
        await service.SaveAsync(parsedItems);

        // Assert
        var savedTransactions = await service.Get();
        Assert.That(savedTransactions.Count, Is.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(savedTransactions.Count(t => t is { Description: "Test 1", Amount: 100, Balance: 500 }), Is.EqualTo(0));
            Assert.That(savedTransactions.Count(t => t is { Description: "Test 2", Amount: 200, Balance: 700 }), Is.EqualTo(0));
        });
    }
    
    [TearDown]
    public void TearDown()
    {
        _client.DropDatabase("financial_data_test");
    }
    
    private class ParsedTransactionItemMock : IParsedTransactionItem
    {
        private string _amount;
        private string _balance;
        private string _date;

        string IParsedTransactionItem.Date => _date;

        public string Description { get; set; }
        public bool? IsRecurring { get; set; }
        public decimal Amount { get; set; }

        string IParsedTransactionItem.Balance => _balance;

        public string Category { get; set; }
        public DateTime Date { get; set; }

        string IParsedTransactionItem.Amount => _amount;

        public decimal Balance { get; set; }
    }

}
