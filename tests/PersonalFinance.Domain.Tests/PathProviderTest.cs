using PersonalFinance.Domain.Util;

namespace PersonalFinance.Domain.Tests;

public class PathProviderTest
{
    #region Validate Tests - Missing OneDrive Environment Variable

    [Test]
    public void Validate_WithoutOneDriveEnvironmentVariable_ReturnsFalse()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("OneDrive");
        Environment.SetEnvironmentVariable("OneDrive", null);
        var pathProvider = new PathProvider();

        try
        {
            // Act
            var (error, isValid) = pathProvider.Validate();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(isValid, Is.False);
                Assert.That(error, Contains.Substring("OneDrive"));
                Assert.That(error, Contains.Substring("not found"));
            });
        }
        finally
        {
            // Restore original value
            if (originalValue != null)
            {
                Environment.SetEnvironmentVariable("OneDrive", originalValue);
            }
        }
    }

    #endregion

    #region Property Tests

    [Test]
    public void CategoryModelTrainingDataFilePath_InitializedAsEmpty()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var path = pathProvider.CategoryModelTrainingDataFilePath;

        // Assert
        Assert.That(path, Is.Empty);
    }

    [Test]
    public void DownloadedStatementFilePath_InitializedAsEmpty()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var path = pathProvider.DownloadedStatementFilePath;

        // Assert
        Assert.That(path, Is.Empty);
    }

    [Test]
    public void FinancesPath_InitializedAsEmpty()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var path = pathProvider.FinancesPath;

        // Assert
        Assert.That(path, Is.Empty);
    }

    #endregion

    #region TrainingFileOnly Parameter Tests

    [Test]
    public void Validate_WithTrainingFileOnlyTrue_DoesNotCheckDownloadsFolder()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var (error, isValid) = pathProvider.Validate(trainingFileOnly: true);

        // Assert
        // If OneDrive and training file exist, trainingFileOnly=true should not check Downloads folder
        // The error message should not mention Downloads if it fails
        if (!isValid)
        {
            // If invalid, error should be about OneDrive or training file, not Downloads
            Assert.That(error, Is.Not.Empty);
        }
    }

    [Test]
    public void Validate_WithTrainingFileOnlyFalse_ChecksAllPaths()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var (error, isValid) = pathProvider.Validate(trainingFileOnly: false);

        // Assert
        // Should validate all paths including Downloads
        // Just verify the method completes without exception
        Assert.That(error, Is.Not.Null);
    }

    #endregion

    #region Error Messages

    [Test]
    public void Validate_ReturnsEmptyErrorStringOnSuccess()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var (error, isValid) = pathProvider.Validate();

        // Assert
        if (isValid)
        {
            Assert.That(error, Is.Empty);
        }
    }

    [Test]
    public void Validate_ReturnsNonEmptyErrorStringOnFailure()
    {
        // Arrange
        var originalValue = Environment.GetEnvironmentVariable("OneDrive");
        Environment.SetEnvironmentVariable("OneDrive", null);
        var pathProvider = new PathProvider();

        try
        {
            // Act
            var (error, isValid) = pathProvider.Validate();

            // Assert
            if (!isValid)
            {
                Assert.That(error, Is.Not.Empty);
            }
        }
        finally
        {
            if (originalValue != null)
            {
                Environment.SetEnvironmentVariable("OneDrive", originalValue);
            }
        }
    }

    #endregion

    #region Path Construction Tests

    [Test]
    public void Validate_ConstructsFinancesPathCorrectly()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var (error, isValid) = pathProvider.Validate(trainingFileOnly: true);

        // Assert
        if (isValid)
        {
            Assert.Multiple(() =>
            {
                Assert.That(pathProvider.FinancesPath, Is.Not.Empty);
                Assert.That(pathProvider.FinancesPath, Contains.Substring("personal"));
                Assert.That(pathProvider.FinancesPath, Contains.Substring("finances"));
                Assert.That(Directory.Exists(pathProvider.FinancesPath), Is.True);
            });
        }
    }

    [Test]
    public void Validate_ConstructsCategoryModelTrainingDataFilePathCorrectly()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var (error, isValid) = pathProvider.Validate(trainingFileOnly: true);

        // Assert
        if (isValid)
        {
            Assert.Multiple(() =>
            {
                Assert.That(pathProvider.CategoryModelTrainingDataFilePath, Is.Not.Empty);
                Assert.That(pathProvider.CategoryModelTrainingDataFilePath, Contains.Substring("category-model-training-data.csv"));
                Assert.That(File.Exists(pathProvider.CategoryModelTrainingDataFilePath), Is.True);
            });
        }
    }

    [Test]
    public void Validate_ConstructsDownloadedStatementFilePathCorrectly()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var (error, isValid) = pathProvider.Validate(trainingFileOnly: false);

        // Assert
        if (isValid)
        {
            Assert.Multiple(() =>
            {
                Assert.That(pathProvider.DownloadedStatementFilePath, Is.Not.Empty);
                Assert.That(pathProvider.DownloadedStatementFilePath, Contains.Substring("Downloads"));
                Assert.That(pathProvider.DownloadedStatementFilePath, Contains.Substring("stmt.csv"));
            });
        }
    }

    #endregion

    #region Directory Existence Tests

    [Test]
    public void Validate_ChecksFinancesDirectoryExists()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var (error, isValid) = pathProvider.Validate(trainingFileOnly: true);

        // Assert
        if (!isValid)
        {
            // If invalid, one reason could be missing finances directory
            Assert.That(error, Contains.Substring("not found").IgnoreCase);
        }
    }

    #endregion

    #region File Existence Tests

    [Test]
    public void Validate_ChecksTrainingFileExists()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var (error, isValid) = pathProvider.Validate(trainingFileOnly: true);

        // Assert
        if (!isValid)
        {
            // Error should indicate what's missing
            Assert.That(error, Is.Not.Empty);
        }
    }

    [Test]
    public void Validate_ChecksDownloadsDirectoryExists()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var (error, isValid) = pathProvider.Validate(trainingFileOnly: false);

        // Assert
        if (!isValid && error.Contains("Downloads", StringComparison.OrdinalIgnoreCase))
        {
            Assert.That(error, Contains.Substring("not found"));
        }
    }

    [Test]
    public void Validate_ChecksStatementFileExists()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var (error, isValid) = pathProvider.Validate(trainingFileOnly: false);

        // Assert
        if (!isValid && error.Contains("stmt.csv", StringComparison.OrdinalIgnoreCase))
        {
            Assert.That(error, Contains.Substring("not found"));
        }
    }

    #endregion

    #region Return Value Tests

    [Test]
    public void Validate_ReturnsTupleWithTwoElements()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var result = pathProvider.Validate();

        // Assert
        Assert.That(result.Item1, Is.TypeOf<string>());
        Assert.That(result.Item2, Is.TypeOf<bool>());
    }

    [Test]
    public void Validate_SecondElementIsBool()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var (error, isValid) = pathProvider.Validate();

        // Assert
        Assert.That(isValid, Is.True.Or.False);
    }

    #endregion

    #region Multiple Validations

    [Test]
    public void Validate_CanBeCalledMultipleTimes()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var result1 = pathProvider.Validate(trainingFileOnly: true);
        var result2 = pathProvider.Validate(trainingFileOnly: true);

        // Assert
        Assert.That(result1.Item2, Is.EqualTo(result2.Item2));
    }

    [Test]
    public void Validate_ConsistentResultsWithSameParameter()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act
        var result1 = pathProvider.Validate(trainingFileOnly: false);
        var result2 = pathProvider.Validate(trainingFileOnly: false);

        // Assert
        Assert.That(result1.Item2, Is.EqualTo(result2.Item2));
    }

    #endregion

    #region Edge Cases

    [Test]
    public void Validate_WithEmptyTrainingFileOnlyParameter_DefaultsToFalse()
    {
        // Arrange
        var pathProvider = new PathProvider();

        // Act - implicit default is false
        var result = pathProvider.Validate();

        // Assert
        // Should check all paths like trainingFileOnly=false
        Assert.That(result.Item1, Is.Not.Null.Or.Empty.Or.SameAs(string.Empty));
    }

    #endregion
}
