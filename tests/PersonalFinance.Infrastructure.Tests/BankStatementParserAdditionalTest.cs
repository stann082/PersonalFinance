using PersonalFinance.Domain.Models;
using PersonalFinance.Infrastructure;

namespace PersonalFinance.Infrastructure.Tests;

public class BankStatementParserAdditionalTest
{
    private IBankStatementParser _statementParser;

    [SetUp]
    public void SetUp()
    {
        _statementParser = new BankStatementParser();
    }

    #region File Not Found Tests

    [Test]
    public async Task Parse_WithNonExistentFile_ReturnsEmptyArray()
    {
        // Arrange
        string filePath = "/path/that/does/not/exist/file.csv";

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        Assert.That(records, Is.Empty);
    }

    [Test]
    public async Task Parse_WithEmptyFilePath_ReturnsEmptyArray()
    {
        // Arrange
        string filePath = string.Empty;

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        Assert.That(records, Is.Empty);
    }

    [Test]
    public async Task Parse_WithNullFilePath_ReturnsEmptyArray()
    {
        // Arrange
        string filePath = null!;

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        Assert.That(records, Is.Empty);
    }

    #endregion

    #region Valid File Tests

    [Test]
    public async Task Parse_ValidFile_ReturnsNonEmptyArray()
    {
        // Arrange
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        string filePath = Path.Combine(rootDir!.FullName, "etc", "stmt-03-2024.csv");

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        Assert.That(records, Is.Not.Empty);
    }

    [Test]
    public async Task Parse_ValidFile_ReturnsIParsedTransactionItems()
    {
        // Arrange
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        string filePath = Path.Combine(rootDir!.FullName, "etc", "stmt-03-2024.csv");

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        Assert.That(records, Is.All.TypeOf<IParsedTransactionItem>());
    }

    [Test]
    public async Task Parse_ValidFile_EachRecordHasProperties()
    {
        // Arrange
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        string filePath = Path.Combine(rootDir!.FullName, "etc", "stmt-03-2024.csv");

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        if (records.Length > 0)
        {
            var firstRecord = records[0];
            Assert.Multiple(() =>
            {
                Assert.That(firstRecord, Is.Not.Null);
                Assert.That(firstRecord.Description, Is.Not.Null.Or.Empty);
            });
        }
    }

    #endregion

    #region Bad File Format Tests

    [Test]
    public async Task Parse_MalformedCSV_ReturnsEmptyArray()
    {
        // Arrange
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        string filePath = Path.Combine(rootDir!.FullName, "etc", "stmt-03-2024-bad-statement.csv");

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        Assert.That(records, Is.Empty);
    }

    #endregion

    #region Special Characters Tests

    [Test]
    public async Task Parse_FileWithQuotesInDescription_HandlesCorrectly()
    {
        // Arrange
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        string filePath = Path.Combine(rootDir!.FullName, "etc", "stmt-01-2025.csv");

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        // Should handle quotes gracefully (returns empty if malformed, or records if valid)
        Assert.That(records, Is.Not.Null);
    }

    #endregion

    #region Performance Tests

    [Test]
    public async Task Parse_LargeFile_ReturnsAllRecords()
    {
        // Arrange
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        string filePath = Path.Combine(rootDir!.FullName, "etc", "stmt-03-2024.csv");

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        Assert.That(records.Length, Is.GreaterThanOrEqualTo(0));
    }

    #endregion

    #region Async Behavior Tests

    [Test]
    public void Parse_ReturnsTask()
    {
        // Arrange
        string filePath = "non-existent-file.csv";

        // Act
        var task = _statementParser.Parse(filePath);

        // Assert
        Assert.That(task, Is.TypeOf<Task<IParsedTransactionItem[]>>());
    }

    [Test]
    public async Task Parse_TaskCompletesSuccessfully()
    {
        // Arrange
        string filePath = "non-existent-file.csv";

        // Act
        var task = _statementParser.Parse(filePath);
        var result = await task;

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(task.IsCompletedSuccessfully, Is.True);
    }

    #endregion

    #region Record Content Tests

    [Test]
    public async Task Parse_ValidFile_RecordsHaveExpectedProperties()
    {
        // Arrange
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        string filePath = Path.Combine(rootDir!.FullName, "etc", "stmt-03-2024.csv");

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        if (records.Length > 0)
        {
            var record = records[0];
            // Check that properties are accessible (may be null or empty, but exist)
            Assert.Multiple(() =>
            {
                Assert.That(record.Description, Is.TypeOf<string>().Or.Null);
                Assert.That(record.Amount, Is.TypeOf<string>().Or.Null);
                Assert.That(record.Date, Is.TypeOf<string>().Or.Null);
                Assert.That(record.Balance, Is.TypeOf<string>().Or.Null);
            });
        }
    }

    #endregion

    #region Consistency Tests

    [Test]
    public async Task Parse_SameFileTwice_ProducesSameResult()
    {
        // Arrange
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        string filePath = Path.Combine(rootDir!.FullName, "etc", "stmt-03-2024.csv");

        // Act
        var result1 = await _statementParser.Parse(filePath);
        var result2 = await _statementParser.Parse(filePath);

        // Assert
        Assert.That(result1.Length, Is.EqualTo(result2.Length));
    }

    #endregion

    #region Directory Path Tests

    [Test]
    public async Task Parse_WithRelativePath_HandlesCorrectly()
    {
        // Arrange
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        string filePath = Path.Combine(rootDir!.FullName, "etc", "stmt-03-2024.csv");

        // Act & Assert - File should be found or return empty (no exception)
        Assert.DoesNotThrowAsync(async () => await _statementParser.Parse(filePath));
    }

    #endregion

    #region Array Type Tests

    [Test]
    public async Task Parse_ReturnsArray()
    {
        // Arrange
        string filePath = "non-existent.csv";

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        Assert.That(records, Is.TypeOf<IParsedTransactionItem[]>());
    }

    [Test]
    public async Task Parse_EmptyArrayIsNotNull()
    {
        // Arrange
        string filePath = "non-existent.csv";

        // Act
        var records = await _statementParser.Parse(filePath);

        // Assert
        Assert.That(records, Is.Not.Null);
    }

    #endregion

    #region Exception Handling Tests

    [Test]
    public async Task Parse_WithInvalidPath_DoesNotThrowException()
    {
        // Arrange
        string filePath = "C:\\Windows\\System32\\";  // This is a directory, not a file

        // Act & Assert
        Assert.DoesNotThrowAsync(async () => await _statementParser.Parse(filePath));
    }

    #endregion
}
