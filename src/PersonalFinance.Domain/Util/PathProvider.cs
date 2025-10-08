namespace PersonalFinance.Domain.Util;

public class PathProvider
{

    #region Properties

    public string CategoryModelTrainingDataFilePath { get; private set; } = string.Empty;
    public string DownloadedStatementFilePath { get; private set; } =  string.Empty;
    public string FinancesPath { get; private set; } =  string.Empty;

    #endregion

    #region Public Methods

    public (string Error, bool IsValid) Validate()
    {
        string? oneDrivePath = Environment.GetEnvironmentVariable("OneDrive");
        if (oneDrivePath == null)
        {
            return ("OneDrive environment variable is not found.", false);
        }

        FinancesPath = Path.Combine(oneDrivePath, "Documents", "personal", "finances");
        if (!Directory.Exists(FinancesPath))
        {
            return ($"Directory {FinancesPath} not found", false);
        }

        CategoryModelTrainingDataFilePath = Path.Combine(FinancesPath, "category-model-training-data.csv");
        if (!File.Exists(CategoryModelTrainingDataFilePath))
        {
            return ($"File {CategoryModelTrainingDataFilePath} not found", false);
        }

        string userProfilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string downloadsPath = Path.Combine(userProfilePath, "Downloads");
        if (!Directory.Exists(downloadsPath))
        {
            return ($"Directory {downloadsPath} not found", false);
        }

        DownloadedStatementFilePath = Path.Combine(downloadsPath, "stmt.csv");
        if (!File.Exists(DownloadedStatementFilePath))
        {
            return ($"File {DownloadedStatementFilePath} not found.", false);
        }
        
        return (string.Empty, true);
    }

    #endregion
    
}