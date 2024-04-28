namespace PersonalFinance.Infrastructure.Tests;

public class BankOfAmericaStatementParserTest
{

    private IBankOfAmericaStatementParser _statementParser;
    
    [SetUp]
    public void SetUp()
    {
        _statementParser = new BankOfAmericaStatementParser();
    }
    
    [Test]
    public async Task BlueSkyTest_Parse()
    {
        // setup
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        string filePath = Path.Combine(rootDir!.FullName, "etc", "stmt-03-2024.csv");
        
        // exercise
        var records = await _statementParser.Parse(filePath);
        
        // post-conditions
        Assert.That(records, Has.Length.EqualTo(5));
    }
    
    [Test]
    public async Task BlueSkyTest_Parse_BadFile()
    {
        // setup
        DirectoryInfo? rootDir = new DirectoryInfo(Directory.GetCurrentDirectory()).Parent?.Parent?.Parent;
        string filePath = Path.Combine(rootDir!.FullName, "etc", "stmt-03-2024-bad-statement.csv");
        
        // exercise
        var records = await _statementParser.Parse(filePath);
        
        // post-conditions
        Assert.That(records, Is.Empty);
    }
    
}