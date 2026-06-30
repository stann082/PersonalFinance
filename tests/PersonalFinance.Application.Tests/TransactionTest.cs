using PersonalFinance.Domain.Misc;
using PersonalFinance.Domain.Models;

namespace PersonalFinance.Application.Tests;

public class TransactionTest
{
    #region Constructor Tests

    [Test]
    public void Constructor_InitializesWithValidParsedItem()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Description = "Test Description",
            Amount = "100.50",
            Balance = "5000.00",
            Date = "01/15/2024",
            Category = "Groceries",
            IsRecurring = true
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(transaction.Description, Is.EqualTo("Test Description"));
            Assert.That(transaction.Amount, Is.EqualTo(100.50m));
            Assert.That(transaction.Balance, Is.EqualTo(5000.00m));
            Assert.That(transaction.Date, Is.EqualTo(new DateTime(2024, 1, 15)));
            Assert.That(transaction.Category, Is.EqualTo("Groceries"));
            Assert.That(transaction.IsRecurring, Is.True);
        });
    }

    [Test]
    public void Constructor_SetsNegativeAmountAsDebit()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Amount = "-50.00",
            Date = "01/15/2024"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.That(transaction.TransactionType, Is.EqualTo(TransactionType.Debit));
    }

    [Test]
    public void Constructor_SetsPositiveAmountAsCredit()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Amount = "50.00",
            Date = "01/15/2024"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.That(transaction.TransactionType, Is.EqualTo(TransactionType.Credit));
    }

    [Test]
    public void Constructor_SetsZeroAmountAsCredit()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Amount = "0",
            Date = "01/15/2024"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.That(transaction.TransactionType, Is.EqualTo(TransactionType.Credit));
        Assert.That(transaction.Amount, Is.EqualTo(0m));
    }

    [Test]
    public void Constructor_HandlesInvalidAmount_DefaultsToZero()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Amount = "invalid",
            Date = "01/15/2024"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.That(transaction.Amount, Is.EqualTo(0m));
    }

    [Test]
    public void Constructor_HandlesInvalidBalance_DefaultsToZero()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Amount = "100",
            Balance = "not a number",
            Date = "01/15/2024"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.That(transaction.Balance, Is.EqualTo(0m));
    }

    [Test]
    public void Constructor_HandlesInvalidDate_DefaultsToMinValue()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Amount = "100",
            Date = "invalid date"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.That(transaction.Date, Is.EqualTo(DateTime.MinValue));
    }

    [Test]
    public void Constructor_GeneratesUniqueTransactionId()
    {
        // Arrange
        var parsedItem1 = new MockParsedTransactionItem { Amount = "100", Date = "01/15/2024" };
        var parsedItem2 = new MockParsedTransactionItem { Amount = "100", Date = "01/15/2024" };

        // Act
        var transaction1 = new Transaction(parsedItem1);
        var transaction2 = new Transaction(parsedItem2);

        // Assert
        Assert.That(transaction1.TransactionId, Is.Not.EqualTo(transaction2.TransactionId));
        Assert.That(transaction1.TransactionId, Is.Not.Null.And.Not.Empty);
    }

    [Test]
    public void Constructor_PreservesCategory()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Category = "Entertainment",
            Amount = "100",
            Date = "01/15/2024"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.That(transaction.Category, Is.EqualTo("Entertainment"));
    }

    [Test]
    public void Constructor_PreservesIsRecurring_ForDebit()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            IsRecurring = true,
            Amount = "-100",
            Date = "01/15/2024"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.That(transaction.IsRecurring, Is.True);
    }

    [Test]
    public void Constructor_SetsIsRecurringNull_ForCredit()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            IsRecurring = true,
            Amount = "100",
            Date = "01/15/2024"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert — credits never have a recurring designation
        Assert.That(transaction.IsRecurring, Is.Null);
    }

    [Test]
    public void Constructor_HandlesLargeAmounts()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Amount = "999999.99",
            Date = "01/15/2024"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.That(transaction.Amount, Is.EqualTo(999999.99m));
    }

    [Test]
    public void Constructor_HandlesTinyAmounts()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Amount = "0.01",
            Date = "01/15/2024"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.That(transaction.Amount, Is.EqualTo(0.01m));
    }

    [Test]
    public void Constructor_HandlesNullDescription()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Description = null,
            Amount = "100",
            Date = "01/15/2024"
        };

        // Act
        var transaction = new Transaction(parsedItem);

        // Assert
        Assert.That(transaction.Description, Is.Null);
    }

    #endregion

    #region ToString Tests

    [Test]
    public void ToString_FormatsTransactionCorrectly()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Description = "Test Payment",
            Amount = "123.45",
            Date = "01/15/2024"
        };
        var transaction = new Transaction(parsedItem);

        // Act
        var result = transaction.ToString();

        // Assert
        Assert.That(result, Contains.Substring("1/15/2024"));
        Assert.That(result, Contains.Substring("$123.45"));
        Assert.That(result, Contains.Substring("Test Payment"));
    }

    [Test]
    public void ToString_HandlesNegativeAmount()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Description = "Withdrawal",
            Amount = "-50.00",
            Date = "02/20/2024"
        };
        var transaction = new Transaction(parsedItem);

        // Act
        var result = transaction.ToString();

        // Assert
        Assert.That(result, Contains.Substring("$-50"));
    }

    [Test]
    public void ToString_HandlesNullDescription()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem
        {
            Description = null,
            Amount = "100",
            Date = "01/15/2024"
        };
        var transaction = new Transaction(parsedItem);

        // Act
        var result = transaction.ToString();

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Contains.Substring("$100"));
    }

    #endregion

    #region Property Tests

    [Test]
    public void Category_CanBeSetAfterConstruction()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem { Amount = "100", Date = "01/15/2024" };
        var transaction = new Transaction(parsedItem);

        // Act
        transaction.Category = "NewCategory";

        // Assert
        Assert.That(transaction.Category, Is.EqualTo("NewCategory"));
    }

    [Test]
    public void IsRecurring_CanBeModifiedAfterConstruction()
    {
        // Arrange
        var parsedItem = new MockParsedTransactionItem { Amount = "100", Date = "01/15/2024", IsRecurring = false };
        var transaction = new Transaction(parsedItem);

        // Act
        transaction.IsRecurring = true;

        // Assert
        Assert.That(transaction.IsRecurring, Is.True);
    }

    #endregion

    #region Helper Classes

    private class MockParsedTransactionItem : IParsedTransactionItem
    {
        public string? Description { get; set; }
        public bool? IsRecurring { get; set; }
        public string? Amount { get; set; }
        public string? Category { get; set; }
        public string? Date { get; set; }
        public string? Balance { get; set; }
    }

    #endregion
}
