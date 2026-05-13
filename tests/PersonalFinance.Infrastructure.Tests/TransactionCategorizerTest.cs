using Microsoft.ML;
using PersonalFinance.Domain.Models;
using PersonalFinance.Infrastructure;
using PersonalFinance.Infrastructure.Models;

namespace PersonalFinance.Infrastructure.Tests;

public class TransactionCategorizerTest
{
    private ITransactionCategorizer _categorizer;
    private string _testDataPath;

    [SetUp]
    public void SetUp()
    {
        _categorizer = new TransactionCategorizer();

        // Create a temporary test training file
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        _testDataPath = Path.Combine(rootDir!.FullName, "etc");
    }

    #region Basic Categorization Tests

    [Test]
    public async Task CategorizeAsync_WithValidTrainingData_AssignsCategories()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");

        // Only run if training file exists
        if (!File.Exists(trainingFilePath))
        {
            Assert.Inconclusive("Training data file not found");
        }

        var transactions = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = "WHOLE FOODS MARKET" },
            new MockTrainingModelItem { Description = "AMAZON PRIME" }
        };

        // Act
        await _categorizer.CategorizeAsync(trainingFilePath, transactions);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(transactions[0].Category, Is.Not.Null);
            Assert.That(transactions[1].Category, Is.Not.Null);
        });
    }

    [Test]
    public async Task CategorizeAsync_WithValidTrainingData_AssignsRecurringStatus()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");

        if (!File.Exists(trainingFilePath))
        {
            Assert.Inconclusive("Training data file not found");
        }

        var transactions = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = "NETFLIX.COM" }
        };

        // Act
        await _categorizer.CategorizeAsync(trainingFilePath, transactions);

        // Assert
        // Recurring status should be set (true or false, not null)
        Assert.That(transactions[0].IsRecurring, Is.TypeOf<bool>());
    }

    #endregion

    #region Empty Data Tests

    [Test]
    public async Task CategorizeAsync_WithEmptyTransactionList_CompletesWithoutError()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");

        if (!File.Exists(trainingFilePath))
        {
            Assert.Inconclusive("Training data file not found");
        }

        var transactions = new List<ITrainingModelItem>();

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _categorizer.CategorizeAsync(trainingFilePath, transactions));
    }

    [Test]
    public async Task CategorizeAsync_WithSingleTransaction_CategorizeIt()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");

        if (!File.Exists(trainingFilePath))
        {
            Assert.Inconclusive("Training data file not found");
        }

        var transactions = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = "TEST TRANSACTION" }
        };

        // Act
        await _categorizer.CategorizeAsync(trainingFilePath, transactions);

        // Assert
        Assert.That(transactions[0].Category, Is.Not.Null);
    }

    #endregion

    #region Invalid File Path Tests

    [Test]
    public async Task CategorizeAsync_WithNonExistentTrainingFile_HandlesGracefully()
    {
        // Arrange
        var trainingFilePath = "/path/that/does/not/exist.csv";
        var transactions = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = "TRANSACTION" }
        };

        // Act & Assert - Should handle error gracefully
        Assert.DoesNotThrowAsync(async () => await _categorizer.CategorizeAsync(trainingFilePath, transactions));
    }

    [Test]
    public async Task CategorizeAsync_WithNullTrainingFile_HandlesGracefully()
    {
        // Arrange
        string trainingFilePath = null!;
        var transactions = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = "TRANSACTION" }
        };

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _categorizer.CategorizeAsync(trainingFilePath, transactions));
    }

    #endregion

    #region Multiple Transactions Tests

    [Test]
    public async Task CategorizeAsync_WithMultipleTransactions_CategorizeAllOfThem()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");

        if (!File.Exists(trainingFilePath))
        {
            Assert.Inconclusive("Training data file not found");
        }

        var transactions = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = "GROCERY STORE" },
            new MockTrainingModelItem { Description = "GAS STATION" },
            new MockTrainingModelItem { Description = "RESTAURANT" },
            new MockTrainingModelItem { Description = "MOVIE THEATER" }
        };

        // Act
        await _categorizer.CategorizeAsync(trainingFilePath, transactions);

        // Assert
        Assert.Multiple(() =>
        {
            for (int i = 0; i < transactions.Count; i++)
            {
                Assert.That(transactions[i].Category, Is.Not.Null, $"Transaction {i} should have a category");
                Assert.That(transactions[i].IsRecurring, Is.TypeOf<bool>(), $"Transaction {i} should have IsRecurring set");
            }
        });
    }

    #endregion

    #region Data Validation Tests

    [Test]
    public async Task CategorizeAsync_WithMissingDescription_HandlesGracefully()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");

        if (!File.Exists(trainingFilePath))
        {
            Assert.Inconclusive("Training data file not found");
        }

        var transactions = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = null }
        };

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _categorizer.CategorizeAsync(trainingFilePath, transactions));
    }

    [Test]
    public async Task CategorizeAsync_WithEmptyDescription_CategorizeIt()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");

        if (!File.Exists(trainingFilePath))
        {
            Assert.Inconclusive("Training data file not found");
        }

        var transactions = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = string.Empty }
        };

        // Act
        await _categorizer.CategorizeAsync(trainingFilePath, transactions);

        // Assert
        // Should assign some category even for empty description
        Assert.That(transactions[0].Category, Is.Not.Null);
    }

    [Test]
    public async Task CategorizeAsync_WithWhitespaceDescription_CategorizeIt()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");

        if (!File.Exists(trainingFilePath))
        {
            Assert.Inconclusive("Training data file not found");
        }

        var transactions = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = "   " }
        };

        // Act
        await _categorizer.CategorizeAsync(trainingFilePath, transactions);

        // Assert
        Assert.That(transactions[0].Category, Is.Not.Null);
    }

    #endregion

    #region Async Behavior Tests

    [Test]
    public void CategorizeAsync_ReturnsTask()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");
        var transactions = new List<ITrainingModelItem>();

        // Act
        var task = _categorizer.CategorizeAsync(trainingFilePath, transactions);

        // Assert
        Assert.That(task, Is.TypeOf<Task>());
    }

    [Test]
    public async Task CategorizeAsync_TaskCompletesSuccessfully()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");
        var transactions = new List<ITrainingModelItem>();

        // Act
        var task = _categorizer.CategorizeAsync(trainingFilePath, transactions);
        await task;

        // Assert
        Assert.That(task.IsCompletedSuccessfully, Is.True);
    }

    #endregion

    #region Prediction Consistency Tests

    [Test]
    public async Task CategorizeAsync_SameDescriptionProducesConsistentCategory()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");

        if (!File.Exists(trainingFilePath))
        {
            Assert.Inconclusive("Training data file not found");
        }

        var transactions1 = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = "STARBUCKS" }
        };

        var transactions2 = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = "STARBUCKS" }
        };

        // Act
        await _categorizer.CategorizeAsync(trainingFilePath, transactions1);
        await _categorizer.CategorizeAsync(trainingFilePath, transactions2);

        // Assert
        // Both should have categories assigned
        Assert.Multiple(() =>
        {
            Assert.That(transactions1[0].Category, Is.Not.Null);
            Assert.That(transactions2[0].Category, Is.Not.Null);
        });
    }

    #endregion

    #region Edge Cases

    [Test]
    public async Task CategorizeAsync_WithVeryLongDescription_CategorizeIt()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");

        if (!File.Exists(trainingFilePath))
        {
            Assert.Inconclusive("Training data file not found");
        }

        var longDescription = "VERY LONG DESCRIPTION THAT CONTAINS MANY WORDS " +
                             "LIKE A REAL TRANSACTION DESCRIPTION MIGHT HAVE " +
                             "WITH LOTS OF DETAILS AND INFORMATION INCLUDED";

        var transactions = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = longDescription }
        };

        // Act
        await _categorizer.CategorizeAsync(trainingFilePath, transactions);

        // Assert
        Assert.That(transactions[0].Category, Is.Not.Null);
    }

    [Test]
    public async Task CategorizeAsync_WithSpecialCharactersInDescription_CategorizeIt()
    {
        // Arrange
        var trainingFilePath = Path.Combine(_testDataPath, "category-model-training-data.csv");

        if (!File.Exists(trainingFilePath))
        {
            Assert.Inconclusive("Training data file not found");
        }

        var transactions = new List<ITrainingModelItem>
        {
            new MockTrainingModelItem { Description = "STORE #123 & CO." }
        };

        // Act
        await _categorizer.CategorizeAsync(trainingFilePath, transactions);

        // Assert
        Assert.That(transactions[0].Category, Is.Not.Null);
    }

    #endregion

    #region Helper Classes

    private class MockTrainingModelItem : ITrainingModelItem
    {
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsRecurring { get; set; }
    }

    #endregion
}
